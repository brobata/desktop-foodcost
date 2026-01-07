using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dfc.Core.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<LocationService>? _logger;

    public LocationService(
        ILocationRepository repository,
        ILogger<LocationService>? logger = null)
    {
        _repository = repository;
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
        Debug.WriteLine($"[LocationService] CREATE LOCATION: {location.Name}");

        if (string.IsNullOrWhiteSpace(location.Name))
        {
            throw new ArgumentException("Location name is required");
        }

        location.Id = Guid.NewGuid();
        Debug.WriteLine($"[LocationService] Generated location ID: {location.Id}");

        var createdLocation = await _repository.AddAsync(location);
        Debug.WriteLine($"[LocationService] Saved location '{location.Name}' to local database");

        return createdLocation;
    }

    public async Task<Location> UpdateLocationAsync(Location location)
    {
        if (string.IsNullOrWhiteSpace(location.Name))
            throw new ArgumentException("Location name is required");

        var exists = await _repository.ExistsAsync(location.Id);
        if (!exists)
            throw new InvalidOperationException("Location not found");

        var updatedLocation = await _repository.UpdateAsync(location);
        Debug.WriteLine($"[LocationService] Updated location '{location.Name}' in local database");

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

        await _repository.DeleteAsync(id);
        Debug.WriteLine($"[LocationService.Delete] Deleted from local database");

        Debug.WriteLine($"=== LocationService.DeleteLocationAsync END ===");
    }
}
