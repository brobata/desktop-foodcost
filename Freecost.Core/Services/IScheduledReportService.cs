using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IScheduledReportService
{
    /// <summary>
    /// Create a new scheduled report
    /// </summary>
    Task<ScheduledReport> CreateScheduledReportAsync(ScheduledReport report);

    /// <summary>
    /// Get all scheduled reports
    /// </summary>
    Task<List<ScheduledReport>> GetAllScheduledReportsAsync();

    /// <summary>
    /// Get active scheduled reports
    /// </summary>
    Task<List<ScheduledReport>> GetActiveScheduledReportsAsync();

    /// <summary>
    /// Get reports due to run
    /// </summary>
    Task<List<ScheduledReport>> GetDueReportsAsync();

    /// <summary>
    /// Update scheduled report
    /// </summary>
    Task UpdateScheduledReportAsync(ScheduledReport report);

    /// <summary>
    /// Delete scheduled report
    /// </summary>
    Task DeleteScheduledReportAsync(Guid id);

    /// <summary>
    /// Execute a scheduled report immediately
    /// </summary>
    Task<string> ExecuteScheduledReportAsync(Guid id);

    /// <summary>
    /// Calculate next run date based on schedule
    /// </summary>
    DateTime CalculateNextRunDate(ReportSchedule schedule, DateTime? lastRunDate = null);
}
