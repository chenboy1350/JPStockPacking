using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using static JPStockPacking.Services.Implement.AuthService;

namespace JPStockPacking.Services.Implement
{
    public class PISService(IConfiguration configuration, IApiClientService apiClientService, ICacheService cacheService, Serilog.ILogger logger, SPDbContext sPDbContext) : IPISService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IApiClientService _apiClientService = apiClientService;
        private readonly ICacheService _cacheService = cacheService;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly Serilog.ILogger _logger = logger;

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
                        _logger.Information("GetEmployeeAsync Response : {@response}", response.Content.Content);
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
                _logger.Information("GetAvailableEmployeeAsync Response : {@response}", response.Content.Content);
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
                        _logger.Information("GetDepartmentAsync Response : {@response}", response.Content.Content);
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
            _logger.Information("GetDepartmentAsync Request : {@payload}", payload);

            var response = await _apiClientService.PostAsync<BaseResponseModel<UserModel>>(url!, payload);

            if (response.IsSuccess && response.Content != null)
            {
                if (_sPDbContext.MappingPermission.Any(x => x.UserId == response.Content.Content!.UserID && x.IsActive && x.PermissionId == 3)) 
                {
                    _logger.Information("ValidateApproverAsync Response : {@response}", response.Content.Content);
                    return response.Content.Content!;
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        public async Task<List<UserModel>?> GetAllUser()
        {
            var users = await _cacheService.GetOrCreateAsync(
                cacheKey: "UserList",
                async () =>
                {
                    var apiSettings = _configuration.GetSection("ApiSettings");
                    var url = apiSettings["GetAllUser"];

                    var response = await _apiClientService.GetAsync<BaseResponseModel<List<UserModel>>>(url!);

                    if (response.IsSuccess && response.Content != null)
                    {
                        _logger.Information("GetAllUser Response : {@response}", response.Content.Content);
                        return response.Content.Content;
                    }

                    return [];
                }
            );

            return users;
        }

        public async Task<List<UserModel>> GetUser(ReqUserModel? payload = null)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["GetUser"];

            var requestPayload = payload ?? new ReqUserModel();
            _logger.Information("GetUser Request : {@payload}", requestPayload);

            var response = await _apiClientService.PostAsync<BaseResponseModel<List<UserModel>>>(url!, requestPayload);

            if (response.IsSuccess && response.Content != null)
            {
                _logger.Information("GetUser Response : {@response}", response.Content.Content);
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

            _logger.Information("AddNewUser Request : {@payload}", payload);

            var response = await _apiClientService.PostAsync(url!, payload);

            if (response.IsSuccess)
            {
                _logger.Information("AddNewUser Successfully");
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User added successfully."
                };
            }
            else
            {
                _logger.Information("AddNewUser Failed");
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

            _logger.Information("EditUser Request : {@payload}", payload);

            var response = await _apiClientService.PatchAsync(url!, payload);

            if (response.IsSuccess)
            {
                _logger.Information("EditUser Successfully");
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User edit successfully."
                };
            }
            else
            {
                _logger.Information("EditUser Failed");
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

            _logger.Information("ToggleUserStatus Request : {@payload}", payload);

            var response = await _apiClientService.PatchAsync(url!, payload);

            if (response.IsSuccess)
            {
                _logger.Information("ToggleUserStatus Successfully");
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "User edit successfully."
                };
            }
            else
            {
                _logger.Information("ToggleUserStatus Failed");
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
