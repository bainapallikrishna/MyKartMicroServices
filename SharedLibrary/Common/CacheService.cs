using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary.Common
{
    public class CacheService:ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache _redisCache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
            // Attempt to keep a typed reference to the Redis implementation where available
            _redisCache = cache as Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);

            return data == null
                ? default
                : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan expiry)
        {
            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(value),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // Only supported when underlying implementation is RedisCache
            if (_redisCache == null)
            {
                // Fall back to best-effort: try remove exact key if pattern has no wildcard
                if (!pattern.Contains("*"))
                {
                    await _cache.RemoveAsync(pattern);
                }
                return;
            }

            // Use StackExchange.Redis directly to scan keys matching the pattern
            try
            {
                var connectionField = typeof(Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache)
                    .GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var connection = connectionField?.GetValue(_redisCache) as StackExchange.Redis.ConnectionMultiplexer;
                if (connection == null) return;

                var server = connection.GetEndPoints().Select(e => connection.GetServer(e)).FirstOrDefault(s => !s.IsSlave && s.IsConnected) ?? connection.GetServer(connection.GetEndPoints().First());
                if (server == null) return;

                var keys = server.Keys(pattern: pattern.Replace("*", "*"));
                var tasks = new List<Task>();
                foreach (var key in keys)
                {
                    tasks.Add(_cache.RemoveAsync(key.ToString()));
                }
                await Task.WhenAll(tasks);
            }
            catch
            {
                // ignore failures in best-effort invalidation
            }
        }
    }
}
