using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Freecost.Core.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;
    private readonly IUserSessionService? _sessionService;
    private readonly SupabaseDataService? _dataService;
    private readonly ILogger<LocationService>? _logger;

    public LocationService(
        ILocationRepository repository,
        IUserSessionService? sessionService = null,
        SupabaseDataService? dataService = null,
        ILogger<LocationService>? logger = null)
    {
        _repository = repository;
        _sessionService = sessionService;
        _dataService = dataService;
        _logger = logger;
    }

    private void LogError(string message, Exception? ex = null)
    {
        Debug.WriteLine("    ╔═══════════════════════════════════════════════════╗");
        Debug.WriteLine("    ║ [LOCATION SERVICE ERROR]                          ║");
        Debug.WriteLine("    ╠═══════════════════════════════════════════════════╣");
        Debug.WriteLine($"    {message}");
        if (ex != null)
        {
            Debug.WriteLine($"    Exception Type: {ex.GetType().Name}");
            Debug.WriteLine($"    Message: {ex.Message}");
            Debug.WriteLine($"    Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
                Debug.WriteLine($"    Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        Debug.WriteLine("    ╚═══════════════════════════════════════════════════╝");
    }

    public async Task<List<Location>> GetAllLocationsAsync()
    {
        Debug.WriteLine("=== LocationService.GetAllLocationsAsync START ===");

        // Get local locations (sync happens via SupabaseSyncService.SyncRestaurantsAsync)
        var locations = await _repository.GetAllAsync();
        Debug.WriteLine($"[LocationService.GetAll] Found {locations.Count} local locations");

        Debug.WriteLine($"=== LocationService.GetAllLocationsAsync END - Returning {locations.Count} locations ===");
        return locations;
    }

    public async Task<Location?> GetLocationByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Location> CreateLocationAsync(Location location)
    {
        SyncDebugLogger.WriteSection($"CREATE LOCATION: {location.Name}");

        if (string.IsNullOrWhiteSpace(location.Name))
        {
            SyncDebugLogger.WriteError("CreateLocationAsync", new ArgumentException("Location name is required"));
            throw new ArgumentException("Location name is required");
        }

        location.Id = Guid.NewGuid();
        SyncDebugLogger.WriteInfo($"Generated location ID: {location.Id}");

        // Save to local database
        var createdLocation = await _repository.AddAsync(location);
        SyncDebugLogger.WriteSuccess($"Saved location '{location.Name}' to local database");

        // Sync to Supabase if authenticated and data service available
        SyncDebugLogger.WriteInfo($"Checking sync conditions:");
        SyncDebugLogger.WriteInfo($"  IsAuthenticated: {_sessionService?.IsAuthenticated}");
        SyncDebugLogger.WriteInfo($"  DataService exists: {_dataService != null}");

        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                // IMPORTANT: Use SupabaseAuthUid (which stores Supabase Auth UID), not local User.Id
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                SyncDebugLogger.WriteInfo($"  User Email: {_sessionService.CurrentUser?.Email}");
                SyncDebugLogger.WriteInfo($"  Supabase Auth UID (stored in SupabaseAuthUid): {userId ?? "NULL - THIS IS THE PROBLEM"}");

                if (string.IsNullOrEmpty(userId))
                {
                    SyncDebugLogger.WriteWarning("❌ SYNC FAILED: User has no Supabase Auth UID");
                    Debug.WriteLine("[LocationService.Create] No Supabase Auth UID available for sync");
                    _logger?.LogWarning("Cannot sync location to Supabase: User has no Supabase Auth UID");
                    return createdLocation;
                }

                Debug.WriteLine($"[LocationService.Create] Syncing location to Supabase with user_id: {userId}");
                SyncDebugLogger.WriteInfo($"Starting Supabase sync with user_id: {userId}");

                // Create Supabase location model
                var supabaseLocation = new SupabaseLocation
                {
                    Id = location.Id,
                    UserId = userId, // Supabase Auth UID
                    Name = location.Name,
                    Address = location.Address,
                    Phone = location.Phone,
                    IsActive = location.IsActive
                };

                SyncDebugLogger.WriteInfo($"Calling SupabaseDataService.UpsertAsync...");
                var result = await _dataService.UpsertAsync(supabaseLocation);

                if (!result.IsSuccess)
                {
                    SyncDebugLogger.WriteError("Supabase upsert failed", new Exception(result.Error ?? "Unknown error"));
                    Debug.WriteLine($"[LocationService.Create] ❌ Failed to sync to Supabase: {result.Error}");
                    _logger?.LogWarning("Failed to sync location to Supabase: {Error}", result.Error);
                }
                else
                {
                    SyncDebugLogger.WriteSuccess($"✓ Location '{location.Name}' synced to Supabase!");
                    Debug.WriteLine($"[LocationService.Create] ✓ Location synced to Supabase: {location.Name}");
                    _logger?.LogInformation("Location synced to Supabase: {LocationName}", location.Name);
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError("CreateLocationAsync - Supabase sync exception", ex);
                Debug.WriteLine($"[LocationService.Create] ❌ Exception syncing to Supabase: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to sync location to Supabase");
            }
        }
        else
        {
            SyncDebugLogger.WriteWarning($"❌ SYNC SKIPPED - Not authenticated or no data service");
            Debug.WriteLine($"[LocationService.Create] Sync skipped - Authenticated: {_sessionService?.IsAuthenticated}, DataService exists: {_dataService != null}");
        }

        SyncDebugLogger.WriteSection("CREATE LOCATION COMPLETE");
        return createdLocation;
    }

    public async Task<Location> UpdateLocationAsync(Location location)
    {
        if (string.IsNullOrWhiteSpace(location.Name))
            throw new ArgumentException("Location name is required");

        var exists = await _repository.ExistsAsync(location.Id);
        if (!exists)
            throw new InvalidOperationException("Location not found");

        // Update local database
        var updatedLocation = await _repository.UpdateAsync(location);

        // Sync to Supabase if authenticated and data service available
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                // IMPORTANT: Use SupabaseAuthUid (which stores Supabase Auth UID), not local User.Id
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine("[LocationService.Update] No Supabase Auth UID available for sync");
                    _logger?.LogWarning("Cannot sync location update to Supabase: User has no Supabase Auth UID");
                    return updatedLocation;
                }

                Debug.WriteLine($"[LocationService.Update] Syncing location update to Supabase with user_id: {userId}");

                // Create Supabase location model
                var supabaseLocation = new SupabaseLocation
                {
                    Id = location.Id,
                    UserId = userId, // Supabase Auth UID
                    Name = location.Name,
                    Address = location.Address,
                    Phone = location.Phone,
                    IsActive = location.IsActive
                };

                var result = await _dataService.UpsertAsync(supabaseLocation);

                if (!result.IsSuccess)
                {
                    Debug.WriteLine($"[LocationService.Update] ❌ Failed to sync to Supabase: {result.Error}");
                    _logger?.LogWarning("Failed to sync location update to Supabase: {Error}", result.Error);
                }
                else
                {
                    Debug.WriteLine($"[LocationService.Update] ✓ Location update synced to Supabase: {location.Name}");
                    _logger?.LogInformation("Location update synced to Supabase: {LocationName}", location.Name);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocationService.Update] ❌ Exception syncing to Supabase: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to sync location update to Supabase");
            }
        }

        return updatedLocation;
    }

    public async Task DeleteLocationAsync(Guid id)
    {
        Debug.WriteLine($"=== LocationService.DeleteLocationAsync START - ID: {id} ===");

        var exists = await _repository.ExistsAsync(id);
        Debug.WriteLine($"[LocationService.Delete] Location exists locally: {exists}");

        if (!exists)
        {
            Debug.WriteLine($"[LocationService.Delete] ERROR: Location not found");
            throw new InvalidOperationException("Location not found");
        }

        // Delete from local database
        await _repository.DeleteAsync(id);
        Debug.WriteLine($"[LocationService.Delete] Deleted from local database");

        // Sync deletion to Supabase if authenticated and data service available
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            Debug.WriteLine($"[LocationService.Delete] User is authenticated, attempting Supabase sync");
            try
            {
                // IMPORTANT: Use SupabaseAuthUid (which stores Supabase Auth UID), not local User.Id
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine("[LocationService.Delete] No Supabase Auth UID available for sync");
                    _logger?.LogWarning("Cannot delete location from Supabase: User has no Supabase Auth UID");
                    return;
                }

                Debug.WriteLine($"[LocationService.Delete] Attempting to delete from Supabase: {id}");

                var result = await _dataService.DeleteAsync<SupabaseLocation>(id);

                Debug.WriteLine($"[LocationService.Delete] Supabase delete result - IsSuccess: {result.IsSuccess}");

                if (!result.IsSuccess)
                {
                    Debug.WriteLine($"[LocationService.Delete] ❌ Supabase delete FAILED: {result.Error}");
                    _logger?.LogWarning("Failed to delete location from Supabase: {Error}", result.Error);
                }
                else
                {
                    Debug.WriteLine($"[LocationService.Delete] ✓ Successfully deleted from Supabase");
                    _logger?.LogInformation("Location deleted from Supabase: {LocationId}", id);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocationService.Delete] ❌ Exception deleting from Supabase: {ex.Message}");
                LogError($"Failed to delete location {id} from Supabase", ex);
                _logger?.LogWarning(ex, "Failed to delete location from Supabase");
            }
        }
        else
        {
            Debug.WriteLine($"[LocationService.Delete] Sync skipped - Authenticated: {_sessionService?.IsAuthenticated}, DataService exists: {_dataService != null}");
        }

        Debug.WriteLine($"=== LocationService.DeleteLocationAsync END ===");
    }
}
