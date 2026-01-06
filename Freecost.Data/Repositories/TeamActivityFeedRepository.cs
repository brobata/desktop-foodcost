using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class TeamActivityFeedRepository : ITeamActivityFeedRepository
{
    private readonly FreecostDbContext _context;

    public TeamActivityFeedRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<TeamActivityFeed> CreateAsync(TeamActivityFeed activity)
    {
        _context.TeamActivityFeeds.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<List<TeamActivityFeed>> GetRecentAsync(int limit)
    {
        return await _context.TeamActivityFeeds
            .Include(a => a.User)
            .OrderByDescending(a => a.ActivityAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TeamActivityFeed>> GetByUserAsync(Guid userId, int limit)
    {
        return await _context.TeamActivityFeeds
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.ActivityAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TeamActivityFeed>> GetByTypeAsync(ActivityType activityType, int limit)
    {
        return await _context.TeamActivityFeeds
            .Include(a => a.User)
            .Where(a => a.ActivityType == activityType)
            .OrderByDescending(a => a.ActivityAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TeamActivityFeed>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TeamActivityFeeds
            .Include(a => a.User)
            .Where(a => a.ActivityAt >= startDate && a.ActivityAt <= endDate)
            .OrderByDescending(a => a.ActivityAt)
            .ToListAsync();
    }

    public async Task DeleteOldAsync(DateTime beforeDate)
    {
        var oldActivities = await _context.TeamActivityFeeds
            .Where(a => a.ActivityAt < beforeDate)
            .ToListAsync();

        _context.TeamActivityFeeds.RemoveRange(oldActivities);
        await _context.SaveChangesAsync();
    }
}
