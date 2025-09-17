using JPStockPacking.Models;
using JPStockPacking.Services.Implement;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Interface
{
    public interface IReportService
    {
        byte[] GenerateSendQtyToPackReport(SendToPackModel model, PrintTo printTo);
    }
}
