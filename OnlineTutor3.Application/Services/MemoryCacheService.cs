using Microsoft.Extensions.Caching.Memory;
using OnlineTutor3.Application.Interfaces;

namespace OnlineTutor3.Application.Services
{
    /// <summary>
    /// Реализация кэширования через IMemoryCache
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
                SlidingExpiration = expiration ?? _defaultExpiration
            };

            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            // IMemoryCache не поддерживает полную очистку напрямую
            // В реальном приложении можно использовать IMemoryCache.Clear() если доступен
            // Или использовать другой механизм кэширования (Redis, etc.)
            return Task.CompletedTask;
        }
    }
}

