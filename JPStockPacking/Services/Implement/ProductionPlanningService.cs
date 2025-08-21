using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;

namespace JPStockPacking.Services.Implement
{
    public class ProductionPlanningService : IProductionPlanningService
    {
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
