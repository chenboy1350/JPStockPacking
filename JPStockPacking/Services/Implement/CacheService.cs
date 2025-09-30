using JPStockPacking.Services.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace JPStockPacking.Services.Implement
{
    public class CacheService(IMemoryCache cache) : ICacheService
    {
        private readonly IMemoryCache _cache = cache;
        private readonly HashSet<string> _keys = [];

        public async Task<T?> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedValue))
            {
                if (cachedValue != null) return cachedValue;
            }

            var result = await factory();

            if (result != null)
            {
                _cache.Set(cacheKey, result, absoluteExpiration ?? TimeSpan.FromHours(12));
                lock (_keys)
                {
                    _keys.Add(cacheKey);
                }
            }

            return result;
        }

        public void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
            lock (_keys)
            {
                _keys.Remove(cacheKey);
            }
        }

        public void Clear()
        {
            lock (_keys)
            {
                foreach (var key in _keys)
                {
                    _cache.Remove(key);
                }
                _keys.Clear();
            }
        }
    }
}
