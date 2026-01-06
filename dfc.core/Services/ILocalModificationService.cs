using Dfc.Core.Enums;
using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Service for tracking local modifications for delta sync optimization
/// </summary>
public interface ILocalModificationService
{
    /// <summary>
    /// Records that an entity was created locally and needs to be synced
    /// </summary>
    Task TrackCreationAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null);

    /// <summary>
    /// Records that an entity was updated locally and needs to be synced
    /// </summary>
    Task TrackUpdateAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null);

    /// <summary>
    /// Records that an entity was deleted locally and needs to be synced
    /// </summary>
    Task TrackDeletionAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null);

    /// <summary>
    /// Gets all unsynced modifications for a specific location
    /// </summary>
    Task<List<LocalModification>> GetUnsyncedModificationsAsync(Guid locationId);

    /// <summary>
    /// Gets synced modifications for a specific location (for monitoring/reporting)
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <param name="limit">Maximum number of records to return (default 100)</param>
    Task<List<LocalModification>> GetSyncedModificationsAsync(Guid locationId, int limit = 100);

    /// <summary>
    /// Marks a modification as successfully synced
    /// </summary>
    Task MarkAsSyncedAsync(Guid modificationId);

    /// <summary>
    /// Records a sync failure for a modification
    /// </summary>
    Task RecordSyncFailureAsync(Guid modificationId, string errorMessage);

    /// <summary>
    /// Clears all synced modifications older than the specified date
    /// (for cleanup to prevent table from growing indefinitely)
    /// </summary>
    Task ClearOldSyncedModificationsAsync(DateTime olderThan);

    /// <summary>
    /// Gets the most recent modification timestamp for a specific location
    /// Used to determine the lastSyncedAt timestamp for delta sync
    /// </summary>
    Task<DateTime?> GetLastSyncTimestampAsync(Guid locationId);

    /// <summary>
    /// Clears all modification records (synced and unsynced) for a specific entity
    /// Used when permanently deleting items from recycle bin to prevent orphaned records
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Ingredient", "Recipe", "Entree")</param>
    /// <param name="entityId">Unique identifier of the entity</param>
    Task ClearEntityModificationsAsync(string entityType, Guid entityId);
}
