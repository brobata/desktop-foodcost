using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IChangeTrackingService
{
    /// <summary>
    /// Track a change to an entity
    /// </summary>
    Task TrackChangeAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid userId,
        string userEmail,
        ChangeType changeType,
        string? fieldName = null,
        string? oldValue = null,
        string? newValue = null,
        string? changeDescription = null);

    /// <summary>
    /// Track multiple field changes in one operation
    /// </summary>
    Task TrackMultipleChangesAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid userId,
        string userEmail,
        Dictionary<string, (string? oldValue, string? newValue)> changes);

    /// <summary>
    /// Get change history for an entity
    /// </summary>
    Task<List<ChangeHistory>> GetEntityHistoryAsync(string entityType, Guid entityId);

    /// <summary>
    /// Get all changes by a user
    /// </summary>
    Task<List<ChangeHistory>> GetChangesByUserAsync(Guid userId, int? limit = null);

    /// <summary>
    /// Get changes by type
    /// </summary>
    Task<List<ChangeHistory>> GetChangesByTypeAsync(ChangeType changeType, int? limit = null);

    /// <summary>
    /// Get recent changes across all entities
    /// </summary>
    Task<List<ChangeHistory>> GetRecentChangesAsync(int limit = 50);

    /// <summary>
    /// Get changes within a date range
    /// </summary>
    Task<List<ChangeHistory>> GetChangesByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get who last modified an entity
    /// </summary>
    Task<ChangeHistory?> GetLastModificationAsync(string entityType, Guid entityId);
}
