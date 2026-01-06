using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freecost.Desktop.Services;

/// <summary>
/// Telemetry and crash reporting service
/// </summary>
public interface ITelemetryService
{
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);
    void TrackPageView(string pageName, TimeSpan? duration = null);
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    void TrackPerformance(string operationName, TimeSpan duration);
    Task FlushAsync();
    Task<List<TelemetryEvent>> GetRecentEventsAsync(int count = 100);
}

public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService>? _logger;
    private readonly string _telemetryPath;
    private readonly Queue<TelemetryEvent> _eventQueue = new();
    private readonly object _queueLock = new();
    private const int MaxQueueSize = 1000;
    private const int MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public TelemetryService(ILogger<TelemetryService>? logger = null)
    {
        _logger = logger;

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost",
            "Telemetry"
        );

        Directory.CreateDirectory(appDataPath);
        _telemetryPath = Path.Combine(appDataPath, $"telemetry_{DateTime.Now:yyyyMMdd}.json");
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        var telemetryEvent = new TelemetryEvent
        {
            Type = TelemetryEventType.Event,
            Name = eventName,
            Timestamp = DateTime.UtcNow,
            Properties = properties ?? new Dictionary<string, string>()
        };

        EnqueueEvent(telemetryEvent);
        _logger?.LogInformation("Telemetry Event: {EventName}", eventName);
    }

    public void TrackPageView(string pageName, TimeSpan? duration = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "PageName", pageName }
        };

        if (duration.HasValue)
        {
            properties["Duration"] = duration.Value.TotalMilliseconds.ToString("F2");
        }

        var telemetryEvent = new TelemetryEvent
        {
            Type = TelemetryEventType.PageView,
            Name = pageName,
            Timestamp = DateTime.UtcNow,
            Properties = properties
        };

        EnqueueEvent(telemetryEvent);
        _logger?.LogDebug("Page View: {PageName} ({Duration}ms)", pageName, duration?.TotalMilliseconds ?? 0);
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        var exceptionProperties = properties ?? new Dictionary<string, string>();
        exceptionProperties["ExceptionType"] = exception.GetType().Name;
        exceptionProperties["Message"] = exception.Message;
        exceptionProperties["StackTrace"] = exception.StackTrace ?? "No stack trace";

        if (exception.InnerException != null)
        {
            exceptionProperties["InnerException"] = exception.InnerException.Message;
        }

        var telemetryEvent = new TelemetryEvent
        {
            Type = TelemetryEventType.Exception,
            Name = exception.GetType().Name,
            Timestamp = DateTime.UtcNow,
            Properties = exceptionProperties,
            Severity = TelemetrySeverity.Error
        };

        EnqueueEvent(telemetryEvent);
        _logger?.LogError(exception, "Exception tracked: {ExceptionType}", exception.GetType().Name);
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        var metricProperties = properties ?? new Dictionary<string, string>();
        metricProperties["Value"] = value.ToString("F2");

        var telemetryEvent = new TelemetryEvent
        {
            Type = TelemetryEventType.Metric,
            Name = metricName,
            Timestamp = DateTime.UtcNow,
            Properties = metricProperties,
            MetricValue = value
        };

        EnqueueEvent(telemetryEvent);
        _logger?.LogDebug("Metric: {MetricName} = {Value}", metricName, value);
    }

    public void TrackPerformance(string operationName, TimeSpan duration)
    {
        var properties = new Dictionary<string, string>
        {
            { "Duration", duration.TotalMilliseconds.ToString("F2") },
            { "DurationSeconds", duration.TotalSeconds.ToString("F2") }
        };

        var telemetryEvent = new TelemetryEvent
        {
            Type = TelemetryEventType.Performance,
            Name = operationName,
            Timestamp = DateTime.UtcNow,
            Properties = properties,
            MetricValue = duration.TotalMilliseconds
        };

        EnqueueEvent(telemetryEvent);

        if (duration.TotalSeconds > 5)
        {
            _logger?.LogWarning("Slow Operation: {OperationName} took {Duration}s", operationName, duration.TotalSeconds);
        }
    }

    private void EnqueueEvent(TelemetryEvent telemetryEvent)
    {
        lock (_queueLock)
        {
            _eventQueue.Enqueue(telemetryEvent);

            // Limit queue size
            while (_eventQueue.Count > MaxQueueSize)
            {
                _eventQueue.Dequeue();
            }
        }

        // Auto-flush if queue is getting large
        if (_eventQueue.Count > MaxQueueSize * 0.8)
        {
            _ = FlushAsync();
        }
    }

    public async Task FlushAsync()
    {
        List<TelemetryEvent> eventsToWrite;

        lock (_queueLock)
        {
            if (_eventQueue.Count == 0) return;

            eventsToWrite = _eventQueue.ToList();
            _eventQueue.Clear();
        }

        try
        {
            // Check file size and rotate if needed
            if (File.Exists(_telemetryPath))
            {
                var fileInfo = new FileInfo(_telemetryPath);
                if (fileInfo.Length > MaxFileSize)
                {
                    var archivePath = Path.Combine(
                        Path.GetDirectoryName(_telemetryPath)!,
                        $"telemetry_{DateTime.Now:yyyyMMdd_HHmmss}_archive.json"
                    );
                    File.Move(_telemetryPath, archivePath);
                }
            }

            // Read existing events
            List<TelemetryEvent> existingEvents = new();
            if (File.Exists(_telemetryPath))
            {
                var json = await File.ReadAllTextAsync(_telemetryPath);
                existingEvents = JsonSerializer.Deserialize<List<TelemetryEvent>>(json) ?? new();
            }

            // Append new events
            existingEvents.AddRange(eventsToWrite);

            // Keep only last 10,000 events
            if (existingEvents.Count > 10000)
            {
                existingEvents = existingEvents.Skip(existingEvents.Count - 10000).ToList();
            }

            // Write back to file
            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(existingEvents, options);
            await File.WriteAllTextAsync(_telemetryPath, updatedJson);

            _logger?.LogDebug("Flushed {Count} telemetry events", eventsToWrite.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error flushing telemetry events");
        }
    }

    public async Task<List<TelemetryEvent>> GetRecentEventsAsync(int count = 100)
    {
        try
        {
            if (!File.Exists(_telemetryPath))
                return new List<TelemetryEvent>();

            var json = await File.ReadAllTextAsync(_telemetryPath);
            var events = JsonSerializer.Deserialize<List<TelemetryEvent>>(json) ?? new();

            return events.OrderByDescending(e => e.Timestamp).Take(count).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading telemetry events");
            return new List<TelemetryEvent>();
        }
    }
}

