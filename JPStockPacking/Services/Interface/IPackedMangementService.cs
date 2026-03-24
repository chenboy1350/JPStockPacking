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
        Task<BaseResponseModel> ConfirmToSendShowroomAsync(string[] lotNos, string userId);
        Task<List<ConfirmPreviewItem>> GetPreviewForConfirmAsync(string[] lotNos, string type);
        Task<List<TempPack>> GetAllDocToPrint(string[] lotNos, string userid);
        Task<List<TempPack>> GetDocToPrintByType(string[] lotNos, string userid, string sendType);
        Task<List<TempPack>> GetDocToPrintByReceiveNo(string receiveNo, string sendType, string userid);
        Task UpdateArticleAsync(string orderNo);
        Task UpdateOrderSuccessAsync(string orderNo);
    }
}
