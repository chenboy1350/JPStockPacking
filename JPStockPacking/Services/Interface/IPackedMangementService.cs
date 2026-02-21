using JPStockPacking.Models;
using static JPStockPacking.Services.Implement.PackedMangementService;

namespace JPStockPacking.Services.Interface
{
    public interface IPackedMangementService
    {
        Task<List<OrderToStoreModel>> GetOrderToStoreAsync(string orderNo);
        Task<OrderToStoreModel?> GetOrderToStoreByLotAsync(string lotNo);
        Task<BaseResponseModel> SendStockAsync(SendStockInput input);
        Task<BaseResponseModel> ConfirmToSendStoreAsync(string[] lotNos, string userId);
        Task<BaseResponseModel> ConfirmToSendMeltAsync(string[] lotNos, string userId);
        Task<BaseResponseModel> ConfirmToSendLostAsync(string[] lotNos, string userId);
        Task<BaseResponseModel> ConfirmToSendExportAsync(string[] lotNos, string userId);
        Task<List<TempPack>> GetAllDocToPrint(string[] lotNos, string userid);
        Task<List<TempPack>> GetDocToPrintByType(string[] lotNos, string userid, string sendType);
        Task UpdateArticleAsync(string orderNo);
        Task UpdateOrderSuccessAsync(string orderNo);
    }
}
