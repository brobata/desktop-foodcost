using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IPerformanceService
{
    /// <summary>
    /// Create paginated result from a collection
    /// </summary>
    PagedResult<T> CreatePagedResult<T>(IEnumerable<T> source, int pageNumber, int pageSize);

    /// <summary>
    /// Create paginated result from a queryable (optimized for EF)
    /// </summary>
    Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> source, int pageNumber, int pageSize);

    /// <summary>
    /// Batch process items to avoid memory issues
    /// </summary>
    Task BatchProcessAsync<T>(IEnumerable<T> items, Func<T, Task> action, int batchSize = 100);

    /// <summary>
    /// Get recommended page size based on data complexity
    /// </summary>
    int GetRecommendedPageSize(DataComplexity complexity);
}

public enum DataComplexity
{
    Simple,     // 100 items per page
    Moderate,   // 50 items per page
    Complex,    // 25 items per page
    VeryComplex // 10 items per page
}
