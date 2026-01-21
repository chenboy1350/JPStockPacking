using JPStockPacking.Data.SPDbContext.Entities;

namespace JPStockPacking.Services.Interface
{
    public interface IProductionPlanningService
    {
        Task<List<CustomerGroup>> GetCustomerGroupsAsync();
        Task<List<ProductType>> GetProductionTypeAsync();
        Task RegroupCustomer();
        double CalLotOperateDay(int TtQty);
        Task GetOperateOrderToPlan(DateTime? StartDate = null, DateTime? EndDate = null);
    }
}
