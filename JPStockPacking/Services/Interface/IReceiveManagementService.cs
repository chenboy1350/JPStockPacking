using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IReceiveManagementService
    {
        Task<List<ReceivedListModel>> GetTopJPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo);
        Task<List<ReceivedListModel>> GetJPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo);
        Task UpdateLotItemsAsync(string receiveNo, string[] orderNos, int[] receiveIds);
        Task CancelUpdateLotItemsAsync(string receiveNo, string[] orderNos, int[] receiveIds);
        Task<List<Lot>> GetJPLotAsync(List<Order> newOrders);
    }
}
