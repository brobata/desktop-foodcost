using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Freecost.Desktop.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    private const string PAYPAL_URL = "https://www.paypal.com/paypalme/FreeCostApp";
    private const string INSTAGRAM_URL = "https://www.instagram.com/FreecostApp/";

    private readonly IAutoUpdateService? _updateService;
    private readonly ILogger<AboutViewModel>? _logger;

    [ObservableProperty]
    private string _versionInfo;

    [ObservableProperty]
    private string _buildInfo;

    public AboutViewModel()
    {
        // Designer constructor
        VersionInfo = "v0.9.0";
        BuildInfo = "";
    }

    public AboutViewModel(IAutoUpdateService updateService, ILogger<AboutViewModel>? logger = null)
    {
        _updateService = updateService;
        _logger = logger;

        // Get version from assembly (SemVer format)
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            // Display as SemVer: v0.9.0
            VersionInfo = $"v{version.Major}.{version.Minor}.{version.Build}";
            BuildInfo = ""; // No longer using build numbers
        }
        else
        {
            VersionInfo = "Version Unknown";
            BuildInfo = "";
        }
    }

    [RelayCommand]
    private void OpenPayPal()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PAYPAL_URL,
                UseShellExecute = true
            });
            _logger?.LogInformation("Opened PayPal donation link");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open PayPal link");
        }
    }

    [RelayCommand]
    private void OpenInstagram()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = INSTAGRAM_URL,
                UseShellExecute = true
            });
            _logger?.LogInformation("Opened Instagram link");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open Instagram link");
        }
    }

    [RelayCommand]
    private void OpenBugReport()
    {
        try
        {
            var bugReportWindow = new Views.BugReportWindow();
            bugReportWindow.Show();
            _logger?.LogInformation("Opened bug report window");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open bug report window");
        }
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            if (_updateService == null)
            {
                _logger?.LogWarning("Update service not available");
                return;
            }

            _logger?.LogInformation("Checking for updates from About page");
            var result = await _updateService.CheckForUpdateAsync();

            // Get the correct logger type for UpdateNotificationViewModel
            var updateLogger = App.Services?.GetService(typeof(ILogger<UpdateNotificationViewModel>)) as ILogger<UpdateNotificationViewModel>;

            var viewModel = new UpdateNotificationViewModel(_updateService, updateLogger);
            viewModel.Initialize(result);

            var window = new Views.UpdateNotificationWindow
            {
                DataContext = viewModel
            };

            viewModel.CloseRequested += (s, e) => window.Close();

            window.Show();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to check for updates");
        }
    }
}
