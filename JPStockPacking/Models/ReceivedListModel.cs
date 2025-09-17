namespace JPStockPacking.Models
{
    public class ReceivedListModel
    {
        public int ReceivedID { get; set; } = 0;
        public string ReceiveNo { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public double TtWg { get; set; } = 0;
        public string Article { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string CustPCode { get; set; } = string.Empty;
        public int AssignmentID { get; set; } = 0;
        public bool IsReceived { get; set; } = false;
        public bool HasRevButNotAll { get; set; } = false;
        public string Mdate { get; set; } = string.Empty;

    }
}
