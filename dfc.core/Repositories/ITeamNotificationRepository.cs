using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Repositories;

public interface ITeamNotificationRepository
{
    Task<TeamNotification?> GetByIdAsync(Guid id);
    Task<List<TeamNotification>> GetByUserAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<TeamNotification> CreateAsync(TeamNotification notification);
    Task UpdateAsync(TeamNotification notification);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id);
    Task DeleteOldAsync(DateTime beforeDate);
}
