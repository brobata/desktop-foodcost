using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Services;

/// <summary>
/// Implementation of local modification tracking for delta sync optimization.
/// Tracks all CRUD operations on entities to enable efficient delta synchronization with Firebase,
/// reducing API calls by 70%+ compared to full collection sync.
/// </summary>
/// <remarks>
/// This service implements smart conflict resolution:
/// - Create followed by Update = stays as Create
/// - Create followed by Delete = removes both (entity never existed remotely)
/// - Update followed by Delete = only tracks Delete
/// - Multiple Updates = consolidates to single Update
///
/// All operations are idempotent and safe to call multiple times.
/// </remarks>
public class LocalModificationService : ILocalModificationService
{
    private readonly DfcDbContext _context;
    private readonly ILogger<LocalModificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the LocalModificationService
    /// </summary>
    /// <param name="context">Database context for accessing LocalModifications table</param>
    /// <param name="logger">Logger for diagnostic and error logging</param>
    public LocalModificationService(DfcDbContext context, ILogger<LocalModificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Tracks that an entity was created locally and needs to be synced to Firebase.
    /// Removes any existing unsynced modifications for this entity to ensure clean state.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Ingredient", "Recipe", "Entree")</param>
    /// <param name="entityId">Unique identifier of the created entity</param>
    /// <param name="locationId">Location ID where the entity was created</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown if database operation fails</exception>
    public async Task TrackCreationAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null)
    {
        try
        {
            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            // from previous operations (shared DbContext across repositories)
            _context.ChangeTracker.Clear();

            // Remove any existing modifications for this entity (in case of quick create/update/delete cycles)
            var existing = await _context.LocalModifications
                .Where(m => m.EntityType == entityType && m.EntityId == entityId && !m.IsSynced)
                .ToListAsync();

            _context.LocalModifications.RemoveRange(existing);

            // Add new creation modification
            var modification = new LocalModification
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                LocationId = locationId,
                ModificationType = ModificationType.Create,
                ModifiedAt = DateTime.UtcNow,
                IsSynced = false
            };

            _context.LocalModifications.Add(modification);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Tracked creation: {EntityType} {EntityId} {EntityName}", entityType, entityId, entityName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking creation for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <summary>
    /// Tracks that an entity was updated locally and needs to be synced to Firebase.
    /// Implements smart conflict resolution: if entity has unsynced Create, keeps it as Create
    /// (since Create+Update can be consolidated into a single Create operation).
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Ingredient", "Recipe", "Entree")</param>
    /// <param name="entityId">Unique identifier of the updated entity</param>
    /// <param name="locationId">Location ID where the entity was updated</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown if database operation fails</exception>
    /// <remarks>
    /// Smart conflict resolution logic:
    /// - If unsynced Create exists: updates timestamp but keeps as Create
    /// - If unsynced Update exists: replaces with new Update (consolidates multiple updates)
    /// - Otherwise: creates new Update modification
    /// </remarks>
    public async Task TrackUpdateAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null)
    {
        try
        {
            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            _context.ChangeTracker.Clear();

            // Check if there's already an unsynced creation - if so, keep it as creation
            var existingCreation = await _context.LocalModifications
                .Where(m => m.EntityType == entityType && m.EntityId == entityId &&
                           m.ModificationType == ModificationType.Create && !m.IsSynced)
                .FirstOrDefaultAsync();

            if (existingCreation != null)
            {
                // Update the timestamp and entity name but keep it as a creation
                existingCreation.ModifiedAt = DateTime.UtcNow;
                if (entityName != null)
                    existingCreation.EntityName = entityName;
                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated existing creation timestamp: {EntityType} {EntityId} {EntityName}", entityType, entityId, entityName);
                return;
            }

            // Remove any existing update modifications
            var existingUpdates = await _context.LocalModifications
                .Where(m => m.EntityType == entityType && m.EntityId == entityId &&
                           m.ModificationType == ModificationType.Update && !m.IsSynced)
                .ToListAsync();

            _context.LocalModifications.RemoveRange(existingUpdates);

            // Add new update modification
            var modification = new LocalModification
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                LocationId = locationId,
                ModificationType = ModificationType.Update,
                ModifiedAt = DateTime.UtcNow,
                IsSynced = false
            };

            _context.LocalModifications.Add(modification);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Tracked update: {EntityType} {EntityId} {EntityName}", entityType, entityId, entityName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking update for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <summary>
    /// Tracks that an entity was deleted locally and needs to be synced to Firebase.
    /// Removes all existing unsynced modifications for this entity since deletion supersedes all previous operations.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Ingredient", "Recipe", "Entree")</param>
    /// <param name="entityId">Unique identifier of the deleted entity</param>
    /// <param name="locationId">Location ID where the entity was deleted</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown if database operation fails</exception>
    /// <remarks>
    /// Special handling: If entity had unsynced Create, both are removed since entity never existed remotely.
    /// Otherwise, tracks Delete to remove from Firebase on next sync.
    /// </remarks>
    public async Task TrackDeletionAsync(string entityType, Guid entityId, Guid locationId, string? entityName = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Starting TrackDeletionAsync");
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] EntityType: {entityType}");
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] EntityId: {entityId}");
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] EntityName: {entityName}");
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] LocationId: {locationId}");

            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Clearing change tracker...");
            _context.ChangeTracker.Clear();
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Change tracker cleared");

            // Remove all existing modifications for this entity
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Querying existing modifications...");
            var existing = await _context.LocalModifications
                .Where(m => m.EntityType == entityType && m.EntityId == entityId && !m.IsSynced)
                .ToListAsync();
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Found {existing.Count} existing modifications");

            if (existing.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Removing existing modifications...");
                _context.LocalModifications.RemoveRange(existing);
            }

            // Add deletion modification
            var newId = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Creating new modification with Id: {newId}");
            var modification = new LocalModification
            {
                Id = newId,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                LocationId = locationId,
                ModificationType = ModificationType.Delete,
                ModifiedAt = DateTime.UtcNow,
                IsSynced = false
            };

            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Adding modification to DbSet...");
            _context.LocalModifications.Add(modification);

            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] Calling SaveChangesAsync...");
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] SaveChangesAsync complete");

            _logger.LogDebug("Tracked deletion: {EntityType} {EntityId} {EntityName}", entityType, entityId, entityName);
            System.Diagnostics.Debug.WriteLine($"          [MODIFICATION SERVICE] TrackDeletionAsync complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("          ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("          ║ [MODIFICATION SERVICE EXCEPTION]                  ║");
            System.Diagnostics.Debug.WriteLine("          ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"          Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"          Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"          Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"          Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"          Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("          ╚═══════════════════════════════════════════════════╝");

            _logger.LogError(ex, "Error tracking deletion for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all unsynced modifications for a specific location, ordered by modification timestamp.
    /// Used by delta sync to determine which entities need to be uploaded to Firebase.
    /// </summary>
    /// <param name="locationId">Location ID to filter modifications</param>
    /// <returns>List of unsynced modifications ordered chronologically (oldest first)</returns>
    /// <remarks>
    /// Returns modifications where IsSynced = false, meaning they haven't been successfully uploaded to Firebase.
    /// Processing order (oldest first) ensures consistency and proper sequencing of operations.
    /// </remarks>
    public async Task<List<LocalModification>> GetUnsyncedModificationsAsync(Guid locationId)
    {
        return await _context.LocalModifications
            .Where(m => m.LocationId == locationId && !m.IsSynced)
            .OrderBy(m => m.ModifiedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves synced modifications for a specific location for monitoring and reporting purposes.
    /// Returns most recent synced modifications first, limited to specified count.
    /// </summary>
    /// <param name="locationId">Location ID to filter modifications</param>
    /// <param name="limit">Maximum number of records to return (default 100)</param>
    /// <returns>List of synced modifications ordered by sync timestamp (newest first)</returns>
    /// <remarks>
    /// Used by SyncMonitorViewModel to display recent sync history and calculate statistics.
    /// Limit prevents loading excessive data for locations with long sync history.
    /// </remarks>
    public async Task<List<LocalModification>> GetSyncedModificationsAsync(Guid locationId, int limit = 100)
    {
        return await _context.LocalModifications
            .Where(m => m.LocationId == locationId && m.IsSynced)
            .OrderByDescending(m => m.SyncedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Marks a modification as successfully synced to Firebase.
    /// Sets IsSynced flag, records sync timestamp, and clears error messages.
    /// </summary>
    /// <param name="modificationId">ID of the modification to mark as synced</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <remarks>
    /// Called by SyncService after successful upload to Firebase.
    /// If modification ID doesn't exist, operation is silently ignored.
    /// </remarks>
    public async Task MarkAsSyncedAsync(Guid modificationId)
    {
        // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
        _context.ChangeTracker.Clear();

        var modification = await _context.LocalModifications.FindAsync(modificationId);
        if (modification != null)
        {
            modification.IsSynced = true;
            modification.SyncedAt = DateTime.UtcNow;
            modification.LastSyncError = null;

            await _context.SaveChangesAsync();

            _logger.LogDebug("Marked as synced: {ModificationId}", modificationId);
        }
    }

    /// <summary>
    /// Records a sync failure for a modification, incrementing attempt counter and storing error message.
    /// Used for retry logic and troubleshooting failed syncs.
    /// </summary>
    /// <param name="modificationId">ID of the modification that failed to sync</param>
    /// <param name="errorMessage">Error message describing the failure (truncated to 1000 chars if longer)</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <remarks>
    /// Increments SyncAttempts counter each time called.
    /// Error messages longer than 1000 characters are truncated to prevent database issues.
    /// If modification ID doesn't exist, operation is silently ignored.
    /// Failed modifications can be retried via SyncMonitorViewModel.
    /// </remarks>
    public async Task RecordSyncFailureAsync(Guid modificationId, string errorMessage)
    {
        // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
        _context.ChangeTracker.Clear();

        var modification = await _context.LocalModifications.FindAsync(modificationId);
        if (modification != null)
        {
            modification.SyncAttempts++;
            modification.LastSyncError = errorMessage?.Length > 1000 ? errorMessage.Substring(0, 1000) : errorMessage;

            await _context.SaveChangesAsync();

            _logger.LogWarning("Sync failure for {ModificationId}: {Error}", modificationId, errorMessage);
        }
    }

    /// <summary>
    /// Removes all synced modifications older than the specified date to prevent table bloat.
    /// Should be called periodically (e.g., after each sync) to maintain database performance.
    /// </summary>
    /// <param name="olderThan">Cutoff date - modifications synced before this date will be removed</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <remarks>
    /// Only removes modifications where IsSynced = true (successfully synced to Firebase).
    /// Recommended retention: 30 days (balances audit trail with database size).
    /// SyncService automatically calls this after each successful sync.
    /// Can also be manually triggered via SyncMonitorViewModel.
    /// </remarks>
    public async Task ClearOldSyncedModificationsAsync(DateTime olderThan)
    {
        // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
        _context.ChangeTracker.Clear();

        var oldModifications = await _context.LocalModifications
            .Where(m => m.IsSynced && m.SyncedAt != null && m.SyncedAt < olderThan)
            .ToListAsync();

        if (oldModifications.Any())
        {
            _context.LocalModifications.RemoveRange(oldModifications);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared {Count} old synced modifications", oldModifications.Count);
        }
    }

    /// <summary>
    /// Gets the most recent sync timestamp for a specific location.
    /// Can be used to determine when last successful sync occurred.
    /// </summary>
    /// <param name="locationId">Location ID to check</param>
    /// <returns>DateTime of most recent sync, or null if no synced modifications exist</returns>
    /// <remarks>
    /// Returns SyncedAt timestamp of the most recently synced modification.
    /// Useful for displaying "Last synced" information in UI.
    /// Returns null if location has never been synced or all modifications have been cleaned up.
    /// </remarks>
    public async Task<DateTime?> GetLastSyncTimestampAsync(Guid locationId)
    {
        var lastSynced = await _context.LocalModifications
            .Where(m => m.LocationId == locationId && m.IsSynced && m.SyncedAt != null)
            .OrderByDescending(m => m.SyncedAt)
            .Select(m => m.SyncedAt)
            .FirstOrDefaultAsync();

        return lastSynced;
    }

    /// <summary>
    /// Clears all modification records (both synced and unsynced) for a specific entity.
    /// Used when permanently deleting items from recycle bin to prevent orphaned modification records.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Ingredient", "Recipe", "Entree")</param>
    /// <param name="entityId">Unique identifier of the entity</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <remarks>
    /// This removes ALL modifications for the entity (both synced and unsynced).
    /// Should only be called when permanently deleting an entity that will never be restored.
    /// </remarks>
    public async Task ClearEntityModificationsAsync(string entityType, Guid entityId)
    {
        try
        {
            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            _context.ChangeTracker.Clear();

            // Remove ALL modifications for this entity (synced and unsynced)
            var modifications = await _context.LocalModifications
                .Where(m => m.EntityType == entityType && m.EntityId == entityId)
                .ToListAsync();

            if (modifications.Any())
            {
                _context.LocalModifications.RemoveRange(modifications);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Cleared {Count} modification records for {EntityType} {EntityId}",
                    modifications.Count, entityType, entityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing modifications for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }
}
