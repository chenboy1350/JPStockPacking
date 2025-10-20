namespace JPStockPacking.Models
{
    public class BreakAndLostFilterModel
    {
        public string LotNo { get; set; } = string.Empty;
        public int[] BreakIDs { get; set; } = [];
        public int[] LostIDs { get; set; } = [];
        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string ReceiveNo { get; set; } = string.Empty;
        public string BreakDescription { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
