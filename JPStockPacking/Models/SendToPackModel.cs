namespace JPStockPacking.Models
{
    public class SendToPackModel
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string SCountry { get; set; } = string.Empty;
        public string Special { get; set; } = string.Empty;
        public double SumTtQty { get; set; } = 0;
        public string SumTtPrice { get; set; } = string.Empty;
        public double SumSendTtQty { get; set; } = 0;
        public string SumSendTtPrice { get; set; } = string.Empty;
        public bool IsOrderDefined { get; set; } = false;
        public int Persentage { get; set; } = 0;
        public List<SendToPackLots> Lots { get; set; } = [];
    }

    public class SendToPackLots
    {
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Tunit { get; set; } = string.Empty;
        public string EdesFn { get; set; } = string.Empty;
        public string TdesFn { get; set; } = string.Empty;
        public string TdesArt { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public decimal QtySi { get; set; } = 0;
        public decimal SendTtQty { get; set; } = 0;
        public decimal TtQtyToPack { get; set; } = 0;
        public bool IsDefined { get; set; } = false;
        public bool IsUnderQuota { get; set; } = false;
        public string ApproverID { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public int Persentage { get; set; } = 0;
        public string EnPrice { get; set; } = string.Empty;
        public string EnTtPrice { get; set; } = string.Empty;
        public double DePrice { get; set; } = 0;
        public double DeTtPrice { get; set; } = 0;
        public string EnSendQtyPrice { get; set; } = string.Empty;
        public string EnSendQtyTtPrice { get; set; } = string.Empty;
        public double DeSendQtyPrice { get; set; } = 0;
        public double DeSendQtyTtPrice { get; set; } = 0;
        public List<Size> Size { get; set; } = [];
    }

    public class Size
    {
        public string S { get; set; } = string.Empty;
        public string CS { get; set; } = string.Empty;
        public decimal Q { get; set; } = 0;
        public decimal TtQtyToPack { get; set; } = 0;
        public bool IsDefined { get; set; } = false;
        public bool IsUnderQuota { get; set; } = false;
        public string ApproverID { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public int Persentage { get; set; } = 0;

    }

    public class DefineToPackRequest
    {
        public string OrderNo { get; set; } = string.Empty;
        public List<LotToPackDTO> Lots { get; set; } = [];
    }

    public class LotToPackDTO
    {
        public string LotNo { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public int LotApprover { get; set; } = 0;
        public List<SizeToPackDTO> Sizes { get; set; } = [];
    }

    public class SizeToPackDTO
    {
        public int SizeIndex { get; set; }
        public decimal TtQty { get; set; }
        public int SizeApprover { get; set; } = 0;
    }

}
