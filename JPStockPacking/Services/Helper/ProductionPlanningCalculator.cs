namespace JPStockPacking.Services.Helper
{
    public class ProductionPlanningCalculator
    {
        private const double MINUTES_PER_HOUR = 60.0;
        private const double WORKING_HOURS_PER_DAY = 8.5;

        private static readonly int workersCount = 30;

        public ProductionPlan CalculateProductionPlan(int TtQty, double BaseTime)
        {
            double totalAverageTimePerPieceSeconds = BaseTime;
            double totalMinutes = TtQty * totalAverageTimePerPieceSeconds;
            double totalHours = totalMinutes / MINUTES_PER_HOUR;
            double totalDays = totalHours / WORKING_HOURS_PER_DAY;
            double actualDays = totalDays / workersCount;

            return new ProductionPlan
            {
                TotalPiecesToProduce = TtQty,
                WorkersCount = workersCount,
                TotalAverageTimePerPieceSeconds = totalAverageTimePerPieceSeconds,
                TotalProductionMinutes = totalMinutes,
                TotalProductionHours = totalHours,
                TotalProductionDays = totalDays,
                ActualProductionDays = actualDays
            };
        }
    }

    public class WorkStep
    {
        public string StepName { get; set; } = string.Empty;
        public int PiecesTimedCount { get; set; }
        public double Person1TimeSeconds { get; set; }
        public double Person2TimeSeconds { get; set; }
    }

    public class StepResult
    {
        public string StepName { get; set; } = string.Empty;
        public int PiecesTimedCount { get; set; }
        public double Person1TimeSeconds { get; set; }
        public double Person2TimeSeconds { get; set; }
        public double AverageTimePerPieceSeconds { get; set; }
    }

    public class ProductionPlan
    {
        public int TotalPiecesToProduce { get; set; }
        public int WorkersCount { get; set; }
        public double TotalAverageTimePerPieceSeconds { get; set; }
        public double TotalProductionMinutes { get; set; }
        public double TotalProductionHours { get; set; }
        public double TotalProductionDays { get; set; }
        public double ActualProductionDays { get; set; }
    }
}
