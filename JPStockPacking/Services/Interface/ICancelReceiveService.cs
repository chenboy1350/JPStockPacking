using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface ICancelReceiveService
    {
        Task<List<ReceivedListModel>> GetTopSJ1JPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetSJ1JPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetTopSJ2JPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetSJ2JPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetTopSendLostReceivedAsync(string? receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetSendLostReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetTopExportReceivedAsync(string? receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetExportReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo);
        Task<BaseResponseModel> CancelSJ1ByReceiveNoAsync(string receiveNo, int userId);
        Task<BaseResponseModel> CancelSJ1ByLotNoAsync(string receiveNo, string[] lotNos, int userId);
        Task<BaseResponseModel> CancelSJ2ByReceiveNoAsync(string receiveNo, int userId);
        Task<BaseResponseModel> CancelSJ2ByLotNoAsync(string receiveNo, string[] lotNos, int userId);
        Task<BaseResponseModel> CancelSendLostByReceiveNoAsync(string receiveNo, int userId);
        Task<BaseResponseModel> CancelSendLostByLotNoAsync(string receiveNo, string[] lotNos, int userId);
        Task<BaseResponseModel> CancelExportByReceiveNoAsync(string receiveNo, int userId);
        Task<BaseResponseModel> CancelExportByLotNoAsync(string receiveNo, string[] lotNos, int userId);
    }
}
