namespace JPStockPacking.Models
{
    public class TempPack
    {
        public string Name { get; set; } = string.Empty;
        public int EmpCode { get; set; } = 0;

        public string LotNo { get; set; } = string.Empty;
        public string JobBarcode { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string ArtCode { get; set; } = string.Empty;
        public string FnCode { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public string Doc { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public bool CheckBill { get; set; } = false;
        public string Username { get; set; } = string.Empty;
        public DateTime Mdate { get; set; }
        public bool SendPack { get; set; } = false;
        public string PackDoc { get; set; } = string.Empty;
        public string Num { get; set; } = string.Empty;
        public DateTime SendDate { get; set; }
        public bool SizeNoOrd { get; set; } = false;
        public bool Wg_Over { get; set; } = false;
        public int BillNumber { get; set; } = 0;

        public string SendType { get; set; } = string.Empty;
        public decimal OkTtl { get; set; } = 0;
        public decimal OkWg { get; set; } = 0;

        public decimal OkQ1 { get; set; } = 0;
        public decimal OkQ2 { get; set; } = 0;
        public decimal OkQ3 { get; set; } = 0;
        public decimal OkQ4 { get; set; } = 0;
        public decimal OkQ5 { get; set; } = 0;
        public decimal OkQ6 { get; set; } = 0;
        public decimal OkQ7 { get; set; } = 0;
        public decimal OkQ8 { get; set; } = 0;
        public decimal OkQ9 { get; set; } = 0;
        public decimal OkQ10 { get; set; } = 0;
        public decimal OkQ11 { get; set; } = 0;
        public decimal OkQ12 { get; set; } = 0;

        public string SUser { get; set; } = string.Empty;

        public string SizeZone { get; set; } = string.Empty;
        public bool ChkSize { get; set; } = false;
        public string S1 { get; set; } = string.Empty;
        public string S2 { get; set; } = string.Empty;
        public string S3 { get; set; } = string.Empty;
        public string S4 { get; set; } = string.Empty;
        public string S5 { get; set; } = string.Empty;
        public string S6 { get; set; } = string.Empty;
        public string S7 { get; set; } = string.Empty;
        public string S8 { get; set; } = string.Empty;
        public string S9 { get; set; } = string.Empty;
        public string S10 { get; set; } = string.Empty;
        public string S11 { get; set; } = string.Empty;
        public string S12 { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string ListGem { get; set; } = string.Empty;
        public string FinishingTH { get; set; } = string.Empty;
        public string FinishingEN { get; set; } = string.Empty;
        public decimal WgActual { get; set; } = 0;
        public string CustCode { get; set; } = string.Empty;

        public int NumSend { get; set; } = 0;
        public DateTime MdateSend { get; set; }

        public int COther1 { get; set; } = 0;

        public string CreatedBy { get; set; } = string.Empty;

        public string BreakDescription { get; set; } = string.Empty;
        public bool IsOverQouta { get; set; } = false;

        public decimal Unallocated { get; set; } = 0;
    }

    public class TempPackPage
    {
        public string Doc { get; set; } = string.Empty;
        public DateTime ListDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SendTo { get; set; } = string.Empty;
        public string SendType { get; set; } = string.Empty;
        public string OrderNoAndCusCode { get; set; } = string.Empty;
        public string Reporter { get; set; } = string.Empty;
        public List<TempPack> TempPacks { get; set; } = [];
    }
}
