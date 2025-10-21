using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IBreakService
    {
        Task<List<LostAndRepairModel>> GetBreakAsync(BreakAndLostFilterModel breakAndLostFilterModel);
        Task AddBreakAsync(string lotNo, double breakQty, int breakDes);
        Task<List<BreakDescription>> GetBreakDescriptionsAsync();
        Task<List<BreakDescription>> AddNewBreakDescription(string breakDescription);
        Task PintedBreakReport(int[]? BreakIDs);
    }
}
