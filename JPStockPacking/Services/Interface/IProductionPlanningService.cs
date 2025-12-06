using JPStockPacking.Data.SPDbContext.Entities;

namespace JPStockPacking.Services.Interface
{
    public interface IProductionPlanningService
    {
        Task<List<CustomerGroup>> GetCustomerGroupsAsync();
        Task<List<ProductType>> GetProductionTypeAsync();
        Task<List<PackMethod>> GetPackMethodsAsync();
        Task RegroupCustomer();
        double CalLotOperateDay(int TtQty);
        DateTime FindAvailableStartDate(double totalWorkHours, DateTime deadline, Dictionary<DateTime, double> usedPerDay, double capacity);
    }
}
