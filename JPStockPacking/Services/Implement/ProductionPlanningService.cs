using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class ProductionPlanningService(JPDbContext jPDbContext, SPDbContext sPDbContext) : IProductionPlanningService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<CustomerGroup>> GetCustomerGroupsAsync()
        {
            var customerGroups = await _sPDbContext.CustomerGroup
                .Where(cg => cg.IsActive)
                .Select(cg => new CustomerGroup
                {
                    CustomerGroupId = cg.CustomerGroupId,
                    Name = cg.Name
                })
                .ToListAsync();
            return customerGroups;
        }

        public async Task<List<ProductType>> GetProductionTypeAsync()
        {
            var customerGroups = await _sPDbContext.ProductType
                .Where(cg => cg.IsActive)
                .Select(cg => new ProductType
                {
                    ProductTypeId = cg.ProductTypeId,
                    Name = cg.Name
                })
                .ToListAsync();
            return customerGroups;
        }

        public async Task<List<PackMethod>> GetPackMethodsAsync()
        {
            var packMethods = await _sPDbContext.PackMethod
                .Where(pm => pm.IsActive)
                .Select(pm => new PackMethod
                {
                    PackMethodId = pm.PackMethodId,
                    Name = pm.Name
                })
                .ToListAsync();
            return packMethods;
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


        public double CalLotOperateDay(int TtQty)
        {
            if (TtQty > 0)
            {
                ProductionPlanningCalculator productionPlanningCalculator = new();
                var productionPlan = productionPlanningCalculator.CalculateProductionPlan(TtQty);
                return productionPlan.ActualProductionDays;
            }
            else
            {
                return 0;
            }
        }

        public async Task GetOperateOrderToPlan(DateTime? FromDate = null, DateTime? ToDate = null)
        {
            var orders = from o in _sPDbContext.Order
                         join l in _sPDbContext.Lot on o.OrderNo equals l.OrderNo
                         select new { l, seldate = o.SeldDate1 };

            if (FromDate != null)
            {
                orders = orders.Where(o => o.seldate >= FromDate);
            }

            if (ToDate != null)
            {
                orders = orders.Where(o => o.seldate <= ToDate);
            }

            var orderList = await orders.ToListAsync();

            var TotalOrder = orderList.DistinctBy(x => x.l.OrderNo).ToList().Count;

            var TotalQty = orderList.Sum(o => o.l.TtQty);
            var TotalLots = orderList.Count;

            var TotalOperateDays = orderList.Sum(o => o.l.OperateDays);

            var TotalWorker = 30; // Assume a fixed number of workers for this example

            Console.WriteLine($"Total Quantity to Plan: {TotalQty}");
        }
    }
}

