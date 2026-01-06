using Dfc.Core.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Handles concurrent editing conflicts using optimistic locking
/// </summary>
public interface IConcurrencyHandler
{
    /// <summary>
    /// Handle a concurrency conflict and present resolution options to the user
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="currentEntity">The entity the user is trying to save</param>
    /// <param name="databaseEntity">The current entity in the database</param>
    /// <returns>Resolution result with the resolved entity or null if user cancelled</returns>
    Task<ConcurrencyResolution<T>?> HandleConflictAsync<T>(T currentEntity, T databaseEntity) where T : BaseEntity;

    /// <summary>
    /// Check if an entity has been modified since it was loaded
    /// </summary>
    bool HasBeenModified<T>(T loadedEntity, byte[]? originalRowVersion) where T : BaseEntity;
}

/// <summary>
/// Result of a concurrency conflict resolution
/// </summary>
public class ConcurrencyResolution<T> where T : BaseEntity
{
    /// <summary>
    /// The resolved entity to save
    /// </summary>
    public T ResolvedEntity { get; set; }

    /// <summary>
    /// The strategy used to resolve the conflict
    /// </summary>
    public ResolutionStrategy Strategy { get; set; }

    /// <summary>
    /// Whether the user cancelled the operation
    /// </summary>
    public bool Cancelled { get; set; }

    public ConcurrencyResolution(T resolvedEntity, ResolutionStrategy strategy)
    {
        ResolvedEntity = resolvedEntity;
        Strategy = strategy;
        Cancelled = false;
    }

    public static ConcurrencyResolution<T> Cancel()
    {
        return new ConcurrencyResolution<T>(default!, ResolutionStrategy.Cancel)
        {
            Cancelled = true
        };
    }
}

/// <summary>
/// Strategies for resolving concurrency conflicts
/// </summary>
public enum ResolutionStrategy
{
    /// <summary>
    /// Keep the current user's changes (overwrite database)
    /// </summary>
    KeepMine,

    /// <summary>
    /// Discard current changes and use database version
    /// </summary>
    KeepTheirs,

    /// <summary>
    /// Merge changes (user decides field by field)
    /// </summary>
    Merge,

    /// <summary>
    /// User cancelled the operation
    /// </summary>
    Cancel
}
