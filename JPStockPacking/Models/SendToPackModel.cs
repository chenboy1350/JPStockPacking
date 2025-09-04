namespace JPStockPacking.Models
{
    public class SendToPackModel
    {
        public string LotNo { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public decimal SendTtQty { get; set; } = 0;
        public decimal TtQtyToPack { get; set; } = 0;
        public bool IsDefined { get; set; } = false;
    }

    public class DefineToPackRequest
    {
        public string OrderNo { get; set; } = string.Empty;
        public List<LotToPackDto> Lots { get; set; } = new();
    }

    public class LotToPackDto
    {
        public string LotNo { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }
}
