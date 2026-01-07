using System;
using System.IO;
using System.Text;

namespace Dfc.Core.Services;

/// <summary>
/// Dedicated file logger for debugging
/// Writes detailed logs to a file that can be analyzed
/// </summary>
public static class SyncDebugLogger
{
    private static readonly string LogFilePath;
    private static readonly object _lock = new object();

    static SyncDebugLogger()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var freecostDir = Path.Combine(appDataPath, "Desktop Food Cost", "Logs");
        Directory.CreateDirectory(freecostDir);
        LogFilePath = Path.Combine(freecostDir, $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        // Write header
        WriteLog("=".PadRight(80, '='));
        WriteLog($"DEBUG LOG - Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLog($"Log file: {LogFilePath}");
        WriteLog("=".PadRight(80, '='));
    }

    public static string GetLogFilePath() => LogFilePath;

    public static void WriteLog(string message)
    {
        try
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logLine = $"[{timestamp}] {message}";
                File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
                System.Diagnostics.Debug.WriteLine($"[DEBUG_LOG] {message}");
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }

    public static void WriteError(string context, Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ERROR in {context}");
        sb.AppendLine($"   Exception Type: {ex.GetType().Name}");
        sb.AppendLine($"   Message: {ex.Message}");
        sb.AppendLine($"   Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            sb.AppendLine($"   Inner Exception: {ex.InnerException.GetType().Name}");
            sb.AppendLine($"   Inner Message: {ex.InnerException.Message}");
        }

        WriteLog(sb.ToString());
    }

    public static void WriteSection(string sectionName)
    {
        WriteLog("");
        WriteLog($">>> {sectionName} <<<");
    }

    public static void WriteSuccess(string message)
    {
        WriteLog($"SUCCESS: {message}");
    }

    public static void WriteWarning(string message)
    {
        WriteLog($"WARNING: {message}");
    }

    public static void WriteInfo(string message)
    {
        WriteLog($"INFO: {message}");
    }
}
