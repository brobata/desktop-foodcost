using System;

namespace Freecost.Core.Models;

public class DraftItem : BaseEntity
{
    public DraftType DraftType { get; set; }
    public string DraftName { get; set; } = string.Empty;
    public string SerializedData { get; set; } = string.Empty; // JSON serialized entity
    public DateTime LastSavedAt { get; set; } = DateTime.UtcNow;
    public Guid? OriginalItemId { get; set; } // ID of item being edited (null for new items)
    public Guid UserId { get; set; } // Future: track which user created the draft
}

public enum DraftType
{
    Ingredient,
    Recipe,
    Entree
}
