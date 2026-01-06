using System;
using System.IO;

namespace Freecost.Core.Helpers;

/// <summary>
/// Thread-safe file logging utility that gracefully handles errors
/// to prevent UI thread blocking during file I/O operations
/// </summary>
public static class SafeFileLogger
{
    private static readonly object _lock = new object();

    /// <summary>
    /// Safely logs a message to the specified log file category
    /// </summary>
    /// <param name="category">Log category (e.g., "auth", "sync", "startup", "signout")</param>
    /// <param name="message">Message to log</param>
    public static void Log(string category, string message)
    {
        try
        {
            lock (_lock)
            {
                var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");
                Directory.CreateDirectory(logFolder); // Ensures directory exists
                var logFile = Path.Combine(logFolder, $"{category}_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
            }
        }
        catch
        {
            // Silently ignore logging errors to prevent UI freezes
            // Application continues normally even if logging fails
        }
    }
}
