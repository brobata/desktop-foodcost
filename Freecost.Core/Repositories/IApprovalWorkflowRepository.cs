using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IApprovalWorkflowRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid id);
    Task<ApprovalWorkflow?> GetByEntityAsync(string entityType, Guid entityId);
    Task<List<ApprovalWorkflow>> GetByStatusAsync(ApprovalStatus status);
    Task<List<ApprovalWorkflow>> GetBySubmittedUserAsync(Guid userId);
    Task<List<ApprovalWorkflow>> GetByReviewedUserAsync(Guid userId);
    Task<ApprovalWorkflow> CreateAsync(ApprovalWorkflow workflow);
    Task UpdateAsync(ApprovalWorkflow workflow);
    Task DeleteAsync(Guid id);
}
