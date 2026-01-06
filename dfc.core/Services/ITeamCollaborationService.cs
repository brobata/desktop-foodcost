using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface ITeamCollaborationService
{
    // Notifications
    Task<TeamNotification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        Guid? relatedUserId = null,
        NotificationPriority priority = NotificationPriority.Normal);

    Task<List<TeamNotification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkNotificationAsReadAsync(Guid notificationId);
    Task MarkAllNotificationsAsReadAsync(Guid userId);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId);

    // Activity Feed
    Task LogActivityAsync(
        Guid userId,
        string userEmail,
        ActivityType activityType,
        string description,
        string? entityType = null,
        Guid? entityId = null,
        string? entityName = null,
        object? additionalData = null);

    Task<List<TeamActivityFeed>> GetTeamActivityFeedAsync(int limit = 50);
    Task<List<TeamActivityFeed>> GetUserActivityFeedAsync(Guid userId, int limit = 50);
    Task<List<TeamActivityFeed>> GetActivityByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<TeamActivityFeed>> GetActivityByTypeAsync(ActivityType activityType, int limit = 50);

    // Collaboration Metrics
    Task<Dictionary<string, int>> GetTeamMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<(User user, int activityCount)>> GetMostActiveUsersAsync(int limit = 10);
    Task<List<(string entityType, int changeCount)>> GetMostEditedEntitiesAsync(int limit = 10);
}
