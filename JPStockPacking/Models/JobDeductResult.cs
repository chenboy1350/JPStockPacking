namespace JPStockPacking.Models
{
    public class JobDeductResult
    {
        public string CustCode { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string Docno { get; set; } = string.Empty;
        public string Doc_no { get; set; } = string.Empty;
        public string JobBarcode { get; set; } = string.Empty;
        public decimal R1 { get; set; } = 0;
        public decimal R2 { get; set; } = 0;
        public decimal R3 { get; set; } = 0;
        public decimal R4 { get; set; } = 0;
        public decimal R5 { get; set; } = 0;
        public decimal DeductQty { get; set; } = 0;
        public string Num { get; set; } = string.Empty;
        public DateTime MDate { get; set; } = DateTime.MinValue;
        public int RunDeduct { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }
}
