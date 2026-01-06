using System;
using System.Collections.Generic;

namespace Dfc.Core.Models;

/// <summary>
/// Tracks a batch of imported ingredients for undo functionality
/// </summary>
public class ImportBatch : BaseEntity
{
    public Guid LocationId { get; set; }

    /// <summary>
    /// When the import was executed
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// Total number of items processed
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Number of new ingredients created
    /// </summary>
    public int NewItemCount { get; set; }

    /// <summary>
    /// Number of existing ingredients updated
    /// </summary>
    public int UpdatedItemCount { get; set; }

    /// <summary>
    /// Number of items skipped due to errors
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Original filename that was imported
    /// </summary>
    public string? SourceFileName { get; set; }

    /// <summary>
    /// Name of the mapping used for this import
    /// </summary>
    public string? MappingUsed { get; set; }

    /// <summary>
    /// Whether this import can still be undone
    /// </summary>
    public bool CanUndo { get; set; } = true;

    /// <summary>
    /// When the undo option expires (ImportedAt + 5 minutes)
    /// </summary>
    public DateTime UndoExpiresAt { get; set; }

    // Navigation
    public Location Location { get; set; } = null!;
    public List<ImportBatchItem> Items { get; set; } = new();
}
