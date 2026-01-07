using Dfc.Core.Enums;
using System;

namespace Dfc.Core.Models;

/// <summary>
/// Tracks local modifications for change tracking purposes.
/// Used for delta sync optimization in local database.
/// </summary>
public class LocalModification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The entity type that was modified (e.g., "Ingredient", "Recipe", "Entree")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity that was modified
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// The name of the entity that was modified (used for duplicate detection)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// The location/restaurant this modification belongs to
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Type of modification: Create, Update, Delete
    /// </summary>
    public ModificationType ModificationType { get; set; }

    /// <summary>
    /// When this modification was made locally
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this modification has been processed
    /// </summary>
    public bool IsSynced { get; set; }

    /// <summary>
    /// When this modification was processed (null if not processed yet)
    /// </summary>
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Number of times we've attempted to process this modification
    /// </summary>
    public int SyncAttempts { get; set; }

    /// <summary>
    /// Last error message if processing failed
    /// </summary>
    public string? LastSyncError { get; set; }

    // Navigation property
    public Location Location { get; set; } = null!;
}
