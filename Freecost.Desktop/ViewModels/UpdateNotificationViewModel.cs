using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;
using Microsoft.Extensions.Logging;

namespace Freecost.Desktop.ViewModels;

public partial class UpdateNotificationViewModel : ObservableObject
{
    private readonly IAutoUpdateService _updateService;
    private readonly ILogger<UpdateNotificationViewModel>? _logger;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string? _latestVersion;

    [ObservableProperty]
    private string? _currentVersion;

    [ObservableProperty]
    private string? _releaseNotes;

    [ObservableProperty]
    private string? _downloadUrl;

    [ObservableProperty]
    private string? _publishedDate;

    [ObservableProperty]
    private string? _downloadSize;

    [ObservableProperty]
    private bool _hasDownloadSize;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private int _downloadProgress;

    [ObservableProperty]
    private string _downloadButtonText = "Download & Install";

    [ObservableProperty]
    private string _statusMessage = "Checking for updates...";

    [ObservableProperty]
    private bool _isError;
    public event EventHandler? CloseRequested;

    public UpdateNotificationViewModel(IAutoUpdateService updateService, ILogger<UpdateNotificationViewModel>? logger = null)
    {
        _updateService = updateService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize with update check result
    /// </summary>
    public void Initialize(UpdateCheckResult result)
    {
        IsUpdateAvailable = result.IsUpdateAvailable;
        LatestVersion = result.LatestVersion;
        CurrentVersion = result.CurrentVersion;
        ReleaseNotes = result.ReleaseNotes ?? "No release notes available.";
        DownloadUrl = result.DownloadUrl;

        if (result.PublishedAt.HasValue)
        {
            PublishedDate = result.PublishedAt.Value.ToString("MMMM dd, yyyy");
        }

        // Parse download size from asset name if available
        if (result.AssetName != null)
        {
            // This is a placeholder - would need actual size from API
            HasDownloadSize = false;
        }

        if (!result.IsUpdateAvailable)
        {
            StatusMessage = result.Message ?? "You are running the latest version.";
            // Check if this is an error or actually up to date
            IsError = result.Message != null && (result.Message.Contains("Failed") || result.Message.Contains("Error"));
        }
    }

    [RelayCommand]
    private async Task DownloadAndInstallAsync()
    {
        System.Diagnostics.Debug.WriteLine("========== DOWNLOAD & INSTALL STARTED ==========");
        System.Diagnostics.Debug.WriteLine($"Download URL: {DownloadUrl}");

        if (string.IsNullOrEmpty(DownloadUrl))
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Download URL is empty!");
            _logger?.LogWarning("Download URL is empty");
            return;
        }

        // Open progress window
        Freecost.Desktop.Views.DownloadProgressWindow? progressWindow = null;

        try
        {
            System.Diagnostics.Debug.WriteLine("Setting IsDownloading = true");
            IsDownloading = true;
            DownloadButtonText = "Downloading...";
            DownloadProgress = 0;

            // Show progress window
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                progressWindow = new Freecost.Desktop.Views.DownloadProgressWindow(this);
                progressWindow.Show();
            });

            var progress = new Progress<int>(percent =>
            {
                DownloadProgress = percent;
                System.Diagnostics.Debug.WriteLine($"Download progress: {percent}%");
            });

            System.Diagnostics.Debug.WriteLine("Calling DownloadUpdateAsync...");
            // Download the update
            var downloadResult = await _updateService.DownloadUpdateAsync(DownloadUrl, progress);

            System.Diagnostics.Debug.WriteLine($"Download completed. Success: {downloadResult.IsSuccess}");
            if (!downloadResult.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Download failed - {downloadResult.ErrorMessage}");
                _logger?.LogError("Failed to download update: {Error}", downloadResult.ErrorMessage);
                StatusMessage = $"Download failed: {downloadResult.ErrorMessage}";
                IsDownloading = false;
                DownloadButtonText = "Download & Install";

                // Close progress window
                progressWindow?.Close();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Download successful! File: {downloadResult.FilePath}");

            // Install the update
            DownloadButtonText = "Installing...";
            StatusMessage = "Installing update...";
            System.Diagnostics.Debug.WriteLine("Calling InstallUpdateAsync...");
            var installSuccess = await _updateService.InstallUpdateAsync(downloadResult.FilePath!);

            System.Diagnostics.Debug.WriteLine($"Install result: {installSuccess}");
            if (installSuccess)
            {
                System.Diagnostics.Debug.WriteLine("Install started successfully - preparing to exit app");
                _logger?.LogInformation("Update installation started successfully");
                StatusMessage = "Update is being installed. The application will close.";

                // Close progress window
                progressWindow?.Close();

                // Give user a moment to see the message, then close
                await Task.Delay(1500);
                CloseRequested?.Invoke(this, EventArgs.Empty);

                // Exit the application to allow installer to run
                System.Diagnostics.Debug.WriteLine("Exiting application...");
                Environment.Exit(0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Install failed");
                _logger?.LogError("Failed to install update");
                StatusMessage = "Installation failed. Please try again.";
                IsDownloading = false;
                DownloadButtonText = "Download & Install";

                // Close progress window
                progressWindow?.Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"========== EXCEPTION IN DOWNLOAD & INSTALL ==========");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("========================================================");

            _logger?.LogError(ex, "Error during download and install");
            StatusMessage = $"Error: {ex.Message}";
            IsDownloading = false;
            DownloadButtonText = "Download & Install";

            // Close progress window
            progressWindow?.Close();
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("========== DOWNLOAD & INSTALL COMPLETE ==========");
        }
    }

    [RelayCommand]
    private void ViewOnGitHub()
    {
        _updateService.OpenReleasesPage();
    }

    [RelayCommand]
    private void RemindLater()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
