namespace JPStockPacking.Models
{
    public class ReceivedListModel
    {
        public int ReceivedID { get; set; } = 0;
        public string ReceiveNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public double TtWg { get; set; } = 0;
        public string Article { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string EdesFn { get; set; } = string.Empty;
        public string CustPCode { get; set; } = string.Empty;
        public int AssignmentID { get; set; } = 0;
        public bool IsReceived { get; set; } = false;
        public bool IsCancel { get; set; } = false;
        public bool HasRevButNotAll { get; set; } = false;
        public string Mdate { get; set; } = string.Empty;
        public string CreateDateTH { get; set; } = string.Empty;

    }

    public class TableModel
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
    }

    public class LostAndRepairModel : ReceivedListModel
    {
        public int LostID { get; set; } = 0;
        public int BreakID { get; set; } = 0;
        public decimal? BreakQty { get; set; }
        public decimal? LostQty { get; set; }
        public string BreakDescription { get; set; } = string.Empty;
        public string SeldDate1 { get; set; } = string.Empty;
        public decimal? PreviousQty { get; set; }
        public decimal? OrderQty { get; set; }
        public bool IsReported { get; set; } = false;
        public DateTime CreateDate { get; set; }
        public int LeaderID { get; set; } = 0;
    }
}
