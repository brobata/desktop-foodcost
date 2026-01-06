using System;

namespace Dfc.Core.Models;

public class ChangeHistory : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Recipe, Ingredient, Entree
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangeDescription { get; set; }
    public string? ChangesSummary { get; set; } // JSON of all changes in this operation

    // Navigation
    public User? User { get; set; }
}

public enum ChangeType
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
    Restored = 3,
    FieldChanged = 4,
    RelationshipAdded = 5,
    RelationshipRemoved = 6
}
