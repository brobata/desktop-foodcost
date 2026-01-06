using System;

namespace Dfc.Core.Models;

/// <summary>
/// Tracks an individual item within an import batch for undo functionality
/// </summary>
public class ImportBatchItem
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid IngredientId { get; set; }

    /// <summary>
    /// What action was taken for this item
    /// </summary>
    public ImportAction Action { get; set; }

    /// <summary>
    /// For updates: the price before the import (for undo)
    /// </summary>
    public decimal? PreviousPrice { get; set; }

    /// <summary>
    /// For updates: the name before the import (for undo)
    /// </summary>
    public string? PreviousName { get; set; }

    /// <summary>
    /// For updates: the vendor name before the import (for undo)
    /// </summary>
    public string? PreviousVendor { get; set; }

    /// <summary>
    /// For updates: the category before the import (for undo)
    /// </summary>
    public string? PreviousCategory { get; set; }

    // Navigation
    public ImportBatch ImportBatch { get; set; } = null!;
}

/// <summary>
/// What action was performed on an ingredient during import
/// </summary>
public enum ImportAction
{
    /// <summary>
    /// New ingredient was created
    /// </summary>
    Created,

    /// <summary>
    /// Existing ingredient was updated (matched by SKU)
    /// </summary>
    Updated,

    /// <summary>
    /// Item was skipped due to error or no changes needed
    /// </summary>
    Skipped
}
