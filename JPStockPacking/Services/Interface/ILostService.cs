using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface ILostService
    {
        Task<List<LostAndRepairModel>> GetLostAsync(BreakAndLostFilterModel breakAndLostFilterModel);
        Task AddLostAsync(string lotNo, double lostQty);
        Task PintedLostReport(int[]? LostIDs);
    }
}
