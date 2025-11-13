using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using System.Text;
using System.Text.Json;

namespace JPStockPacking.Services.Implement
{
    public class ApiClientService(IConfiguration configuration, IHttpContextAccessor contextAccessor, IHttpClientFactory httpClientFactory) : IApiClientService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<BaseResponseModel<T>> GetAsync<T>(string url, string? token = null)
        {
            using var httpClient = CreateHttpClient(token);
            try
            {
                var response = await httpClient.GetAsync(url);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponseModel<T>
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}",
                    Content = default
                };
            }
        }

        public async Task<BaseResponseModel<T>> PostAsync<T>(string url, object payload, string? token = null)
        {
            using var httpClient = CreateHttpClient(token);
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponseModel<T>
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}",
                    Content = default
                };
            }
        }

        public async Task<BaseResponseModel> PostAsync(string url, object payload, string? token = null)
        {
            using var httpClient = CreateHttpClient(token);
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseModel> PatchAsync(string url, object payload, string? token = null)
        {
            using var httpClient = CreateHttpClient(token);
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PatchAsync(url, content);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        private HttpClient CreateHttpClient(string? token)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var apiKey = _configuration["ApiSettings:APIKey"];
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            return client;
        }

        private static async Task<BaseResponseModel<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
                return new BaseResponseModel<T>
                {
                    Code = statusCode,
                    IsSuccess = true,
                    Message = "OK",
                    Content = data
                };
            }

            return new BaseResponseModel<T>
            {
                Code = statusCode,
                IsSuccess = false,
                Message = $"API error: {response.ReasonPhrase ?? response.StatusCode.ToString()}",
                Content = default
            };
        }

        private static BaseResponseModel HandleResponse(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return new BaseResponseModel
                {
                    Code = statusCode,
                    IsSuccess = true,
                    Message = "OK"
                };
            }

            return new BaseResponseModel
            {
                Code = statusCode,
                IsSuccess = false,
                Message = $"API error: {response.ReasonPhrase ?? response.StatusCode.ToString()}"
            };
        }
    }
}
