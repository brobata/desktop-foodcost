using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Repositories;

public interface ITeamActivityFeedRepository
{
    Task<TeamActivityFeed> CreateAsync(TeamActivityFeed activity);
    Task<List<TeamActivityFeed>> GetRecentAsync(int limit);
    Task<List<TeamActivityFeed>> GetByUserAsync(Guid userId, int limit);
    Task<List<TeamActivityFeed>> GetByTypeAsync(ActivityType activityType, int limit);
    Task<List<TeamActivityFeed>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task DeleteOldAsync(DateTime beforeDate);
}
