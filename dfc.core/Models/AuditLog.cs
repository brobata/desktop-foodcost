using System;

namespace Dfc.Core.Models;

public class AuditLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // Navigation
    public User? User { get; set; }
}

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    Viewed,
    Exported,
    Imported,
    SignedIn,
    SignedOut,
    PasswordReset,
    PermissionChanged,
    Approved,
    Rejected,
    Shared,
    Commented
}
