using System;
using System.Collections.Generic;

namespace Freecost.Core.Models;

public class ApprovalWorkflow : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Recipe, Ingredient, Entree
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid SubmittedByUserId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public string? ChangesSummary { get; set; } // JSON of changes
    public ApprovalPriority Priority { get; set; } = ApprovalPriority.Normal;

    // Navigation
    public User? SubmittedByUser { get; set; }
    public User? ReviewedByUser { get; set; }
    public List<ApprovalComment> Comments { get; set; } = new();
}

public class ApprovalComment : BaseEntity
{
    public Guid ApprovalWorkflowId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApprovalWorkflow? ApprovalWorkflow { get; set; }
    public User? User { get; set; }
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    NeedsRevision = 3,
    Cancelled = 4
}

public enum ApprovalPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
