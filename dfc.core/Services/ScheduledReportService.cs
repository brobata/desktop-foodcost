using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class ScheduledReportService : IScheduledReportService
{
    private readonly string _configPath;
    private List<ScheduledReport> _scheduledReports;

    public ScheduledReportService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );

        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "scheduled_reports.json");
        _scheduledReports = LoadReports();
    }

    public async Task<ScheduledReport> CreateScheduledReportAsync(ScheduledReport report)
    {
        report.Id = Guid.NewGuid();
        report.CreatedAt = DateTime.UtcNow;
        report.ModifiedAt = DateTime.UtcNow;
        report.NextRunDate = CalculateNextRunDate(report.Schedule, report.LastRunDate);

        _scheduledReports.Add(report);
        await SaveReportsAsync();

        return report;
    }

    public async Task<List<ScheduledReport>> GetAllScheduledReportsAsync()
    {
        return await Task.FromResult(_scheduledReports.ToList());
    }

    public async Task<List<ScheduledReport>> GetActiveScheduledReportsAsync()
    {
        return await Task.FromResult(_scheduledReports.Where(r => r.IsActive).ToList());
    }

    public async Task<List<ScheduledReport>> GetDueReportsAsync()
    {
        var now = DateTime.UtcNow;
        return await Task.FromResult(_scheduledReports
            .Where(r => r.IsActive && r.NextRunDate.HasValue && r.NextRunDate.Value <= now)
            .ToList());
    }

    public async Task UpdateScheduledReportAsync(ScheduledReport report)
    {
        var existingIndex = _scheduledReports.FindIndex(r => r.Id == report.Id);
        if (existingIndex >= 0)
        {
            report.ModifiedAt = DateTime.UtcNow;
            _scheduledReports[existingIndex] = report;
            await SaveReportsAsync();
        }
    }

    public async Task DeleteScheduledReportAsync(Guid id)
    {
        _scheduledReports.RemoveAll(r => r.Id == id);
        await SaveReportsAsync();
    }

    public async Task<string> ExecuteScheduledReportAsync(Guid id)
    {
        var report = _scheduledReports.FirstOrDefault(r => r.Id == id);
        if (report == null)
        {
            throw new InvalidOperationException($"Scheduled report with ID {id} not found");
        }

        // TODO: Implement actual report execution based on report type
        // This would integrate with existing report services

        report.LastRunDate = DateTime.UtcNow;
        report.NextRunDate = CalculateNextRunDate(report.Schedule, report.LastRunDate);
        await SaveReportsAsync();

        return $"Report '{report.Name}' executed successfully. Next run: {report.NextRunDate}";
    }

    public DateTime CalculateNextRunDate(ReportSchedule schedule, DateTime? lastRunDate = null)
    {
        var baseDate = lastRunDate ?? DateTime.UtcNow;

        return schedule switch
        {
            ReportSchedule.Daily => baseDate.AddDays(1),
            ReportSchedule.Weekly => baseDate.AddDays(7),
            ReportSchedule.Monthly => baseDate.AddMonths(1),
            ReportSchedule.Quarterly => baseDate.AddMonths(3),
            _ => baseDate.AddDays(1)
        };
    }

    private List<ScheduledReport> LoadReports()
    {
        if (!File.Exists(_configPath))
        {
            return new List<ScheduledReport>();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<List<ScheduledReport>>(json) ?? new List<ScheduledReport>();
        }
        catch
        {
            return new List<ScheduledReport>();
        }
    }

    private async Task SaveReportsAsync()
    {
        var json = JsonSerializer.Serialize(_scheduledReports, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_configPath, json);
    }
}
