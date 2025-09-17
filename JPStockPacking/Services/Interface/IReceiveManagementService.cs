using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IReceiveManagementService
    {
        Task<List<ReceivedListModel>> GetTopJPReceivedAsync(string? receiveNo);
        Task<List<ReceivedListModel>> GetJPReceivedByReceiveNoAsync(string receiveNo);
        Task UpdateLotItemsAsync(string receiveNo, string[] orderNos, int[] receiveIds);
    }
}
