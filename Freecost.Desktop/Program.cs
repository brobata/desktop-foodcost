using System;
using System.IO;
using Avalonia;

namespace Freecost.Desktop
{
    internal sealed class Program
    {
        private static string GetLogFilePath()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "OneDrive", "Desktop", "Logs"
            );
            Directory.CreateDirectory(logDirectory);
            return Path.Combine(logDirectory, $"startup_log_{DateTime.Now:yyyyMMdd}.txt");
        }

        private static void LogMessage(string message)
        {
            try
            {
                var logPath = GetLogFilePath();
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // If logging fails, we can't do much about it
            }
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                LogMessage("=== Freecost Application Starting ===");
                LogMessage($"OS: {Environment.OSVersion}");
                LogMessage($".NET Version: {Environment.Version}");
                LogMessage($"64-bit Process: {Environment.Is64BitProcess}");
                LogMessage($"Working Directory: {Environment.CurrentDirectory}");
                LogMessage($"Command Line: {Environment.CommandLine}");

                LogMessage("Building Avalonia app...");
                var app = BuildAvaloniaApp();

                LogMessage("Starting classic desktop lifetime...");
                app.StartWithClassicDesktopLifetime(args);

                LogMessage("Application exited normally");
            }
            catch (Exception ex)
            {
                LogMessage($"FATAL ERROR: {ex.GetType().Name}: {ex.Message}");
                LogMessage($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    LogMessage($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    LogMessage($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                // Write error file to OneDrive Desktop/Logs for cross-machine visibility
                try
                {
                    var logPath = GetLogFilePath();
                    var logDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "OneDrive", "Desktop", "Logs"
                    );
                    Directory.CreateDirectory(logDirectory);

                    var errorFile = Path.Combine(logDirectory, $"FREECOST_ERROR_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                    var errorMessage = $@"===FREECOST FAILED TO START===
Time: {DateTime.Now}

Error: {ex.Message}

Stack Trace:
{ex.StackTrace}

Full Log File: {logPath}

=== COMMON FIXES ===
1. Install .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
2. Install Visual C++ Redistributable: https://aka.ms/vs/17/release/vc_redist.x64.exe
3. Check if antivirus blocked the app
4. Make sure you have admin rights (try Run as Administrator)
5. Check Windows Event Viewer for details

If error persists, send this file to support.

Error file location: {errorFile}
";
                    File.WriteAllText(errorFile, errorMessage);

                    // Also write to console with file location
                    Console.WriteLine(errorMessage);
                }
                catch
                {
                    // Last resort - console only
                    Console.WriteLine($"FATAL ERROR: {ex}");
                }

                Environment.Exit(1);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            try
            {
                LogMessage("Configuring Avalonia...");
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .WithInterFont()
                    .LogToTrace();
            }
            catch (Exception ex)
            {
                LogMessage($"Error building Avalonia app: {ex.Message}");
                throw;
            }
        }
    }
}
