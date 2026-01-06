using System;

namespace Freecost.Core.Models;

public class TeamNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? RelatedUserId { get; set; } // User who triggered the notification
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    // Navigation
    public User? User { get; set; }
    public User? RelatedUser { get; set; }
}

public enum NotificationType
{
    RecipeShared = 0,
    CommentMention = 1,
    CommentReply = 2,
    ApprovalRequested = 3,
    ApprovalApproved = 4,
    ApprovalRejected = 5,
    ApprovalNeedsRevision = 6,
    PriceAlert = 7,
    RecipeUpdated = 8,
    TeamInvitation = 9,
    UserMentioned = 10,
    ChangeTracked = 11
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
