namespace JPStockPacking.Models
{
    public class SendStockInput
    {
        public SendStockInput()
        {
            LotNo = string.Empty;
            BillNumber = 0;
            SizeNoOrd = false;
            ItemSend = "01";
            Unallocated = 0;
            KsQty = 0;
            KsWg = 0;
            KmQty = 0;
            KmWg = 0;
            KmDes = 0;
            KlQty = 0;
            KlWg = 0;
            KxQty = 0;
            KxWg = 0;
            ReturnFound = false;
            UserId = string.Empty;
        }

        public string LotNo { get; set; } = string.Empty;
        public int BillNumber { get; set; } = 0;
        public bool SizeNoOrd { get; set; } = false;
        public string ItemSend { get; set; } = string.Empty;

        public decimal Unallocated { get; set; } = 0;

        public decimal KsQty { get; set; } = 0;
        public decimal KsWg { get; set; } = 0;

        public decimal KmQty { get; set; } = 0;
        public decimal KmWg { get; set; } = 0;
        public int KmDes { get; set; } = 0;

        public decimal KlQty { get; set; } = 0;
        public decimal KlWg { get; set; } = 0;

        public decimal KxQty { get; set; } = 0;
        public decimal KxWg { get; set; } = 0;
        public int Approver { get; set; } = 0;

        public bool ReturnFound { get; set; } = false;
        public string UserId { get; set; } = string.Empty;
    }

    public class OrderToStoreModel
    {
        public string OrderNo { get; set; } = string.Empty;
        public string CustCode { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public decimal TtQty { get; set; } = 0;
        public double TtWg { get; set; } = 0;
        public decimal Si { get; set; } = 0;
        public decimal SendPack_Qty { get; set; } = 0;
        public decimal SendToPack_Qty { get; set; } = 0;
        public decimal Packed_Qty { get; set; } = 0;

        public decimal Store_Qty { get; set; } = 0;
        public double Store_Wg { get; set; } = 0;
        public decimal Store_FixedQty { get; set; } = 0;
        public double Store_FixedWg { get; set; } = 0;
        public bool IsStoreSended { get; set; } = false;
        public bool IsStored { get; set; } = false;

        public decimal Melt_Qty { get; set; } = 0;
        public double Melt_Wg { get; set; } = 0;
        public int BreakDescriptionId { get; set; } = 0;
        public decimal Melt_FixedQty { get; set; } = 0;
        public double Melt_FixedWg { get; set; } = 0;
        public bool IsMeltSended { get; set; } = false;
        public bool IsMelted { get; set; } = false;

        public decimal Lost_Qty { get; set; } = 0;
        public double Lost_Wg { get; set; } = 0;
        public decimal Lost_FixedQty { get; set; } = 0;
        public double Lost_FixedWg { get; set; } = 0;
        public bool IsLostSended { get; set; } = false;
        public bool IsLosted { get; set; } = false;

        public decimal Export_Qty { get; set; } = 0;
        public double Export_Wg { get; set; } = 0;
        public decimal Export_FixedQty { get; set; } = 0;
        public double Export_FixedWg { get; set; } = 0;
        public bool IsExportSended { get; set; } = false;
        public bool HasDraftExport { get; set; } = false;

        public decimal Percentage { get; set; } = 0;
    }

    public sealed class SaveResult
    {
        public string ReceiveNo { get; init; } = "";
        public int InsertedDetails { get; init; }
        public int UpdatedSendStockRows { get; init; }
        public int UpdatedJobBills { get; init; }
    }
}
