using JPStockPacking.Services.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace JPStockPacking.Services.Implement
{
    public class CacheService(IMemoryCache cache, Serilog.ILogger logger) : ICacheService
    {
        private readonly Serilog.ILogger _logger = logger;
        private readonly IMemoryCache _cache = cache;
        private readonly HashSet<string> _keys = [];

        public async Task<T?> GetOrCreateAsync<T>(
            string cacheKey,
            Func<Task<T>> factory,
            TimeSpan? absoluteExpiration = null)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }

            var result = await factory();

            if (result is System.Collections.IEnumerable enumerable)
            {
                bool isEmptyCollection = !enumerable.Cast<object>().Any();
                if (isEmptyCollection)
                {
                    _logger.Warning("Cache skip for key '{CacheKey}' because result is empty collection.", cacheKey);
                    return result;
                }
            }

            if (result != null)
            {
                _cache.Set(cacheKey, result, absoluteExpiration ?? TimeSpan.FromHours(4));

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
