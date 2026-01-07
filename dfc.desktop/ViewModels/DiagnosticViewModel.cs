using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Services;
using Dfc.Core.Repositories;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Dfc.Desktop.ViewModels;

/// <summary>
/// Diagnostic view model for troubleshooting local database issues
/// </summary>
public partial class DiagnosticViewModel : ViewModelBase
{
    private readonly IUserSessionService _sessionService;
    private readonly ILocationRepository _locationRepo;

    [ObservableProperty]
    private string _diagnosticOutput = "";

    [ObservableProperty]
    private string _logFilePath = "";

    public DiagnosticViewModel(
        IUserSessionService sessionService,
        ILocationRepository locationRepo)
    {
        _sessionService = sessionService;
        _locationRepo = locationRepo;
        _logFilePath = SyncDebugLogger.GetLogFilePath();
    }

    [RelayCommand]
    private async Task RunDiagnostic()
    {
        DiagnosticOutput = "Running diagnostic...\n\n";

        try
        {
            SyncDebugLogger.WriteSection("MANUAL DIAGNOSTIC RUN");

            // Check authentication status (always false in local-only mode)
            var isAuth = _sessionService.IsAuthenticated;
            var user = _sessionService.CurrentUser;

            AppendOutput($"Authentication Status: {isAuth}");
            AppendOutput("(Local-only mode - no cloud authentication)");
            SyncDebugLogger.WriteInfo($"IsAuthenticated: {isAuth}");

            if (user != null)
            {
                AppendOutput($"User Email: {user.Email}");
                AppendOutput($"User ID: {user.Id}");

                SyncDebugLogger.WriteInfo($"User Email: {user.Email}");
                SyncDebugLogger.WriteInfo($"User ID: {user.Id}");
            }
            else
            {
                AppendOutput("User: NULL (normal for local-only mode)");
                SyncDebugLogger.WriteInfo("CurrentUser is null (local-only mode)");
            }

            // Check local locations
            var localLocations = await _locationRepo.GetAllAsync();
            AppendOutput($"\nLocal Locations: {localLocations.Count}");
            SyncDebugLogger.WriteInfo($"Local locations count: {localLocations.Count}");

            foreach (var loc in localLocations)
            {
                AppendOutput($"  - {loc.Name} (ID: {loc.Id})");
                SyncDebugLogger.WriteInfo($"  Local location: {loc.Name} (ID: {loc.Id})");
            }

            AppendOutput($"\n✓ Diagnostic complete. Log saved to:\n{LogFilePath}");
            SyncDebugLogger.WriteInfo("Diagnostic complete");
        }
        catch (Exception ex)
        {
            AppendOutput($"\n❌ Diagnostic error: {ex.Message}");
            SyncDebugLogger.WriteError("RunDiagnostic", ex);
        }
    }

    private void AppendOutput(string text)
    {
        DiagnosticOutput += text + "\n";
    }
}
