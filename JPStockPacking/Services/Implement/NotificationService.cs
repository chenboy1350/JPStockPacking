using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class NotificationService(SPDbContext sPDbContext) : INotificationService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task OrderMarkAsReadAsync(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo)) return;

            var orderNotify = await _sPDbContext.OrderNotify.FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (orderNotify is null) return;

            orderNotify.IsNew = false;
            orderNotify.UpdateDate = DateTime.Now;

            await _sPDbContext.SaveChangesAsync();
        }

    }
}
