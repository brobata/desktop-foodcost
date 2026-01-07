using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dfc.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Service for submitting bug reports
/// </summary>
public interface IBugReportService
{
    /// <summary>
    /// Submit a bug report with user description and optional exception info
    /// </summary>
    Task<bool> SubmitBugReportAsync(string whatWereYouDoing, string? additionalNotes = null, Exception? exception = null);

    /// <summary>
    /// Get all bug reports
    /// </summary>
    Task<List<BugReport>> GetAllBugReportsAsync();
}

/// <summary>
/// Local-only bug report service that saves reports to a local file.
/// No cloud submission - reports are stored locally for user review.
/// </summary>
public class LocalBugReportService : IBugReportService
{
    private readonly ILogger<LocalBugReportService>? _logger;
    private readonly string _bugReportsPath;

    public LocalBugReportService(ILogger<LocalBugReportService>? logger = null)
    {
        _logger = logger;
        _bugReportsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost",
            "BugReports"
        );
        Directory.CreateDirectory(_bugReportsPath);
    }

    public async Task<bool> SubmitBugReportAsync(string whatWereYouDoing, string? additionalNotes = null, Exception? exception = null)
    {
        try
        {
            var report = new BugReport
            {
                Id = Guid.NewGuid(),
                WhatWereYouDoing = whatWereYouDoing,
                AdditionalNotes = additionalNotes,
                ExceptionMessage = exception?.Message,
                ExceptionStackTrace = exception?.StackTrace,
                CreatedAt = DateTime.UtcNow,
                AppVersion = GetAppVersion(),
                OsVersion = Environment.OSVersion.ToString()
            };

            var fileName = $"bug_report_{report.CreatedAt:yyyyMMdd_HHmmss}_{report.Id:N}.json";
            var filePath = Path.Combine(_bugReportsPath, fileName);

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            _logger?.LogInformation("Bug report saved locally: {FilePath}", filePath);
            Debug.WriteLine($"[LocalBugReportService] Bug report saved: {filePath}");

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save bug report locally");
            Debug.WriteLine($"[LocalBugReportService] Failed to save bug report: {ex.Message}");
            return false;
        }
    }

    public async Task<List<BugReport>> GetAllBugReportsAsync()
    {
        var reports = new List<BugReport>();

        try
        {
            if (!Directory.Exists(_bugReportsPath))
                return reports;

            var files = Directory.GetFiles(_bugReportsPath, "bug_report_*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var report = JsonSerializer.Deserialize<BugReport>(json);
                    if (report != null)
                        reports.Add(report);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to read bug report file: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get bug reports");
        }

        return reports;
    }

    private string GetAppVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
