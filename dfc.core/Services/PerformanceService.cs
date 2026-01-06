using Dfc.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class PerformanceService : IPerformanceService
{
    public PagedResult<T> CreatePagedResult<T>(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var sourceList = source.ToList();
        var totalCount = sourceList.Count;
        var items = sourceList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<T>> CreatePagedResultAsync<T>(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task BatchProcessAsync<T>(IEnumerable<T> items, Func<T, Task> action, int batchSize = 100)
    {
        var itemsList = items.ToList();
        var totalBatches = (itemsList.Count + batchSize - 1) / batchSize;

        for (int i = 0; i < totalBatches; i++)
        {
            var batch = itemsList
                .Skip(i * batchSize)
                .Take(batchSize)
                .ToList();

            foreach (var item in batch)
            {
                await action(item);
            }

            // Optional: Add delay between batches to prevent overwhelming the system
            if (i < totalBatches - 1)
            {
                await Task.Delay(10);
            }
        }
    }

    public int GetRecommendedPageSize(DataComplexity complexity)
    {
        return complexity switch
        {
            DataComplexity.Simple => 100,
            DataComplexity.Moderate => 50,
            DataComplexity.Complex => 25,
            DataComplexity.VeryComplex => 10,
            _ => 50
        };
    }
}
