using JPStockPacking.Models;
using static JPStockPacking.Services.Implement.AuthService;

namespace JPStockPacking.Services.Interface
{
    public interface IPISService
    {
        Task<List<ResEmployeeModel>?> GetEmployeeAsync();
        Task<List<ResEmployeeModel>?> GetAvailableEmployeeAsync();
        Task<List<DepartmentModel>?> GetDepartmentAsync();
        Task<UserModel> ValidateApproverAsync(string username, string password);
        Task<List<UserModel>> GetUser(ReqUserModel? payload);
        Task<BaseResponseModel> AddNewUser(UserModel payload);
        Task<BaseResponseModel> EditUser(UserModel payload);
        Task<BaseResponseModel> ToggleUserStatus(UserModel payload);
    }
}
