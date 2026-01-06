using System;

namespace Dfc.Core.Models;

public class TeamActivityFeed : BaseEntity
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityName { get; set; }
    public DateTime ActivityAt { get; set; } = DateTime.UtcNow;
    public string? AdditionalData { get; set; } // JSON for extra details

    // Navigation
    public User? User { get; set; }
}

public enum ActivityType
{
    RecipeCreated = 0,
    RecipeUpdated = 1,
    RecipeDeleted = 2,
    IngredientCreated = 3,
    IngredientUpdated = 4,
    IngredientDeleted = 5,
    EntreeCreated = 6,
    EntreeUpdated = 7,
    EntreeDeleted = 8,
    CommentAdded = 9,
    RecipeShared = 10,
    ApprovalSubmitted = 11,
    ApprovalApproved = 12,
    ApprovalRejected = 13,
    UserSignedIn = 14,
    ReportGenerated = 15,
    DataExported = 16,
    DataImported = 17
}
