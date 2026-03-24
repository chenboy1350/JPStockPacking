namespace JPStockPacking.Models
{
    public class ConfirmPreviewItem
    {
        public string LotNo { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string SendType { get; set; } = string.Empty;
        public decimal TtQty { get; set; }
        public decimal TtWg { get; set; }
    }
}
