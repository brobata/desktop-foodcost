using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _cacheKeys;

    // Cache TTL Strategy:
    // - List data (recipes, ingredients, entrees): 5 minutes (data changes frequently)
    // - Single items: 15 minutes (less volatile, accessed individually)
    // - Static data (allergens, units): 30 minutes (rarely changes)
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan ListExpiration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan ItemExpiration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan StaticDataExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
        _cacheKeys = new HashSet<string>();
    }

    public T? Get<T>(string key) where T : class
    {
        return _cache.TryGetValue(key, out T? value) ? value : null;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var cacheExpiration = expiration ?? DefaultExpiration;

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheExpiration)
            .RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                _cacheKeys.Remove(k.ToString() ?? string.Empty);
            });

        _cache.Set(key, value, options);
        _cacheKeys.Add(key);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _cacheKeys.Remove(key);
    }

    public void Clear()
    {
        foreach (var key in _cacheKeys.ToList())
        {
            _cache.Remove(key);
        }
        _cacheKeys.Clear();
    }

    public void ClearPattern(string pattern)
    {
        var keysToRemove = _cacheKeys
            .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.Remove(key);
        }
    }
}
