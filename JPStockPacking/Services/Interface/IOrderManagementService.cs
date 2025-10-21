using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Interface
{
    public interface IOrderManagementService
    {
        Task ImportOrderAsync(string orderNo);
        Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo);
        Task<CustomLot?> GetCustomLotAsync(string lotNo);
        Task<ScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string custCode, DateTime fromDate, DateTime toDate);
        Task<List<WorkTable>> GetTableAsync();
        Task<List<TableModel>> GetTableToReturnAsync(string LotNo);
        Task<List<ReceivedListModel>> GetRecievedToReturnAsync(string LotNo, int TableID);
        Task ReturnReceivedAsync(string LotNo, int[] assignmentIDs, decimal returnQty);
        Task UpdateAllReceivedItemsAsync(string receiveNo);
        Task<UserModel> ValidateApporverAsync(string username, string password);

    }
}
