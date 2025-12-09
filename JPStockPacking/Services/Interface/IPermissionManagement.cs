using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IPermissionManagement
    {
        Task<List<Permission>> GetPermissionAsync();
        Task<List<UserModel>> GetUserAsync();
        Task<List<MappingPermission>> GetMappingPermissionAsync(int UserID);
        Task<BaseResponseModel> UpdatePermissionAsync(UpdatePermissionModel model);
    }
}
