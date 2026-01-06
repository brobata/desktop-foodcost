using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Diagnostic and fix tool for location sync issues
/// Use this to upload locations that were created locally but never synced to Supabase
/// </summary>
public class LocationSyncFixer
{
    private readonly ILocationRepository _locationRepo;
    private readonly IUserSessionService _sessionService;
    private readonly SupabaseDataService _dataService;
    private readonly ILogger<LocationSyncFixer>? _logger;

    public LocationSyncFixer(
        ILocationRepository locationRepo,
        IUserSessionService sessionService,
        SupabaseDataService dataService,
        ILogger<LocationSyncFixer>? logger = null)
    {
        _locationRepo = locationRepo;
        _sessionService = sessionService;
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// Diagnose location sync issues
    /// Returns diagnostic information about local vs remote locations
    /// </summary>
    public async Task<LocationSyncDiagnostic> DiagnoseAsync()
    {
        var diagnostic = new LocationSyncDiagnostic();

        try
        {
            // Check authentication
            if (!_sessionService.IsAuthenticated)
            {
                diagnostic.ErrorMessage = "Not authenticated";
                return diagnostic;
            }

            diagnostic.IsAuthenticated = true;
            diagnostic.CurrentUserEmail = _sessionService.CurrentUser?.Email;

            // Check if user has Supabase Auth UID
            var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
            diagnostic.HasSupabaseAuthUid = !string.IsNullOrEmpty(userId);
            diagnostic.SupabaseAuthUid = userId;

            // Count local locations
            var localLocations = await _locationRepo.GetAllAsync();
            diagnostic.LocalLocationCount = localLocations.Count;
            diagnostic.LocalLocationNames = localLocations.Select(l => l.Name).ToList();

            // Count Supabase locations
            if (diagnostic.HasSupabaseAuthUid)
            {
                var supabaseLocations = await _dataService.GetAllLocationsAsync();
                if (supabaseLocations.IsSuccess && supabaseLocations.Data != null)
                {
                    diagnostic.SupabaseLocationCount = supabaseLocations.Data.Count;
                    diagnostic.SupabaseLocationNames = supabaseLocations.Data.Select(l => l.Name).ToList();
                }
                else
                {
                    diagnostic.ErrorMessage = $"Failed to fetch Supabase locations: {supabaseLocations.Error}";
                }
            }

            diagnostic.SyncNeeded = diagnostic.LocalLocationCount > diagnostic.SupabaseLocationCount;
        }
        catch (Exception ex)
        {
            diagnostic.ErrorMessage = $"Diagnostic error: {ex.Message}";
            _logger?.LogError(ex, "Error during location sync diagnostic");
        }

        return diagnostic;
    }

    /// <summary>
    /// Force-upload all local locations to Supabase
    /// Use this to fix locations that were never synced
    /// </summary>
    public async Task<LocationSyncFixResult> FixAsync()
    {
        var result = new LocationSyncFixResult();

        try
        {
            // Check authentication
            if (!_sessionService.IsAuthenticated)
            {
                result.ErrorMessage = "Not authenticated. Please sign in.";
                return result;
            }

            // Check if user has Supabase Auth UID
            var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
            if (string.IsNullOrEmpty(userId))
            {
                result.ErrorMessage = "User has no Supabase Auth UID. Please sign out and sign in again to populate it.";
                return result;
            }

            Debug.WriteLine($"[LocationSyncFixer] Starting fix with user_id: {userId}");

            // Get all local locations
            var localLocations = await _locationRepo.GetAllAsync();

            if (!localLocations.Any())
            {
                result.Message = "No locations to upload";
                result.Success = true;
                return result;
            }

            Debug.WriteLine($"[LocationSyncFixer] Found {localLocations.Count} local locations to upload");

            // Upload each location to Supabase
            foreach (var location in localLocations)
            {
                try
                {
                    var supabaseLocation = new SupabaseLocation
                    {
                        Id = location.Id,
                        UserId = userId,
                        Name = location.Name,
                        Address = location.Address,
                        Phone = location.Phone,
                        IsActive = location.IsActive
                    };

                    var uploadResult = await _dataService.UpsertAsync(supabaseLocation);

                    if (uploadResult.IsSuccess)
                    {
                        result.UploadedCount++;
                        result.UploadedLocationNames.Add(location.Name);
                        Debug.WriteLine($"[LocationSyncFixer] ✓ Uploaded location: {location.Name}");
                    }
                    else
                    {
                        result.FailedCount++;
                        result.FailedLocationNames.Add($"{location.Name} ({uploadResult.Error})");
                        Debug.WriteLine($"[LocationSyncFixer] ❌ Failed to upload {location.Name}: {uploadResult.Error}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.FailedLocationNames.Add($"{location.Name} (Exception: {ex.Message})");
                    Debug.WriteLine($"[LocationSyncFixer] ❌ Exception uploading {location.Name}: {ex.Message}");
                }
            }

            result.Success = result.UploadedCount > 0;
            result.Message = $"Uploaded {result.UploadedCount} locations, {result.FailedCount} failed";

            _logger?.LogInformation(
                "LocationSyncFixer completed: {Uploaded} uploaded, {Failed} failed",
                result.UploadedCount,
                result.FailedCount);

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Fix error: {ex.Message}";
            _logger?.LogError(ex, "Error during location sync fix");
            return result;
        }
    }
}

/// <summary>
/// Diagnostic result for location sync
/// </summary>
public class LocationSyncDiagnostic
{
    public bool IsAuthenticated { get; set; }
    public string? CurrentUserEmail { get; set; }
    public bool HasSupabaseAuthUid { get; set; }
    public string? SupabaseAuthUid { get; set; }
    public int LocalLocationCount { get; set; }
    public int SupabaseLocationCount { get; set; }
    public List<string> LocalLocationNames { get; set; } = new();
    public List<string> SupabaseLocationNames { get; set; } = new();
    public bool SyncNeeded { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            return $"ERROR: {ErrorMessage}";
        }

        return $@"Location Sync Diagnostic:
- Authenticated: {IsAuthenticated}
- User Email: {CurrentUserEmail}
- Has Supabase Auth UID: {HasSupabaseAuthUid}
- Supabase Auth UID: {SupabaseAuthUid}
- Local Locations: {LocalLocationCount} ({string.Join(", ", LocalLocationNames)})
- Supabase Locations: {SupabaseLocationCount} ({string.Join(", ", SupabaseLocationNames)})
- Sync Needed: {SyncNeeded}";
    }
}

/// <summary>
/// Result of location sync fix operation
/// </summary>
public class LocationSyncFixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UploadedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> UploadedLocationNames { get; set; } = new();
    public List<string> FailedLocationNames { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            return $"ERROR: {ErrorMessage}";
        }

        var result = $@"Location Sync Fix Result:
- {Message}
- Uploaded: {UploadedCount}
- Failed: {FailedCount}";

        if (UploadedLocationNames.Any())
        {
            result += $"\n- Uploaded locations: {string.Join(", ", UploadedLocationNames)}";
        }

        if (FailedLocationNames.Any())
        {
            result += $"\n- Failed locations: {string.Join(", ", FailedLocationNames)}";
        }

        return result;
    }
}
