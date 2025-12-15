using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IReturnService
    {
        Task<BaseResponseModel> ReturnReceivedAsync(string lotNo, int[] assignmentIDs, decimal returnQty);
        Task<List<ReceivedListModel>> GetRecievedToReturnAsync(string LotNo, int TableID);
        Task<List<TableModel>> GetTableToReturnAsync(string LotNo);
    }
}
