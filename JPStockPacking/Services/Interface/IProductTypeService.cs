using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IProductTypeService
    {
        Task<List<ProductType>> GetAllAsync();
        Task<ProductType?> GetByIdAsync(int id);
        Task<BaseResponseModel> AddAsync(ProductType model);
        Task<BaseResponseModel> UpdateAsync(ProductType model);
        Task<BaseResponseModel> ToggleStatusAsync(int id);
    }
}
