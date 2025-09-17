using JPStockPacking.Models;
using JPStockPacking.Services.Interface;

namespace JPStockPacking.Services.Implement
{
    public class PISService(IConfiguration configuration, IApiClientService apiClientService) : IPISService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IApiClientService _apiClientService = apiClientService;

        public async Task<string> GetPersonalInfo(string pis)
        {
            await Task.Delay(1000);
            return $"PIS: {pis}";
        }

        public async Task<List<EmployeeWithDepartmentModel>?> GetEmployeeAsync()
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var url = apiSettings["Employee"];

            var response = await _apiClientService.GetAsync<BaseResponseModel<List<EmployeeWithDepartmentModel>>>(url!);

            if (response.IsSuccess && response.Content != null)
            {
                return response.Content.Content;
            }
            else
            {
                return [];
            }

        }

        public async Task<string> GetDepartment(string pis)
        {
            await Task.Delay(1000);
            return $"PIS: {pis}";
        }
    }
}
