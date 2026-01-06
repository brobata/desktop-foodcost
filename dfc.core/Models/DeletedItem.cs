using System;

namespace Dfc.Core.Models;

/// <summary>
/// Tracks deleted items for recovery
/// </summary>
public class DeletedItem : BaseEntity
{
    public Guid ItemId { get; set; }
    public DeletedItemType ItemType { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string SerializedData { get; set; } = string.Empty; // JSON serialized entity
    public Guid LocationId { get; set; }
    public DateTime DeletedDate { get; set; } = DateTime.UtcNow;
    public string? DeletedBy { get; set; }
    public DateTime? ExpirationDate { get; set; } // Auto-delete after this date

    // Navigation properties
    public Location Location { get; set; } = null!;
}

public enum DeletedItemType
{
    Ingredient,
    Recipe,
    Entree
}
