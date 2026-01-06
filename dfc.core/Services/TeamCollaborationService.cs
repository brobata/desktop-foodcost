using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class TeamCollaborationService : ITeamCollaborationService
{
    private readonly ITeamNotificationRepository _notificationRepository;
    private readonly ITeamActivityFeedRepository _activityFeedRepository;
    private readonly IUserRepository _userRepository;

    public TeamCollaborationService(
        ITeamNotificationRepository notificationRepository,
        ITeamActivityFeedRepository activityFeedRepository,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _activityFeedRepository = activityFeedRepository;
        _userRepository = userRepository;
    }

    // Notifications
    public async Task<TeamNotification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null,
        Guid? relatedUserId = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        var notification = new TeamNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            RelatedUserId = relatedUserId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Priority = priority
        };

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<List<TeamNotification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        return await _notificationRepository.GetByUserAsync(userId, unreadOnly);
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        await _notificationRepository.DeleteAsync(notificationId);
    }

    // Activity Feed
    public async Task LogActivityAsync(
        Guid userId,
        string userEmail,
        ActivityType activityType,
        string description,
        string? entityType = null,
        Guid? entityId = null,
        string? entityName = null,
        object? additionalData = null)
    {
        var activity = new TeamActivityFeed
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            ActivityType = activityType,
            Description = description,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            ActivityAt = DateTime.UtcNow,
            AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.CreateAsync(activity);
    }

    public async Task<List<TeamActivityFeed>> GetTeamActivityFeedAsync(int limit = 50)
    {
        return await _activityFeedRepository.GetRecentAsync(limit);
    }

    public async Task<List<TeamActivityFeed>> GetUserActivityFeedAsync(Guid userId, int limit = 50)
    {
        return await _activityFeedRepository.GetByUserAsync(userId, limit);
    }

    public async Task<List<TeamActivityFeed>> GetActivityByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _activityFeedRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<List<TeamActivityFeed>> GetActivityByTypeAsync(ActivityType activityType, int limit = 50)
    {
        return await _activityFeedRepository.GetByTypeAsync(activityType, limit);
    }

    // Collaboration Metrics
    public async Task<Dictionary<string, int>> GetTeamMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var activities = await _activityFeedRepository.GetByDateRangeAsync(start, end);

        var metrics = new Dictionary<string, int>
        {
            ["TotalActivities"] = activities.Count,
            ["RecipesCreated"] = activities.Count(a => a.ActivityType == ActivityType.RecipeCreated),
            ["RecipesUpdated"] = activities.Count(a => a.ActivityType == ActivityType.RecipeUpdated),
            ["IngredientsCreated"] = activities.Count(a => a.ActivityType == ActivityType.IngredientCreated),
            ["IngredientsUpdated"] = activities.Count(a => a.ActivityType == ActivityType.IngredientUpdated),
            ["EntreesCreated"] = activities.Count(a => a.ActivityType == ActivityType.EntreeCreated),
            ["EntreesUpdated"] = activities.Count(a => a.ActivityType == ActivityType.EntreeUpdated),
            ["CommentsAdded"] = activities.Count(a => a.ActivityType == ActivityType.CommentAdded),
            ["RecipesShared"] = activities.Count(a => a.ActivityType == ActivityType.RecipeShared),
            ["ApprovalsSubmitted"] = activities.Count(a => a.ActivityType == ActivityType.ApprovalSubmitted),
            ["ApprovalsApproved"] = activities.Count(a => a.ActivityType == ActivityType.ApprovalApproved),
            ["ReportsGenerated"] = activities.Count(a => a.ActivityType == ActivityType.ReportGenerated),
            ["UniqueUsers"] = activities.Select(a => a.UserId).Distinct().Count()
        };

        return metrics;
    }

    public async Task<List<(User user, int activityCount)>> GetMostActiveUsersAsync(int limit = 10)
    {
        var activities = await _activityFeedRepository.GetRecentAsync(1000); // Get recent 1000 activities
        var userActivities = activities
            .GroupBy(a => a.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToList();

        var result = new List<(User user, int activityCount)>();
        foreach (var userActivity in userActivities)
        {
            var user = await _userRepository.GetByIdAsync(userActivity.UserId);
            if (user != null)
            {
                result.Add((user, userActivity.Count));
            }
        }

        return result;
    }

    public async Task<List<(string entityType, int changeCount)>> GetMostEditedEntitiesAsync(int limit = 10)
    {
        var activities = await _activityFeedRepository.GetRecentAsync(1000);
        var entityCounts = activities
            .Where(a => a.EntityType != null &&
                       (a.ActivityType == ActivityType.RecipeUpdated ||
                        a.ActivityType == ActivityType.IngredientUpdated ||
                        a.ActivityType == ActivityType.EntreeUpdated))
            .GroupBy(a => a.EntityType)
            .Select(g => (entityType: g.Key!, changeCount: g.Count()))
            .OrderByDescending(x => x.changeCount)
            .Take(limit)
            .ToList();

        return entityCounts;
    }
}
