using ErkanTatilPlani.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace ErkanTatilPlani.API.Services;

/// <summary>
/// Memory Cache implementasyonu
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, bool> _keys;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _keys = new ConcurrentDictionary<string, bool>();
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache hit: {Key}", key);
                return value;
            }
            _logger.LogDebug("Cache miss: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache get error for key: {Key}", key);
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            // Eviction callback
            options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                _keys.TryRemove(evictedKey.ToString()!, out _);
                _logger.LogDebug("Cache evicted: {Key}, Reason: {Reason}", evictedKey, reason);
            });

            _cache.Set(key, value, options);
            _keys.TryAdd(key, true);
            _logger.LogDebug("Cache set: {Key}, Expiration: {Expiration}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache set error for key: {Key}", key);
        }
    }

    public void Remove(string key)
    {
        try
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache remove error for key: {Key}", key);
        }
    }

    public void RemoveByPrefix(string prefix)
    {
        try
        {
            var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
            _logger.LogDebug("Cache removed by prefix: {Prefix}, Count: {Count}", prefix, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache remove by prefix error: {Prefix}", prefix);
        }
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                _logger.LogDebug("Cache hit (GetOrCreate): {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache miss (GetOrCreate): {Key}, creating...", key);
            var value = await factory();
            Set(key, value, expiration);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache GetOrCreate error for key: {Key}", key);
            return await factory();
        }
    }
}
