namespace JPStockPacking.Services.Helper
{
    public class ProductionPlanningCalculator
    {
        private const double MINUTES_PER_HOUR = 60.0;
        private const double WORKING_HOURS_PER_DAY = 8.5;

        private static readonly int totalPiecesToProduce = 100000;
        private static readonly int workersCount = 5;

        private static readonly Dictionary<string, List<WorkStep>> workStepsTemplates = new()
        {
            ["CCJ82-C"] =
            [
                new() {
                    StepName = "Step1",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 1.13,
                    Person2TimeSeconds = 1.39
                },
                new() {
                    StepName = "Step2",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 1.34,
                    Person2TimeSeconds = 1.34
                },
                new() {
                    StepName = "Step3",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 1.48,
                    Person2TimeSeconds = 2.00
                },
                new() {
                    StepName = "Step4",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 1.19,
                    Person2TimeSeconds = 0.58
                },
                new() {
                    StepName = "Step5",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 0.58,
                    Person2TimeSeconds = 0.40
                }
            ],

            ["CCJ82-B"] =
            [
                new() {
                    StepName = "Step1",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 5.55,
                    Person2TimeSeconds = 7.25
                },
                new() {
                    StepName = "Step2",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 2.18,
                    Person2TimeSeconds = 2.10
                },
                new() {
                    StepName = "Step3",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 1.14,
                    Person2TimeSeconds = 1.13
                },
                new() {
                    StepName = "Step4",
                    PiecesTimedCount = 10,
                    Person1TimeSeconds = 0.10,
                    Person2TimeSeconds = 0.13
                }
            ],
        };

        public static double CalculateAverageTimePerPieceSeconds(WorkStep step)
        {
            double totalTimePerson1 = step.Person1TimeSeconds;
            double totalTimePerson2 = step.Person2TimeSeconds;
            double averageTotal = (totalTimePerson1 + totalTimePerson2) / 2.0;
            return averageTotal / step.PiecesTimedCount;
        }

        public ProductionPlan CalculateProductionPlan(int TtQty)
        {
            if (!workStepsTemplates.TryGetValue("CCJ82-C", out List<WorkStep>? selectedWorkSteps))
            {
                throw new ArgumentException($"WorkSteps template not found. Available templates: {string.Join(", ", workStepsTemplates.Keys)}");
            }

            var stepResults = selectedWorkSteps.Select(step => new StepResult
            {
                StepName = step.StepName,
                PiecesTimedCount = step.PiecesTimedCount,
                Person1TimeSeconds = step.Person1TimeSeconds,
                Person2TimeSeconds = step.Person2TimeSeconds,
                AverageTimePerPieceSeconds = CalculateAverageTimePerPieceSeconds(step)
            }).ToList();

            double totalAverageTimePerPieceSeconds = stepResults.Sum(x => x.AverageTimePerPieceSeconds);
            double totalMinutes = TtQty * totalAverageTimePerPieceSeconds;
            double totalHours = totalMinutes / MINUTES_PER_HOUR;
            double totalDays = totalHours / WORKING_HOURS_PER_DAY;
            double actualDays = totalDays / workersCount;

            return new ProductionPlan
            {
                WorkStepsTemplate = "CCJ82-C",
                TotalPiecesToProduce = TtQty,
                WorkersCount = workersCount,
                StepResults = stepResults,
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
        public string WorkStepsTemplate { get; set; } = string.Empty;
        public int TotalPiecesToProduce { get; set; }
        public int WorkersCount { get; set; }
        public List<StepResult> StepResults { get; set; } = [];
        public double TotalAverageTimePerPieceSeconds { get; set; }
        public double TotalProductionMinutes { get; set; }
        public double TotalProductionHours { get; set; }
        public double TotalProductionDays { get; set; }
        public double ActualProductionDays { get; set; }
    }
}