public enum TelemetryEventType
{
    Event,
    PageView,
    Exception,
    Metric,
    Performance
}

public enum TelemetrySeverity
{
    Verbose,
    Information,
    Warning,
    Error,
    Critical
}

public class TelemetryEvent
{
    public TelemetryEventType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
    public TelemetrySeverity Severity { get; set; } = TelemetrySeverity.Information;
    public double? MetricValue { get; set; }
}

/// <summary>
/// Performance tracking helper
/// </summary>
public class PerformanceTracker : IDisposable
{
    private readonly ITelemetryService _telemetryService;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;

    public PerformanceTracker(ITelemetryService telemetryService, string operationName)
    {
        _telemetryService = telemetryService;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _telemetryService.TrackPerformance(_operationName, _stopwatch.Elapsed);
    }
}

/// <summary>
/// Extension methods for telemetry
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Track performance of an operation
    /// </summary>
    public static PerformanceTracker TrackOperation(this ITelemetryService telemetryService, string operationName)
    {
        return new PerformanceTracker(telemetryService, operationName);
    }

    /// <summary>
    /// Track a user action
    /// </summary>
    public static void TrackUserAction(this ITelemetryService telemetryService, string action, string? details = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Action", action },
            { "Timestamp", DateTime.UtcNow.ToString("O") }
        };

        if (details != null)
        {
            properties["Details"] = details;
        }

        telemetryService.TrackEvent("UserAction", properties);
    }
}
