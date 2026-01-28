using JPStockPacking.Data.Models;
using System.Threading.Tasks;

namespace JPStockPacking.Services.Interface
{
    public interface IProductionPlanningService
    {
        Task RegroupCustomer();
        double CalLotOperateDay(int TtQty, string ProdType, string Article, string OrderNo);
        Task<List<OrderPlanModel>> GetOrderToPlan(DateTime FromDate, DateTime ToDate);
    }
}
