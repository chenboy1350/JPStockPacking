namespace JPStockPacking.Models
{
    public class FormulaModel
    {
        public int FormulaID { get; set; } = 0;
        public int CustomerGroupID { get; set; } = 0;
        public string CustomerGroup { get; set; } = string.Empty;
        public int PackMethodID { get; set; } = 0;
        public string PackMethod { get; set; } = string.Empty;
        public int ProductTypeID { get; set; } = 0;
        public string ProductType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Items { get; set; } = 0;
        public double P1 { get; set; } = 0.0;
        public double P2 { get; set; } = 0.0;
        public double Avg { get; set; } = 0.0;
        public double ItemPerSec { get; set; } = 0.0;
        public bool IsActive { get; set; } = false;
    }
}
