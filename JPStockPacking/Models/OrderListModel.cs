namespace JPStockPacking.Models
{
    public class CustomOrder
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public DateTime FactoryDate { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime SeldDate1 { get; set; }
        public DateTime OrdDate { get; set; }
        public int TotalLot { get; set; } = 0;
        public int CompleteLot { get; set; } = 0;
        public bool IsSuccess { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public DateTime StartDate { get; set; }
        public int OperateDays { get; set; } = 0;
        public List<CustomLot> CustomLot { get; set; } = [];
    }

    public class CustomLot
    {
        public string LotNo { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string CustPcode { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public string Article { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string TdesArt { get; set; } = string.Empty;
        public string MarkCenter { get; set; } = string.Empty;
        public string SaleRem { get; set; } = string.Empty;
        public decimal ReceivedQty { get; set; }
        public int AssignTo { get; set; } = 0;
        public bool IsPacked { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public DateTime UpdateDate { get; set; }
    }
}
