namespace JPStockPacking.Models
{
    public class SendLostCheckModel
    {
        public string Customer { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public double Wg { get; set; }
    }
}
