using JPStockPacking.Models;

namespace JPStockPacking.Services.Interface
{
    public interface IApiClientService
    {
        Task<BaseResponseModel<T>> GetAsync<T>(string url, string? token = null);
        Task<BaseResponseModel<T>> PostAsync<T>(string url, object payload, string? token = null);
    }
}
