using JPStockPacking.Data.SPDbContext.Entities;

namespace JPStockPacking.Models
{
    public class ReceivedToImportList
    {
        public string ReceiveNo { get; set; } = string.Empty;
        public List<Received> ReceivedLots { get; set; } = [];
    }
}
