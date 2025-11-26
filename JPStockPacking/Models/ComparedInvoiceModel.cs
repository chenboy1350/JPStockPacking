using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Models
{
    public class ComparedInvoiceFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public InvoiceType InvoiceType { get; set; }
    }

    public class ComparedInvoiceModel
    {
        public string SPInvoiceNo { get; set; } = string.Empty;
        public string SPOrderNo { get; set; } = string.Empty;
        public string SPArticle { get; set; } = string.Empty;
        public double SPTtQty { get; set; } = 0;
        public double SPPrice { get; set; } = 0;
        public double SPTotalPrice { get; set; } = 0;
        public double SPTotalSetTtQty { get; set; } = 0;

        public string JPInvoiceNo { get; set; } = string.Empty;
        public string JPOrderNo { get; set; } = string.Empty;
        public string JPArticle { get; set; } = string.Empty;
        public double JPTtQty { get; set; } = 0;
        public double JPPrice { get; set; } = 0;
        public double JPTotalPrice { get; set; } = 0;
        public double JPTotalSetTtQty { get; set; } = 0;

        public bool IsMatched { get; set; } = false;
        public string CustCode { get; set; } = string.Empty;
        public string MakeUnit { get; set; } = string.Empty;
    }
}
