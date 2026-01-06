using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IAuditLogService
{
    /// <summary>
    /// Log an action performed by a user
    /// </summary>
    Task LogAsync(
        Guid userId,
        string userEmail,
        AuditAction action,
        string entityType,
        Guid? entityId = null,
        string? entityName = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Log a failed action
    /// </summary>
    Task LogFailureAsync(
        Guid userId,
        string userEmail,
        AuditAction action,
        string entityType,
        string errorMessage,
        Guid? entityId = null);

    /// <summary>
    /// Get all audit logs
    /// </summary>
    Task<List<AuditLog>> GetAllLogsAsync(int? limit = null);

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    Task<List<AuditLog>> GetLogsByUserAsync(Guid userId, int? limit = null);

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    Task<List<AuditLog>> GetLogsByEntityAsync(string entityType, Guid entityId);

    /// <summary>
    /// Get audit logs by action type
    /// </summary>
    Task<List<AuditLog>> GetLogsByActionAsync(AuditAction action, int? limit = null);

    /// <summary>
    /// Get audit logs within a date range
    /// </summary>
    Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get failed audit logs
    /// </summary>
    Task<List<AuditLog>> GetFailedLogsAsync(int? limit = null);

    /// <summary>
    /// Delete old audit logs (cleanup)
    /// </summary>
    Task DeleteOldLogsAsync(DateTime beforeDate);
}
