using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using static JPStockPacking.Services.Helper.Enum;
using Enum = JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Interface
{
    public interface IOrderManagementService
    {
        Task ImportOrderAsync();
        Task GetUpdateLotAsync();
        Task UpdateReceivedItemsAsync(string lotNo, string[] receivedNo);
        Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo);
        Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo);
        Task<CustomLot?> GetCustomLotAsync(string lotNo);
        Task<ScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string custCode, DateTime fromDate, DateTime toDate);
        Task<List<WorkTable>> GetTableAsync();
        Task<List<TableMemberModel>> GetTableMemberAsync(int tableID);
        Task AssignReceivedAsync(string lotNo, string[] receivedNo, string tableId, string[] memberIds);
    }
}
