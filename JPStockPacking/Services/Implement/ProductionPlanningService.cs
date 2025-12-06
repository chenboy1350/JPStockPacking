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

        public DateTime FindAvailableStartDate(double totalWorkHours, DateTime deadline, Dictionary<DateTime, double> usedPerDay, double capacity)
        {
            for (int offset = 0; offset <= 30; offset++)
            {
                var candidateStart = deadline.AddDays(-offset);

                while (candidateStart.DayOfWeek == DayOfWeek.Saturday || candidateStart.DayOfWeek == DayOfWeek.Sunday)
                {
                    candidateStart = candidateStart.AddDays(-1);
                }

                bool fits = true;
                double hoursRemaining = totalWorkHours;
                var day = candidateStart;

                while (hoursRemaining > 0)
                {
                    if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                    {
                        var used = usedPerDay.TryGetValue(day, out var u) ? u : 0;
                        var available = capacity - used;
                        if (available <= 0)
                        {
                            fits = false;
                            break;
                        }
                        var hoursToUse = Math.Min(available, hoursRemaining);
                        hoursRemaining -= hoursToUse;
                    }
                    day = day.AddDays(-1);
                }

                if (fits)
                    return day.AddDays(1); // the day after the last used in planning
            }

            // fallback if no fit
            var fallback = deadline.AddDays(-1);
            while (fallback.DayOfWeek == DayOfWeek.Saturday || fallback.DayOfWeek == DayOfWeek.Sunday)
            {
                fallback = fallback.AddDays(-1);
            }
            return fallback;
        }
    }
}
