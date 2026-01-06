using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class ChangeHistoryRepository : IChangeHistoryRepository
{
    private readonly FreecostDbContext _context;

    public ChangeHistoryRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeHistory> CreateAsync(ChangeHistory changeHistory)
    {
        _context.ChangeHistories.Add(changeHistory);
        await _context.SaveChangesAsync();
        return changeHistory;
    }

    public async Task CreateMultipleAsync(List<ChangeHistory> changes)
    {
        _context.ChangeHistories.AddRange(changes);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChangeHistory>> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await _context.ChangeHistories
            .Include(c => c.User)
            .Where(c => c.EntityType == entityType && c.EntityId == entityId)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync();
    }

    public async Task<List<ChangeHistory>> GetByUserAsync(Guid userId, int? limit = null)
    {
        var query = _context.ChangeHistories
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ChangedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<List<ChangeHistory>> GetByTypeAsync(ChangeType changeType, int? limit = null)
    {
        var query = _context.ChangeHistories
            .Include(c => c.User)
            .Where(c => c.ChangeType == changeType)
            .OrderByDescending(c => c.ChangedAt);

        if (limit.HasValue)
        {
            return await query.Take(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<List<ChangeHistory>> GetRecentAsync(int limit)
    {
        return await _context.ChangeHistories
            .Include(c => c.User)
            .OrderByDescending(c => c.ChangedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ChangeHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ChangeHistories
            .Include(c => c.User)
            .Where(c => c.ChangedAt >= startDate && c.ChangedAt <= endDate)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync();
    }
}
