using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface ILostService
    {
        Task<List<LostAndRepairModel>> GetLostAsync(BreakAndLostFilterModel breakAndLostFilterModel);
        Task AddLostAsync(string lotNo, double lostQty, int leaderID);
        Task PintedLostReport(int[]? LostIDs);
        Task<List<AssignedWorkTableModel>> GetTableLeaderAsync(string LotNo);
    }
}
