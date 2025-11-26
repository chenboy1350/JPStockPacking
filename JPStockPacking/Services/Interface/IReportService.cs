using JPStockPacking.Models;
using JPStockPacking.Services.Implement;
using static JPStockPacking.Services.Helper.Enum;
using static JPStockPacking.Services.Implement.AuthService;
using static JPStockPacking.Services.Implement.OrderManagementService;
using static JPStockPacking.Services.Implement.PackedMangementService;

namespace JPStockPacking.Services.Interface
{
    public interface IReportService
    {
        byte[] GenerateSendQtyToPackReport(SendToPackModel model, PrintTo printTo);
        byte[] GenerateBreakReport(List<LostAndRepairModel> model);
        byte[] GenerateLostReport(List<LostAndRepairModel> model, UserModel userModel);
        byte[] GenerateSenToReport(List<TempPack> model);
        byte[] GenerateComparedInvoiceReport(ComparedInvoiceFilterModel comparedInvoiceFilterModel,List<ComparedInvoiceModel> model, InvoiceType invoiceType);
    }
}
