namespace JPStockPacking.Models
{
    public class AssignedWorkTableModel
    {
        public int AssignmentId { get; set; } = 0;
        public string TableName { get; set; } = string.Empty;
        public int LeaderTableID { get; set; } = 0;
        public string LeaderName { get; set; } = string.Empty;
    }
}
