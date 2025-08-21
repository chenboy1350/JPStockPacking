namespace JPStockPacking.Services.Interface
{
    public interface INotificationService
    {
        Task OrderMarkAsReadAsync(string orderNo);
    }
}
