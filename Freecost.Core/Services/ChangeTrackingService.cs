using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class ChangeTrackingService : IChangeTrackingService
{
    private readonly IChangeHistoryRepository _repository;

    public ChangeTrackingService(IChangeHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task TrackChangeAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid userId,
        string userEmail,
        ChangeType changeType,
        string? fieldName = null,
        string? oldValue = null,
        string? newValue = null,
        string? changeDescription = null)
    {
        var change = new ChangeHistory
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            UserEmail = userEmail,
            ChangeType = changeType,
            ChangedAt = DateTime.UtcNow,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeDescription = changeDescription ?? GenerateChangeDescription(changeType, fieldName, oldValue, newValue),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(change);
    }

    public async Task TrackMultipleChangesAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid userId,
        string userEmail,
        Dictionary<string, (string? oldValue, string? newValue)> changes)
    {
        var changesList = new List<ChangeHistory>();
        var changesSummary = new Dictionary<string, object>
        {
            ["EntityType"] = entityType,
            ["EntityId"] = entityId,
            ["EntityName"] = entityName,
            ["ChangedBy"] = userEmail,
            ["ChangedAt"] = DateTime.UtcNow,
            ["Changes"] = changes
        };

        var summaryJson = JsonSerializer.Serialize(changesSummary);

        foreach (var change in changes)
        {
            var changeHistory = new ChangeHistory
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                UserId = userId,
                UserEmail = userEmail,
                ChangeType = ChangeType.FieldChanged,
                ChangedAt = DateTime.UtcNow,
                FieldName = change.Key,
                OldValue = change.Value.oldValue,
                NewValue = change.Value.newValue,
                ChangeDescription = $"Changed {change.Key} from '{change.Value.oldValue}' to '{change.Value.newValue}'",
                ChangesSummary = summaryJson,
                CreatedAt = DateTime.UtcNow
            };

            changesList.Add(changeHistory);
        }

        await _repository.CreateMultipleAsync(changesList);
    }

    public async Task<List<ChangeHistory>> GetEntityHistoryAsync(string entityType, Guid entityId)
    {
        return await _repository.GetByEntityAsync(entityType, entityId);
    }

    public async Task<List<ChangeHistory>> GetChangesByUserAsync(Guid userId, int? limit = null)
    {
        return await _repository.GetByUserAsync(userId, limit);
    }

    public async Task<List<ChangeHistory>> GetChangesByTypeAsync(ChangeType changeType, int? limit = null)
    {
        return await _repository.GetByTypeAsync(changeType, limit);
    }

    public async Task<List<ChangeHistory>> GetRecentChangesAsync(int limit = 50)
    {
        return await _repository.GetRecentAsync(limit);
    }

    public async Task<List<ChangeHistory>> GetChangesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _repository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<ChangeHistory?> GetLastModificationAsync(string entityType, Guid entityId)
    {
        var history = await _repository.GetByEntityAsync(entityType, entityId);
        return history
            .Where(h => h.ChangeType == ChangeType.Updated || h.ChangeType == ChangeType.FieldChanged)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefault();
    }

    private string GenerateChangeDescription(ChangeType changeType, string? fieldName, string? oldValue, string? newValue)
    {
        return changeType switch
        {
            ChangeType.Created => "Entity created",
            ChangeType.Deleted => "Entity deleted",
            ChangeType.Restored => "Entity restored",
            ChangeType.FieldChanged => $"Changed {fieldName} from '{oldValue}' to '{newValue}'",
            ChangeType.RelationshipAdded => $"Added relationship: {fieldName}",
            ChangeType.RelationshipRemoved => $"Removed relationship: {fieldName}",
            _ => "Entity updated"
        };
    }
}
