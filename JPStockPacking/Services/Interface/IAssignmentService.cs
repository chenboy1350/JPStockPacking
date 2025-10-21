using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IAssignmentService
    {
        Task<List<TableMemberModel>> GetTableMemberAsync(int tableID);
        Task AssignReceivedAsync(string lotNo, int[] receivedIDs, string tableId, string[] memberIds, bool hasPartTime, int WorkerNumber);
        Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo);
    }
}
