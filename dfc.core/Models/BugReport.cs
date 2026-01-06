using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a user-submitted bug report in the Supabase database
/// </summary>
[Table("bug_reports")]
public class BugReport : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // User info (auto-filled)
    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("user_email")]
    public string? UserEmail { get; set; }

    [Column("restaurant_name")]
    public string? RestaurantName { get; set; }

    [Column("location_id")]
    public Guid? LocationId { get; set; }

    // App info (auto-filled)
    [Column("app_version")]
    public string AppVersion { get; set; } = string.Empty;

    [Column("build_number")]
    public string? BuildNumber { get; set; }

    [Column("os_version")]
    public string? OsVersion { get; set; }

    // User input
    [Column("what_were_you_doing")]
    public string WhatWereYouDoing { get; set; } = string.Empty;

    [Column("additional_notes")]
    public string? AdditionalNotes { get; set; }

    // Auto-diagnostics
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("stack_trace")]
    public string? StackTrace { get; set; }

    [Column("diagnostic_json")]
    public string? DiagnosticJson { get; set; }

    // Tracking
    [Column("status")]
    public string Status { get; set; } = "new";

    [Column("priority")]
    public string Priority { get; set; } = "medium";

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("resolution_notes")]
    public string? ResolutionNotes { get; set; }
}
