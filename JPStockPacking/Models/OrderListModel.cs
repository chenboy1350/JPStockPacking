namespace JPStockPacking.Models
{
    public class CustomOrder
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public DateTime FactoryDate { get; set; }
        public string FactoryDateTH { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string SeldDate1 { get; set; } = string.Empty;
        public string OrdDate { get; set; } = string.Empty;
        public int TotalLot { get; set; } = 0;
        public int SumTtQty { get; set; } = 0;
        public int CompleteLot { get; set; } = 0;
        public bool IsSuccess { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public string StartDateTH { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int PackDaysRemain { get; set; } = 0;
        public int ExportDaysRemain { get; set; } = 0;
        public double OperateDays { get; set; } = 0;
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
        public decimal ReturnedQty { get; set; }
        public List<AssignedWorkTableModel> AssignTo { get; set; } = [];
        public bool IsSuccess { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public string UpdateDate { get; set; } = string.Empty;
        public bool IsAllReceived { get; set; } = false;
        public bool IsPacking { get; set; } = false;
        public bool HasRepair { get; set; } = false;
        public bool HasLost { get; set; } = false;
        public bool IsAllReturned { get; set; } = false;
        public bool IsAllAssigned { get; set; } = false;
        public bool IsRemainNotAssign { get; set; } = false;
        public bool IsRemainNotReturn { get; set; } = false;
        public List<TableMemberModel> TableMembers { get; set; } = [];
        public string FileName { get; set; } = string.Empty;
    }

    public class PagedScheduleListModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public List<CustomOrder> Data { get; set; } = [];
    }
}
