using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IPISService
    {
        Task<List<EmployeeWithDepartmentModel>?> GetEmployeeAsync();
    }
}
