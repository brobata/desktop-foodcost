using Dfc.Core.Enums;
using System;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a single item parsed from an import file, ready for preview
/// </summary>
public class ImportPreviewItem
{
    /// <summary>
    /// Row number in the source file (for error reporting)
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Parsed ingredient name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parsed price (per unit)
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Parsed case price (before division by quantity)
    /// </summary>
    public decimal? CasePrice { get; set; }

    /// <summary>
    /// Parsed quantity (total units in case)
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Parsed unit type
    /// </summary>
    public UnitType? Unit { get; set; }

    /// <summary>
    /// Unit as string (for display, may include unrecognized units)
    /// </summary>
    public string? UnitText { get; set; }

    /// <summary>
    /// Vendor name
    /// </summary>
    public string? Vendor { get; set; }

    /// <summary>
    /// Vendor SKU
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Brand (may be prepended to name)
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Current status of this item
    /// </summary>
    public ImportPreviewStatus Status { get; set; } = ImportPreviewStatus.New;

    /// <summary>
    /// Human-readable status message (e.g., "NEW", "UPDATE (was $0.79)")
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Whether this item has a warning (non-blocking issue)
    /// </summary>
    public bool HasWarning { get; set; }

    /// <summary>
    /// Warning message if any
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Whether this item has an error (will be skipped)
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// For updates: ID of the existing ingredient
    /// </summary>
    public Guid? ExistingIngredientId { get; set; }

    /// <summary>
    /// For updates: the existing price before update
    /// </summary>
    public decimal? ExistingPrice { get; set; }

    /// <summary>
    /// For updates: the existing name before update
    /// </summary>
    public string? ExistingName { get; set; }

    /// <summary>
    /// Whether this item should be included in the import
    /// </summary>
    public bool IsSelected { get; set; } = true;

    /// <summary>
    /// Whether this is a valid item that can be imported
    /// </summary>
    public bool IsValid => !HasError && !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Create an Ingredient entity from this preview item
    /// </summary>
    public Ingredient ToIngredient(Guid locationId)
    {
        return new Ingredient
        {
            Id = ExistingIngredientId ?? Guid.NewGuid(),
            Name = Name,
            CurrentPrice = Price ?? 0,
            CaseQuantity = Quantity ?? 1,
            Unit = Unit ?? UnitType.Each,
            VendorName = Vendor,
            VendorSku = Sku,
            Category = Category,
            LocationId = locationId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Status of an item in the import preview
/// </summary>
public enum ImportPreviewStatus
{
    /// <summary>
    /// New ingredient will be created
    /// </summary>
    New,

    /// <summary>
    /// Existing ingredient will be updated (matched by SKU)
    /// </summary>
    Update,

    /// <summary>
    /// Existing ingredient with same SKU, but no changes needed
    /// </summary>
    NoChange,

    /// <summary>
    /// Item has an error and will be skipped
    /// </summary>
    Error,

    /// <summary>
    /// Item has a warning but can still be imported
    /// </summary>
    Warning
}
