using Dfc.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Default implementation of concurrency conflict handling
/// </summary>
public class ConcurrencyHandler : IConcurrencyHandler
{
    public Task<ConcurrencyResolution<T>?> HandleConflictAsync<T>(T currentEntity, T databaseEntity) where T : BaseEntity
    {
        // Default strategy: return null to let the UI handle it
        // The UI layer will show a dialog and call this service again with user's choice
        return Task.FromResult<ConcurrencyResolution<T>?>(null);
    }

    public bool HasBeenModified<T>(T loadedEntity, byte[]? originalRowVersion) where T : BaseEntity
    {
        if (originalRowVersion == null || loadedEntity.RowVersion == null)
        {
            // If no row version, compare ModifiedAt
            return false;
        }

        return !originalRowVersion.SequenceEqual(loadedEntity.RowVersion);
    }

    /// <summary>
    /// Apply a resolution strategy to resolve the conflict
    /// </summary>
    public static ConcurrencyResolution<T> ResolveConflict<T>(
        T currentEntity,
        T databaseEntity,
        ResolutionStrategy strategy) where T : BaseEntity
    {
        switch (strategy)
        {
            case ResolutionStrategy.KeepMine:
                // Keep current user's changes, update RowVersion to database version
                currentEntity.RowVersion = databaseEntity.RowVersion;
                return new ConcurrencyResolution<T>(currentEntity, ResolutionStrategy.KeepMine);

            case ResolutionStrategy.KeepTheirs:
                // Use database version
                return new ConcurrencyResolution<T>(databaseEntity, ResolutionStrategy.KeepTheirs);

            case ResolutionStrategy.Cancel:
                return ConcurrencyResolution<T>.Cancel();

            default:
                // Merge not implemented in base class
                throw new NotImplementedException($"Resolution strategy {strategy} must be handled by the caller");
        }
    }
}
