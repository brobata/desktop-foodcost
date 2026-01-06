using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(
        Guid userId,
        string userEmail,
        AuditAction action,
        string entityType,
        Guid? entityId = null,
        string? entityName = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            Action = action.ToString(),
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow,
            IsSuccess = true
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public async Task LogFailureAsync(
        Guid userId,
        string userEmail,
        AuditAction action,
        string entityType,
        string errorMessage,
        Guid? entityId = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            Action = action.ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public async Task<List<AuditLog>> GetAllLogsAsync(int? limit = null)
    {
        return await _auditLogRepository.GetAllAsync(limit);
    }

    public async Task<List<AuditLog>> GetLogsByUserAsync(Guid userId, int? limit = null)
    {
        return await _auditLogRepository.GetByUserAsync(userId, limit);
    }

    public async Task<List<AuditLog>> GetLogsByEntityAsync(string entityType, Guid entityId)
    {
        return await _auditLogRepository.GetByEntityAsync(entityType, entityId);
    }

    public async Task<List<AuditLog>> GetLogsByActionAsync(AuditAction action, int? limit = null)
    {
        return await _auditLogRepository.GetByActionAsync(action.ToString(), limit);
    }

    public async Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _auditLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<List<AuditLog>> GetFailedLogsAsync(int? limit = null)
    {
        return await _auditLogRepository.GetFailedAsync(limit);
    }

    public async Task DeleteOldLogsAsync(DateTime beforeDate)
    {
        await _auditLogRepository.DeleteOldAsync(beforeDate);
    }
}
