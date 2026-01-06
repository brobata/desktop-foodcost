using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly IApprovalWorkflowRepository _repository;

    public ApprovalWorkflowService(IApprovalWorkflowRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApprovalWorkflow> SubmitForApprovalAsync(
        string entityType,
        Guid entityId,
        string entityName,
        Guid submittedByUserId,
        string? changesSummary = null,
        ApprovalPriority priority = ApprovalPriority.Normal)
    {
        // Check if already pending approval
        var existingWorkflow = await _repository.GetByEntityAsync(entityType, entityId);
        if (existingWorkflow != null && existingWorkflow.Status == ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("This entity already has a pending approval");
        }

        var workflow = new ApprovalWorkflow
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            SubmittedByUserId = submittedByUserId,
            SubmittedAt = DateTime.UtcNow,
            Status = ApprovalStatus.Pending,
            ChangesSummary = changesSummary,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(workflow);
    }

    public async Task ApproveAsync(Guid workflowId, Guid reviewedByUserId, string? reviewNotes = null)
    {
        var workflow = await _repository.GetByIdAsync(workflowId);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        if (workflow.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending workflows can be approved");
        }

        workflow.Status = ApprovalStatus.Approved;
        workflow.ReviewedByUserId = reviewedByUserId;
        workflow.ReviewedAt = DateTime.UtcNow;
        workflow.ReviewNotes = reviewNotes;

        await _repository.UpdateAsync(workflow);
    }

    public async Task RejectAsync(Guid workflowId, Guid reviewedByUserId, string? reviewNotes = null)
    {
        var workflow = await _repository.GetByIdAsync(workflowId);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        if (workflow.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending workflows can be rejected");
        }

        workflow.Status = ApprovalStatus.Rejected;
        workflow.ReviewedByUserId = reviewedByUserId;
        workflow.ReviewedAt = DateTime.UtcNow;
        workflow.ReviewNotes = reviewNotes;

        await _repository.UpdateAsync(workflow);
    }

    public async Task RequestRevisionAsync(Guid workflowId, Guid reviewedByUserId, string reviewNotes)
    {
        var workflow = await _repository.GetByIdAsync(workflowId);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        if (workflow.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending workflows can request revision");
        }

        workflow.Status = ApprovalStatus.NeedsRevision;
        workflow.ReviewedByUserId = reviewedByUserId;
        workflow.ReviewedAt = DateTime.UtcNow;
        workflow.ReviewNotes = reviewNotes;

        await _repository.UpdateAsync(workflow);
    }

    public async Task CancelAsync(Guid workflowId)
    {
        var workflow = await _repository.GetByIdAsync(workflowId);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        workflow.Status = ApprovalStatus.Cancelled;
        await _repository.UpdateAsync(workflow);
    }

    public async Task<List<ApprovalWorkflow>> GetPendingApprovalsAsync()
    {
        return await _repository.GetByStatusAsync(ApprovalStatus.Pending);
    }

    public async Task<List<ApprovalWorkflow>> GetApprovalsByStatusAsync(ApprovalStatus status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async Task<List<ApprovalWorkflow>> GetSubmittedByUserAsync(Guid userId)
    {
        return await _repository.GetBySubmittedUserAsync(userId);
    }

    public async Task<List<ApprovalWorkflow>> GetReviewedByUserAsync(Guid userId)
    {
        return await _repository.GetByReviewedUserAsync(userId);
    }

    public async Task<ApprovalWorkflow?> GetWorkflowForEntityAsync(string entityType, Guid entityId)
    {
        return await _repository.GetByEntityAsync(entityType, entityId);
    }

    public async Task AddCommentAsync(Guid workflowId, Guid userId, string comment)
    {
        var workflow = await _repository.GetByIdAsync(workflowId);
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }

        var approvalComment = new ApprovalComment
        {
            Id = Guid.NewGuid(),
            ApprovalWorkflowId = workflowId,
            UserId = userId,
            Comment = comment,
            CommentedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        workflow.Comments.Add(approvalComment);
        await _repository.UpdateAsync(workflow);
    }

    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid workflowId)
    {
        return await _repository.GetByIdAsync(workflowId);
    }
}
