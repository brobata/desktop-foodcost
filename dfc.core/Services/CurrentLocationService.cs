using System;
using System.IO;
using System.Text.Json;
using Dfc.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Manages the currently selected location for the user session
/// </summary>
public class CurrentLocationService : ICurrentLocationService
{
    private Location? _currentLocation;
    private readonly ILogger<CurrentLocationService>? _logger;
    private readonly string _locationFilePath;

    public event EventHandler? CurrentLocationChanged;

    public CurrentLocationService(ILogger<CurrentLocationService>? logger = null)
    {
        _logger = logger;
        
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );
        Directory.CreateDirectory(appDataPath);
        _locationFilePath = Path.Combine(appDataPath, "last_location.json");
    }

    public Guid CurrentLocationId => _currentLocation?.Id ?? Guid.Empty;

    public Location? CurrentLocation => _currentLocation;

    public bool HasLocation => _currentLocation != null;

    public void SetCurrentLocation(Location? location)
    {
        if (location == null)
        {
            _logger?.LogInformation("[LOCATION] Clearing current location");
            _currentLocation = null;
            // Clear the saved location file
            try
            {
                if (File.Exists(_locationFilePath))
                {
                    File.Delete(_locationFilePath);
                    _logger?.LogInformation("[LOCATION] Deleted last_location.json file");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[LOCATION] Failed to delete location file");
            }
            CurrentLocationChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        var previousLocation = _currentLocation;
        _currentLocation = location;

        _logger?.LogInformation("[LOCATION] Current location changed from '{PreviousLocation}' to '{NewLocation}' (ID: {LocationId})",
            previousLocation?.Name ?? "None",
            location.Name,
            location.Id);

        // Save location ID to persist selection across sessions
        SaveLastLocationId(location.Id);

        CurrentLocationChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Save the last selected location ID to a file
    /// </summary>
    private void SaveLastLocationId(Guid locationId)
    {
        try
        {
            var data = new { LocationId = locationId };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_locationFilePath, json);
            _logger?.LogInformation("[LOCATION] Saved last location ID: {LocationId}", locationId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[LOCATION] Failed to save last location ID: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Load the last selected location ID from file
    /// </summary>
    public Guid? LoadLastLocationId()
    {
        try
        {
            if (!File.Exists(_locationFilePath))
            {
                _logger?.LogDebug("[LOCATION] No saved location file found");
                return null;
            }

            var json = File.ReadAllText(_locationFilePath);
            var data = JsonSerializer.Deserialize<LocationData>(json);
            
            if (data?.LocationId != null && data.LocationId != Guid.Empty)
            {
                _logger?.LogInformation("[LOCATION] Loaded last location ID: {LocationId}", data.LocationId);
                return data.LocationId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[LOCATION] Failed to load last location ID: {Message}", ex.Message);
            return null;
        }
    }

    private class LocationData
    {
        public Guid LocationId { get; set; }
    }
}
