using System;

namespace Freecost.Core.Models;

/// <summary>
/// Records ingredient waste/loss for tracking and reporting
/// </summary>
public class WasteRecord : BaseEntity
{
    public Guid IngredientId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public DateTime WasteDate { get; set; } = DateTime.UtcNow;
    public WasteReason Reason { get; set; }
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; }

    // Navigation properties
    public Ingredient Ingredient { get; set; } = null!;
    public Location Location { get; set; } = null!;
}

public enum WasteReason
{
    Spoilage,
    Expired,
    Overproduction,
    Preparation,
    Contamination,
    Damage,
    Other
}
