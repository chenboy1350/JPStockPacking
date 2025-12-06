using JPStockPacking.Models;
using JPStockPacking.Services.Implement;

namespace JPStockPacking.Services.Interface
{
    public interface IAuditService
    {
        Task<List<ComparedInvoiceModel>> GetFilteredInvoice(ComparedInvoiceFilterModel comparedInvoiceFilterModel);
        Task<List<UnallocatedQuantityModel>> GetUnallocatedQuentityToStore(ComparedInvoiceFilterModel comparedInvoiceFilterModel);
    }
}
