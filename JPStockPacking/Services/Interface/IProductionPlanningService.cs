namespace JPStockPacking.Services.Interface
{
    public interface IProductionPlanningService
    {
        double CalLotOperateDay(int TtQty);
        DateTime FindAvailableStartDate(double totalWorkHours, DateTime deadline, Dictionary<DateTime, double> usedPerDay, double capacity);
    }
}
