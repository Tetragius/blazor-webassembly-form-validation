using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorApp.Api.Services
{
     /// <summary>
    /// contains memory cache thread safe methods
    /// used as help: https://michaelscodingspot.com/cache-implementations-in-csharp-net/
    ///
    /// from this article: dont use async methods for work with cache if
    /// 1. There’s no danger of multiple threads accessing the same cache item.
    /// 2. You don’t mind creating the item more than once. For example, if one extra trip to the database won’t change much.
    /// </summary>
    public class MemoryCacheRepository : IDisposable
    {
        private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks;
        private readonly IMemoryCache _cache;

        private const int DefaultCacheItemLifetimeInSeconds = 120;

        private DateTimeOffset GetCacheExpiredDateTimeOffset() =>
            DateTimeOffset.UtcNow.AddSeconds(DefaultCacheItemLifetimeInSeconds);

        public MemoryCacheRepository(IMemoryCache cache)
        {
            _locks = new ConcurrentDictionary<object, SemaphoreSlim>();
            _cache = cache;
        }

        public async Task<TItem> GetOrCreateFactoryMethodAsync<TItem>(object key, Func<Task<TItem>> createItem)
        {
            TItem cacheEntry;

            if (!_cache.TryGetValue(key, out cacheEntry)) // Look for cache key.
            {
                SemaphoreSlim mylock = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

                await mylock.WaitAsync();
                try
                {
                    if (!_cache.TryGetValue(key, out cacheEntry))
                    {
                        // Key not in cache, so get data.
                        cacheEntry = await createItem();
                        _cache.Set(key, cacheEntry, GetCacheExpiredDateTimeOffset());
                    }
                }
                finally
                {
                    mylock.Release();
                }
            }

            return cacheEntry;
        }

        public async Task<TItem> SetFactoryMethodAsync<TItem>(
            object key, 
            TItem item,
            DateTimeOffset? absoluteExpiration)
        {
            var locker = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            try
            {
                await locker.WaitAsync();
                // if(_cache.TryGetValue(key, out cacheEntry))
                _cache.Remove(key);
                _cache.Set(key, item, absoluteExpiration ?? GetCacheExpiredDateTimeOffset());
            }
            finally
            {
                locker.Release();
            }

            return item;
        }

        public async Task<TItem> GetFactoryMethodAsync<TItem>(object key)
        {
            TItem result;

            var locker = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            try
            {
                await locker.WaitAsync();
                if (!_cache.TryGetValue(key, out result))
                {
                    result = default;
                }
            }
            finally
            {
                locker.Release();
            }

            return result;
        }

        public async Task<bool> CleanFactoryMethodAsync(object key)
        {
            var locker = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            try
            {
                await locker.WaitAsync();
                _cache.Remove(key);
            }
            finally
            {
                locker.Release();
            }

            return true;
        }


        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}