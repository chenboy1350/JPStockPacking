using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IOrderManagementService
    {
        void ImportOrder();
        List<CustomOrder> GetOrderAndLot();
    }
}
