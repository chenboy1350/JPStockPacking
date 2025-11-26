using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IComparedInvoiceService
    {
        Task<List<ComparedInvoiceModel>> GetFilteredInvoice(ComparedInvoiceFilterModel comparedInvoiceFilterModel);
    }
}
