namespace JPStockPacking.Services.Interface
{
    public interface ICacheService
    {
        Task<T?> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);
        void Remove(string cacheKey);
        void Clear();
    }
}
