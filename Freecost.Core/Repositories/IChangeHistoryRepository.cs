using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IChangeHistoryRepository
{
    Task<ChangeHistory> CreateAsync(ChangeHistory changeHistory);
    Task CreateMultipleAsync(List<ChangeHistory> changes);
    Task<List<ChangeHistory>> GetByEntityAsync(string entityType, Guid entityId);
    Task<List<ChangeHistory>> GetByUserAsync(Guid userId, int? limit = null);
    Task<List<ChangeHistory>> GetByTypeAsync(ChangeType changeType, int? limit = null);
    Task<List<ChangeHistory>> GetRecentAsync(int limit);
    Task<List<ChangeHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
