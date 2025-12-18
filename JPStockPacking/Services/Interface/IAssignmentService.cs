using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IAssignmentService
    {
        Task<List<TableMemberModel>> GetTableMemberAsync(int tableID);
        Task SyncAssignmentsForTableAsync(string lotNo, int tableId, int[] receivedIds, string[] memberIds, bool hasPartTime, int workerNumber);
        Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo);
    }
}
