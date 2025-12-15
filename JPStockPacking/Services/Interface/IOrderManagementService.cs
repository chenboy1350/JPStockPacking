using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Interface
{
    public interface IOrderManagementService
    {
        Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo);
        Task<CustomLot?> GetCustomLotAsync(string lotNo);
        Task<PagedScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string lotNo, string custCode, DateTime fromDate, DateTime toDate, int page, int pageSize);
        Task<List<WorkTable>> GetTableAsync();
        Task UpdateAllReceivedItemsAsync(string receiveNo);
        Task<UserModel> ValidateApporverAsync(string username, string password);
        Task ImportOrderAsync();

    }
}
