using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface ICheckQtyToSendService
    {
        Task<SendToPackModel> GetOrderToSendQtyAsync(string orderNo);
        Task<SendToPackModel> GetOrderToSendQtyWithPriceAsync(string orderNo);
        Task DefineToPackAsync(string orderNo, List<LotToPackDTO> lots);
    }
}
