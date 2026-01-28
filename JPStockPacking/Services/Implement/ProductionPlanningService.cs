using JPStockPacking.Data.BMDbContext;
using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.Models;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace JPStockPacking.Services.Implement
{
    public class ProductionPlanningService(JPDbContext jPDbContext, SPDbContext sPDbContext, BMDbContext bMDbContext) : IProductionPlanningService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly BMDbContext _bMDbContext = bMDbContext;

        public async Task<List<OrderPlanModel>> GetOrderToPlan(DateTime FromDate, DateTime ToDate)
        {
            // Query หลัก (ที่ทำงานได้อยู่แล้ว)
            var result = await (from ord in _sPDbContext.Order
                                join lot in _sPDbContext.Lot on ord.OrderNo equals lot.OrderNo into lotGroup
                                where ord.SeldDate1 >= FromDate
                                      && ord.SeldDate1 <= ToDate
                                      && lotGroup.Any(lg => lg.OperateDays > 0)
                                let validLots = lotGroup.Where(lg => lg.OperateDays > 0)
                                select new OrderPlanModel
                                {
                                    OrderNo = ord.OrderNo,
                                    CustCode = ord.CustCode ?? string.Empty,
                                    CustomerGroup = 5, // ใส่ default ไว้ก่อน
                                    Article = validLots.FirstOrDefault()!.Article ?? string.Empty,
                                    ProdType = validLots.FirstOrDefault()!.EdesArt ?? string.Empty,
                                    Qty = validLots.Sum(lot => lot.TtQty ?? 0),
                                    SendToPackQty = validLots.Sum(lot => lot.ReceivedQty ?? 0),
                                    OperateDay = validLots.Sum(lot => lot.OperateDays ?? 0),
                                    DueDate = ord.SeldDate1 ?? DateTime.MinValue
                                }).ToListAsync();

            // ดึง CustomerGroup แยก แล้ว map ใน memory
            var custCodes = result.Select(r => r.CustCode).Distinct().ToList();
            var customerGroups = await _sPDbContext.MappingCustomerGroup
                .Where(cg => custCodes.Contains(cg.CustCode))
                .ToDictionaryAsync(cg => cg.CustCode, cg => cg.CustomerGroupId);

            // อัพเดท CustomerGroup
            foreach (var item in result)
            {
                if (customerGroups.TryGetValue(item.CustCode, out var groupId))
                {
                    item.CustomerGroup = groupId;
                }
            }

            foreach (var item in result)
            {
                double BaseTime = 0;
                var a = _bMDbContext.OrderDetail.FirstOrDefault(od => od.OrderNo == item.OrderNo && od.Article == item.Article);
                if (a != null)
                {
                    if (!string.IsNullOrEmpty(a.Mask))
                    {
                        BaseTime += 0.2;
                    }

                    if (!string.IsNullOrEmpty(a.Box))
                    {
                        BaseTime += 0.2;
                    }

                    if (!string.IsNullOrEmpty(a.MaskAndBox))
                    {
                        BaseTime += 0.3;
                    }
                }
                else
                {
                    BaseTime = 0.3;
                }


                var b = _sPDbContext.ProductType.FirstOrDefault(pt => pt.Name == item.ProdType.Trim());
                if (b != null && b.BaseTime.HasValue)
                {
                    BaseTime += b.BaseTime.Value;
                }

                item.BaseTime = BaseTime;
                item.SendToPackOperateDay = CalLotOperateDay((int)item.SendToPackQty, item.ProdType, item.Article, item.OrderNo);
            }

            return result;
        }

        public async Task RegroupCustomer()
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                List<string> custCodes = await _jPDbContext.OrdHorder
                    .Where(o => !string.IsNullOrEmpty(o.CustCode))
                    .Select(o => o.CustCode ?? string.Empty)
                    .Distinct()
                    .ToListAsync();

                if (custCodes.Count == 0)
                    return;

                var profiles = (
                    from code in custCodes
                    join p in _jPDbContext.CusProfile.AsNoTracking() on code equals p.CusCode
                    select p
                ).ToList();

                var existingGroups = await (
                    from mg in _sPDbContext.MappingCustomerGroup
                    join code in custCodes on mg.CustCode equals code
                    select mg
                ).ToDictionaryAsync(x => x.CustCode);

                var now = DateTime.UtcNow;

                foreach (var p in profiles)
                {
                    var groupId = GetCustomerGroup(p);

                    if (existingGroups.TryGetValue(p.CusCode, out var g))
                    {
                        g.CustomerGroupId = groupId;
                        g.UpdateDate = now;
                    }
                    else
                    {
                        _sPDbContext.MappingCustomerGroup.Add(new MappingCustomerGroup
                        {
                            CustCode = p.CusCode,
                            CustomerGroupId = groupId,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        });
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }
        }


        private static int GetCustomerGroup(CusProfile item)
        {
            bool credit = item.CreditTerm ?? false;
            bool deposit = item.DepositPay ?? false;
            bool vip = item.Remark?.Contains("VIP") ?? false;

            if (vip) return 1;

            if (credit && deposit) return 3;

            if (credit) return 2;

            if (deposit) return 4;

            return 5;
        }


        public double CalLotOperateDay(int TtQty, string ProdType, string Article, string OrderNo)
        {
            double BaseTime = 0;
            var a = _bMDbContext.OrderDetail.FirstOrDefault(od => od.OrderNo == OrderNo && od.Article == Article);
            if (a != null)
            {
                if (!string.IsNullOrEmpty(a.Mask))
                {
                    BaseTime += 0.2;
                }

                if (!string.IsNullOrEmpty(a.Box))
                {
                    BaseTime += 0.2;
                }

                if (!string.IsNullOrEmpty(a.MaskAndBox))
                {
                    BaseTime += 0.3;
                }
            }
            else
            {
                BaseTime = 0.3;
            }


            var b = _sPDbContext.ProductType.FirstOrDefault(pt => pt.Name == ProdType.Trim());
            if (b != null && b.BaseTime.HasValue)
            {
                BaseTime += b.BaseTime.Value;
            }

            if (TtQty > 0)
            {
                ProductionPlanningCalculator productionPlanningCalculator = new();
                var productionPlan = productionPlanningCalculator.CalculateProductionPlan(TtQty, BaseTime);
                return productionPlan.ActualProductionDays;
            }
            else
            {
                return 0;
            }
        }
    }
}

