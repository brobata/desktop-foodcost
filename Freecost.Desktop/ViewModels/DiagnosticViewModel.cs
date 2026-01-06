using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;
using Freecost.Core.Repositories;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class DiagnosticViewModel : ViewModelBase
{
    private readonly IUserSessionService _sessionService;
    private readonly ILocationRepository _locationRepo;
    private readonly SupabaseDataService _dataService;

    [ObservableProperty]
    private string _diagnosticOutput = "";

    [ObservableProperty]
    private string _logFilePath = "";

    public DiagnosticViewModel(
        IUserSessionService sessionService,
        ILocationRepository locationRepo,
        SupabaseDataService dataService)
    {
        _sessionService = sessionService;
        _locationRepo = locationRepo;
        _dataService = dataService;
        _logFilePath = SyncDebugLogger.GetLogFilePath();
    }

    [RelayCommand]
    private async Task RunDiagnostic()
    {
        DiagnosticOutput = "Running diagnostic...\n\n";

        try
        {
            SyncDebugLogger.WriteSection("MANUAL DIAGNOSTIC RUN");

            // Check authentication
            var isAuth = _sessionService.IsAuthenticated;
            var user = _sessionService.CurrentUser;

            AppendOutput($"Authentication Status: {isAuth}");
            SyncDebugLogger.WriteInfo($"IsAuthenticated: {isAuth}");

            if (user != null)
            {
                AppendOutput($"User Email: {user.Email}");
                AppendOutput($"User ID (local): {user.Id}");
                AppendOutput($"Supabase Auth UID (SupabaseAuthUid): {user.SupabaseAuthUid ?? "NULL - THIS IS THE PROBLEM!"}");

                SyncDebugLogger.WriteInfo($"User Email: {user.Email}");
                SyncDebugLogger.WriteInfo($"User ID (local): {user.Id}");
                SyncDebugLogger.WriteInfo($"Supabase Auth UID: {user.SupabaseAuthUid ?? "NULL"}");

                if (string.IsNullOrEmpty(user.SupabaseAuthUid))
                {
                    AppendOutput("\n❌ PROBLEM FOUND: User has no Supabase Auth UID!");
                    AppendOutput("FIX: Sign out and sign back in to populate it.\n");
                    SyncDebugLogger.WriteError("Diagnostic", new Exception("User.SupabaseAuthUid is null - user needs to sign out/in"));
                }
            }
            else
            {
                AppendOutput("User: NULL");
                SyncDebugLogger.WriteWarning("CurrentUser is null");
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

            // Check Supabase locations
            if (isAuth && user?.SupabaseAuthUid != null)
            {
                AppendOutput("\nChecking Supabase locations...");
                SyncDebugLogger.WriteInfo("Fetching Supabase locations...");

                var supabaseResult = await _dataService.GetAllLocationsAsync();

                if (supabaseResult.IsSuccess && supabaseResult.Data != null)
                {
                    AppendOutput($"Supabase Locations: {supabaseResult.Data.Count}");
                    SyncDebugLogger.WriteInfo($"Supabase locations count: {supabaseResult.Data.Count}");

                    foreach (var loc in supabaseResult.Data)
                    {
                        AppendOutput($"  - {loc.Name} (ID: {loc.Id}, UserID: {loc.UserId})");
                        SyncDebugLogger.WriteInfo($"  Supabase location: {loc.Name} (ID: {loc.Id}, UserID: {loc.UserId})");
                    }

                    if (localLocations.Count > supabaseResult.Data.Count)
                    {
                        AppendOutput($"\n⚠ SYNC ISSUE: {localLocations.Count} local locations but only {supabaseResult.Data.Count} in Supabase");
                        SyncDebugLogger.WriteWarning($"Sync mismatch: {localLocations.Count} local vs {supabaseResult.Data.Count} remote");
                    }
                }
                else
                {
                    AppendOutput($"❌ Failed to fetch Supabase locations: {supabaseResult.Error}");
                    SyncDebugLogger.WriteError("GetAllLocationsAsync", new Exception(supabaseResult.Error ?? "Unknown"));
                }
            }
            else
            {
                AppendOutput("\nCannot check Supabase locations - not authenticated or no Auth UID");
                SyncDebugLogger.WriteWarning("Cannot check Supabase - not authenticated or no Auth UID");
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
