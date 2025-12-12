using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IPISService
    {
        Task<List<ResEmployeeModel>?> GetEmployeeAsync();
        Task<List<ResEmployeeModel>?> GetAvailableEmployeeAsync();
        Task<List<DepartmentModel>?> GetDepartmentAsync();
        Task<UserModel> ValidateApproverAsync(string username, string password);
        Task<List<UserModel>?> GetAllUser();
        Task<List<UserModel>> GetUser(ReqUserModel? payload);
        Task<BaseResponseModel> AddNewUser(UserModel payload);
        Task<BaseResponseModel> EditUser(UserModel payload);
        Task<BaseResponseModel> ToggleUserStatus(UserModel payload);
        Task<BaseResponseModel> AddNewEmployee(ResEmployeeModel payload);
        Task<BaseResponseModel> EditEmployee(ResEmployeeModel payload);
        Task<BaseResponseModel> ToggleEmployeeStatus(ResEmployeeModel payload);
    }
}
