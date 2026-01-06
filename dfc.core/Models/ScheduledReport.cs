using System;

namespace Dfc.Core.Models;

public class ScheduledReport : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty; // "CostAnalysis", "Profitability", etc.
    public string Parameters { get; set; } = string.Empty; // JSON serialized parameters
    public ReportSchedule Schedule { get; set; } = ReportSchedule.Daily;
    public DateTime? LastRunDate { get; set; }
    public DateTime? NextRunDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string OutputFormat { get; set; } = "PDF"; // PDF, Excel, CSV
    public string? EmailRecipients { get; set; } // Comma-separated emails
    public string? OutputPath { get; set; } // Where to save the report
}

public enum ReportSchedule
{
    Daily,
    Weekly,
    Monthly,
    Quarterly
}
