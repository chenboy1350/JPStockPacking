using JPStockPacking.Data.BMDbContext;
using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.Models;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class ProductionPlanningService(JPDbContext jPDbContext, SPDbContext sPDbContext, BMDbContext bMDbContext) : IProductionPlanningService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly BMDbContext _bMDbContext = bMDbContext;

        public async Task<List<OrderPlanModel>> GetOrderToPlan(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                var result =  from ord in _sPDbContext.Order
                                    join lot in _sPDbContext.Lot on ord.OrderNo equals lot.OrderNo into lotGroup
                                    where lotGroup.Any(lg => lg.OperateDays > 0 && !lg.IsSuccess)
                                          && ord.SeldDate1.HasValue // กรองเฉพาะที่มี DueDate
                                          && (FromDate == null || ord.SeldDate1 >= FromDate)
                                          && (ToDate == null || ord.SeldDate1 <= ToDate)
                                    let validLots = lotGroup.Where(lg => lg.OperateDays > 0 && !lg.IsSuccess)
                                    select new OrderPlanModel
                                    {
                                        OrderNo = ord.OrderNo,
                                        CustCode = ord.CustCode ?? string.Empty,
                                        CustomerGroup = 5,
                                        Article = validLots.FirstOrDefault()!.Article ?? string.Empty,
                                        ProdType = validLots.FirstOrDefault()!.EdesArt ?? string.Empty,
                                        Qty = validLots.Sum(lot => lot.TtQty ?? 0),
                                        SendToPackQty = lotGroup.Sum(lot => lot.ReceivedQty ?? 0),
                                        OperateDay = validLots.Sum(lot => lot.OperateDays ?? 0),
                                        DueDate = ord.SeldDate1.Value // ปลอดภัยเพราะ filter ด้านบนแล้ว
                                    };

                var resultList = await result.ToListAsync();

                // กรอง order ที่ Qty = 0 ออก
                resultList = [.. resultList.Where(r => r.Qty > 0)];
                if (resultList.Count == 0) return resultList;

                // Batch load ข้อมูลทั้งหมดเพื่อหลีกเลี่ยง N+1 Query
                var lookupData = await LoadLookupDataAsync(resultList);
                EnrichOrderPlanData(resultList, lookupData);

                return resultList;
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        private async Task<OrderPlanLookupData> LoadLookupDataAsync(List<OrderPlanModel> orders)
        {
            var custCodes = orders.Select(r => r.CustCode).Distinct().ToHashSet();

            // ดึง CustomerGroup ทั้งหมด แล้ว filter ใน memory
            var allCustomerGroups = await _sPDbContext.MappingCustomerGroup.ToListAsync();
            var customerGroups = allCustomerGroups
                .Where(cg => custCodes.Contains(cg.CustCode))
                .ToDictionary(cg => cg.CustCode, cg => cg.CustomerGroupId);

            // ดึง ProductType ทั้งหมด (มีไม่กี่รายการ)
            var productTypes = await _sPDbContext.ProductType
                .ToDictionaryAsync(pt => pt.Name?.Trim() ?? string.Empty, pt => pt.BaseTime);

            return new OrderPlanLookupData(customerGroups, productTypes);
        }

        private void EnrichOrderPlanData(List<OrderPlanModel> orders, OrderPlanLookupData lookupData)
        {
            var calculator = new ProductionPlanningCalculator();

            foreach (var item in orders)
            {
                if (lookupData.CustomerGroups.TryGetValue(item.CustCode, out var groupId))
                    item.CustomerGroup = groupId;

                item.BaseTime = CalculateBaseTime(item, lookupData);

                item.SendToPackOperateDay = item.SendToPackQty > 0
                    ? calculator.CalculateProductionPlan((int)item.SendToPackQty, item.BaseTime).ActualProductionDays
                    : 0;
            }
        }

        private double CalculateBaseTime(OrderPlanModel item, OrderPlanLookupData lookupData)
        {
            double baseTime = 0;

            // N+1 query สำหรับ OrderDetail
            var detail = _bMDbContext.OrderDetail
                .FirstOrDefault(od => od.OrderNo == item.OrderNo && od.Article == item.Article);

            if (detail != null)
            {
                if (!string.IsNullOrEmpty(detail.Mask)) baseTime += 0.2;
                if (!string.IsNullOrEmpty(detail.Box)) baseTime += 0.2;
                if (!string.IsNullOrEmpty(detail.MaskAndBox)) baseTime += 0.3;
            }
            else
            {
                baseTime = 0.3;
            }

            var prodTypeKey = item.ProdType?.Trim() ?? string.Empty;
            if (lookupData.ProductTypes.TryGetValue(prodTypeKey, out var ptBaseTime) && ptBaseTime.HasValue)
                baseTime += ptBaseTime.Value;

            return baseTime;
        }

        private record OrderPlanLookupData(
            Dictionary<string, int> CustomerGroups,
            Dictionary<string, double?> ProductTypes);

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

