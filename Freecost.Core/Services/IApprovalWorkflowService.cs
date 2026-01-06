using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IApprovalWorkflowService
{
    /// <summary>
    /// Submit an entity for approval
    /// </summary>
    Task<ApprovalWorkflow> SubmitForApprovalAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid submittedByUserId,
        string? changesSummary = null,
        ApprovalPriority priority = ApprovalPriority.Normal);

    /// <summary>
    /// Approve a pending workflow
    /// </summary>
    Task ApproveAsync(Guid workflowId, Guid reviewedByUserId, string? reviewNotes = null);

    /// <summary>
    /// Reject a pending workflow
    /// </summary>
    Task RejectAsync(Guid workflowId, Guid reviewedByUserId, string? reviewNotes = null);

    /// <summary>
    /// Request revision on a workflow
    /// </summary>
    Task RequestRevisionAsync(Guid workflowId, Guid reviewedByUserId, string reviewNotes);

    /// <summary>
    /// Cancel a workflow
    /// </summary>
    Task CancelAsync(Guid workflowId);

    /// <summary>
    /// Get all pending approvals
    /// </summary>
    Task<List<ApprovalWorkflow>> GetPendingApprovalsAsync();

    /// <summary>
    /// Get approvals by status
    /// </summary>
    Task<List<ApprovalWorkflow>> GetApprovalsByStatusAsync(ApprovalStatus status);

    /// <summary>
    /// Get approvals submitted by a user
    /// </summary>
    Task<List<ApprovalWorkflow>> GetSubmittedByUserAsync(Guid userId);

    /// <summary>
    /// Get approvals reviewed by a user
    /// </summary>
    Task<List<ApprovalWorkflow>> GetReviewedByUserAsync(Guid userId);

    /// <summary>
    /// Get approval workflow for an entity
    /// </summary>
    Task<ApprovalWorkflow?> GetWorkflowForEntityAsync(string entityType, Guid entityId);

    /// <summary>
    /// Add comment to approval workflow
    /// </summary>
    Task AddCommentAsync(Guid workflowId, Guid userId, string comment);

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    Task<ApprovalWorkflow?> GetByIdAsync(Guid workflowId);
}
