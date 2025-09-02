namespace JPStockPacking.Models
{
    public class ScheduleListModel
    {
        public List<Day> Days { get; set; } = [];
        public List<Week> Weeks { get; set; } = [];
    }

    public class Day
    {
        public string Title { get; set; } = string.Empty;
        public List<CustomOrder> Orders { get; set; } = [];
    }

    public class Week
    {
        public string Title { get; set; } = string.Empty;
        public List<Day> Orders { get; set; } = [];
    }

    public class CustomOrder
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public string FactoryDate { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string SeldDate1 { get; set; } = string.Empty;
        public string OrdDate { get; set; } = string.Empty;
        public int TotalLot { get; set; } = 0;
        public int SumTtQty { get; set; } = 0;
        public int CompleteLot { get; set; } = 0;
        public bool IsSuccess { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public string StartDateTH { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int PackDaysRemain { get; set; } = 0;
        public int ExportDaysRemain { get; set; } = 0;
        public int OperateDays { get; set; } = 0;
        public bool IsReceivedLate { get; set; } = false;
        public bool IsPackingLate { get; set; } = false;
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
        public decimal PackedQty { get; set; }
        public string AssignTo { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public string UpdateDate { get; set; } = string.Empty;
        public bool IsAllReceived { get; set; } = false;
        public bool IsAllPacking { get; set; } = false;
        public bool IsPacking { get; set; } = false;
        public bool IsPacked { get; set; } = false;
    }
}
