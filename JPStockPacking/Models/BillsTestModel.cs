namespace JPStockPacking.Models
{
    public class BillsTestModel : JobDeductResult
    {
        public int No { get; set; }

        public byte[]? ImageBytes { get; set; }
        public string? ImagePath { get; set; }
    }
}
