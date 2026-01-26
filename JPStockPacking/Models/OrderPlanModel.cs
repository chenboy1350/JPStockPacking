namespace JPStockPacking.Data.Models
{
    public class OrderPlanModel
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public int CustomerGroup { get; set; } = 0;
        public string Article { get; set; } = string.Empty;
        public decimal Qty { get; set; } = 0;
        public decimal SendToPackQty { get; set; } = 0;
        public double OperateDay { get; set; } = 0;
        public double SendToPackOperateDay { get; set; } = 0;
        public DateTime DueDate { get; set; }
        public string ProdType { get; set; } = string.Empty;
        public double BaseTime { get; set; } = 0;
    }
}