using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetAllAsync(int? limit = null);
    Task<List<AuditLog>> GetByUserAsync(Guid userId, int? limit = null);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
    Task<List<AuditLog>> GetByActionAsync(string action, int? limit = null);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<AuditLog>> GetFailedAsync(int? limit = null);
    Task DeleteOldAsync(DateTime beforeDate);
}
