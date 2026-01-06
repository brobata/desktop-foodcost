using System;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Get or create cached value
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Remove value from cache
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Clear all cached values
    /// </summary>
    void Clear();

    /// <summary>
    /// Clear cached values matching a pattern
    /// </summary>
    void ClearPattern(string pattern);
}
