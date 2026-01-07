using System;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a user-submitted bug report
/// </summary>
public class BugReport
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    // User info
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? RestaurantName { get; set; }
    public Guid? LocationId { get; set; }

    // App info
    public string AppVersion { get; set; } = string.Empty;
    public string? BuildNumber { get; set; }
    public string? OsVersion { get; set; }

    // User input
    public string WhatWereYouDoing { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }

    // Exception info
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? DiagnosticJson { get; set; }

    // Tracking
    public string Status { get; set; } = "new";
    public string Priority { get; set; } = "medium";
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}
