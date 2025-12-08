using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IFormulaManagementService
    {
        Task<List<FormulaModel>> GetFormula(Formula formula);
        Task<BaseResponseModel> AddNewFormula(Formula formula);
        Task<BaseResponseModel> UpdateFormula(Formula formula);
        Task<BaseResponseModel> ToggleFormulaStatus(int formulaId);
    }
}
