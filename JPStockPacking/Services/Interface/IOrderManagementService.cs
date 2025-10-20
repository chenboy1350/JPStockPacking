using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using static JPStockPacking.Services.Helper.Enum;
using static JPStockPacking.Services.Implement.AuthService;
using static JPStockPacking.Services.Implement.OrderManagementService;
using Enum = JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Interface
{
    public interface IOrderManagementService
    {
        Task ImportOrderAsync(string orderNo);
        Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo);
        Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo);
        Task<CustomLot?> GetCustomLotAsync(string lotNo);
        Task<ScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string custCode, DateTime fromDate, DateTime toDate);
        Task<List<WorkTable>> GetTableAsync();
        Task<List<TableMemberModel>> GetTableMemberAsync(int tableID);
        Task AssignReceivedAsync(string lotNo, int[] receivedIDs, string tableId, string[] memberIds, bool hasPartTime, int WorkerNumber);
        Task<List<TableModel>> GetTableToReturnAsync(string LotNo);
        Task<List<ReceivedListModel>> GetRecievedToReturnAsync(string LotNo, int TableID);
        Task ReturnReceivedAsync(string LotNo, int[] assignmentIDs, decimal returnQty);
        Task UpdateAllReceivedItemsAsync(string receiveNo);
        Task<SendToPackModel> GetOrderToSendQtyAsync(string orderNo);
        Task<SendToPackModel> GetOrderToSendQtyWithPriceAsync(string orderNo);
        Task DefineToPackAsync(string orderNo, List<LotToPackDTO> lots);
        Task<UserModel> ValidateApporverAsync(string username, string password);
        Task<List<BreakDescription>> GetBreakDescriptionsAsync();
        Task<List<BreakDescription>> AddNewBreakDescription(string breakDescription);
        Task<List<LostAndRepairModel>> GetBreakAsync(BreakAndLostFilterModel breakAndLostFilterModel);
        Task<List<LostAndRepairModel>> GetLostAsync(BreakAndLostFilterModel breakAndLostFilterModel);
        Task AddLostAsync(string lotNo, double lostQty);
        Task AddBreakAsync(string lotNo, double breakQty, int breakDes);
        Task PintedBreakReport(int[]? BreakIDs);
        Task PintedLostReport(int[]? LostIDs);
    }
}
