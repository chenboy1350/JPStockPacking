using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface ICheckQtyToSendService
    {
        Task<SendToPackModel> GetOrderToSendQtyAsync(string orderNo, int? userid);
        Task<SendToPackModel> GetOrderToSendQtyWithPriceAsync(string orderNo, int? userid);
        Task DefineToPackAsync(string orderNo, List<LotToPackDTO> lots);
    }
}
