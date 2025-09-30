using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using static JPStockPacking.Services.Implement.AuthService;

namespace JPStockPacking.Services.Implement
{
    public class PISService(IConfiguration configuration, IApiClientService apiClientService, ICacheService cacheService) : IPISService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IApiClientService _apiClientService = apiClientService;
        private readonly ICacheService _cacheService = cacheService;

        public async Task<string> GetPersonalInfoAsync(string pis)
        {
            await Task.Delay(1000);
            return $"PIS: {pis}";
        }

        public async Task<List<ResEmployeeModel>?> GetEmployeeAsync()
        {
            var employees = await _cacheService.GetOrCreateAsync(
                cacheKey: "EmployeeList",
                async () =>
                {
                    var apiSettings = _configuration.GetSection("ApiSettings");
                    var url = apiSettings["Employee"];

                    var response = await _apiClientService.GetAsync<BaseResponseModel<List<ResEmployeeModel>>>(url!);

                    if (response.IsSuccess && response.Content != null)
                    {
                        return response.Content.Content;
                    }

                    return [];
                }
            );

            return employees;
        }

        public async Task<List<ResEmployeeModel>?> GetAvailableEmployeeAsync()
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["AvailableEmployee"];

            var response = await _apiClientService.GetAsync<BaseResponseModel<List<ResEmployeeModel>>>(url!);

            if (response.IsSuccess && response.Content != null)
            {
                return response.Content.Content;
            }
            else
            {
                return [];
            }
        }

        public async Task<List<DepartmentModel>?> GetDepartmentAsync()
        {
            var departments = await _cacheService.GetOrCreateAsync(
                cacheKey: "DepartmentList",
                async () =>
                {
                    var apiSettings = _configuration.GetSection("ApiSettings");
                    var url = apiSettings["Department"];

                    var response = await _apiClientService.GetAsync<BaseResponseModel<List<DepartmentModel>>>(url!);

                    if (response.IsSuccess && response.Content != null)
                    {
                        return response.Content.Content;
                    }

                    return [];
                }
            );

            return departments;
        }

        public async Task<UserModel> ValidateApproverAsync(string username, string password)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["ValidateApprover"];

            AuthRequestModel payload = new()
            {
                ClientId = username,
                ClientSecret = password
            };

            var response = await _apiClientService.PostAsync<BaseResponseModel<UserModel>>(url!, payload);

            if (response.IsSuccess && response.Content != null)
            {
                return response.Content.Content!;
            }
            else
            {
                return new UserModel();
            }
        }

        public async Task<List<UserModel>> GetUser(ReqUserModel? payload = null)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["GetUser"];

            var requestPayload = payload ?? new ReqUserModel();

            var response = await _apiClientService.PostAsync<BaseResponseModel<List<UserModel>>>(url!, requestPayload);

            if (response.IsSuccess && response.Content != null)
            {
                return response.Content.Content!;
            }
            else
            {
                return [];
            }
        }

        public async Task<BaseResponseModel> AddNewUser(UserModel payload)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["AddNewUser"];

            var response = await _apiClientService.PostAsync(url!, payload);

            if (response.IsSuccess)
            {
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User added successfully."
                };
            }
            else
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "Failed to add user."
                };
            }

        }

        public async Task<BaseResponseModel> EditUser(UserModel payload)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["EditUser"];

            var response = await _apiClientService.PatchAsync(url!, payload);

            if (response.IsSuccess)
            {
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User edit successfully."
                };
            }
            else
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "Failed to edit user."
                };
            }

        }

        public async Task<BaseResponseModel> ToggleUserStatus(UserModel payload)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["ToggleUserStatus"];

            var response = await _apiClientService.PatchAsync(url!, payload);

            if (response.IsSuccess)
            {
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User edit successfully."
                };
            }
            else
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "Failed to edit user."
                };
            }

        }
    }
}
