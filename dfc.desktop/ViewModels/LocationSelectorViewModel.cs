using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dfc.Desktop.ViewModels;

public partial class LocationSelectorViewModel : ObservableObject
{
    private readonly ILocationService _locationService;
    private readonly IUserSessionService _sessionService;
    private readonly ISyncService? _syncService;
    private readonly ILogger<LocationSelectorViewModel>? _logger;
    private readonly Action<Location?> _onLocationSelected;

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"  [LOCATION SELECTOR] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("  ║ [LOCATION SELECTOR ERROR]                         ║");
        System.Diagnostics.Debug.WriteLine("  ╠═══════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"  {message}");
        if (ex != null)
        {
            System.Diagnostics.Debug.WriteLine($"  Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"  Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        System.Diagnostics.Debug.WriteLine("  ╚═══════════════════════════════════════════════════╝");
    }

    [ObservableProperty]
    private ObservableCollection<Location> _locations = new();

    [ObservableProperty]
    private Location? _selectedLocation;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public LocationSelectorViewModel(
        ILocationService locationService,
        IUserSessionService sessionService,
        Action<Location?> onLocationSelected,
        ISyncService? syncService = null,
        ILogger<LocationSelectorViewModel>? logger = null)
    {
        _locationService = locationService;
        _sessionService = sessionService;
        _syncService = syncService;
        _onLocationSelected = onLocationSelected;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting InitializeAsync                          ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");

            // Get user's accessible locations
            var user = _sessionService.CurrentUser;
            if (user == null)
            {
                LogDebug("ERROR: User not authenticated");
                ErrorMessage = "User not authenticated";
                return;
            }
            LogDebug($"Current user: {user.Email}");

            // Sync restaurants before loading locations (if sync service is available)
            // This ensures we only show locations the user currently has access to
            if (_sessionService.IsAuthenticated && _syncService != null)
            {
                LogDebug("Syncing restaurants...");
                var syncResult = await _syncService.SyncRestaurantsAsync();

                if (syncResult.IsSuccess)
                {
                    LogDebug("✓ Restaurant sync completed - locations are up-to-date");
                }
                else
                {
                    LogDebug($"⚠ Restaurant sync failed: {syncResult.ErrorMessage}");
                    _logger?.LogWarning("Failed to sync restaurants before loading locations: {Error}", syncResult.ErrorMessage);
                    // Continue anyway - user can still select from cached locations
                }
            }

            // Get all locations (both offline and online)
            LogDebug("Loading all locations...");
            var allLocations = await _locationService.GetAllLocationsAsync();
            LogDebug($"Found {allLocations.Count()} total locations");

            Locations.Clear();

            // Separate offline and online locations for better organization
            var offlineLocations = allLocations
                .Where(l => l.UserId == null && l.IsActive)
                .OrderBy(l => l.Name)
                .ToList();
            LogDebug($"Found {offlineLocations.Count} offline locations");

            var onlineLocations = allLocations
                .Where(l => l.UserId != null && l.IsActive)
                .OrderBy(l => l.Name)
                .ToList();
            LogDebug($"Found {onlineLocations.Count} online locations");

            // Add offline location first (if exists) - clearly labeled
            foreach (var offline in offlineLocations)
            {
                Locations.Add(offline);
                LogDebug($"  Added offline location: {offline.Name} (ID: {offline.Id})");
            }

            // Then add online locations (skip duplicates by name)
            foreach (var online in onlineLocations)
            {
                // Skip if we already have a location with this name
                if (!Locations.Any(loc => loc.Name.Equals(online.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    Locations.Add(online);
                    LogDebug($"  Added online location: {online.Name} (ID: {online.Id})");
                }
                else
                {
                    LogDebug($"  Skipped duplicate online location: {online.Name}");
                }
            }

            LogDebug($"Total locations available: {Locations.Count}");

            // Auto-select if only one location and close immediately
            if (Locations.Count == 1)
            {
                SelectedLocation = Locations[0];
                LogDebug($"Auto-selected single location: {SelectedLocation.Name}");
                _logger?.LogInformation("Auto-selected single location: {LocationName}", SelectedLocation.Name);
                _onLocationSelected(SelectedLocation);
                LogDebug("╚═══════════════════════════════════════════════════╝");
                return;
            }
            // Select first location as default
            else if (Locations.Count > 0)
            {
                SelectedLocation = Locations[0];
                LogDebug($"Selected first location as default: {SelectedLocation.Name}");
            }
            else
            {
                LogDebug("⚠ No locations available for selection");
            }

            LogDebug($"✓ InitializeAsync complete - {Locations.Count} locations loaded");
            LogDebug("╚═══════════════════════════════════════════════════╝");
            _logger?.LogInformation("Loaded {Count} locations for user {Email}", Locations.Count, user.Email);
        }
        catch (Exception ex)
        {
            LogError("Failed to initialize location selector", ex);
            ErrorMessage = $"Failed to load locations: {ex.Message}";
            _logger?.LogError(ex, "Error loading locations");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when user double-clicks or presses Enter on a location
    /// </summary>
    public void OnLocationActivated()
    {
        if (SelectedLocation != null)
        {
            _logger?.LogInformation("User activated location: {LocationName}", SelectedLocation.Name);
            _onLocationSelected(SelectedLocation);
        }
    }

    [RelayCommand]
    private void SelectLocation()
    {
        if (SelectedLocation == null)
        {
            ErrorMessage = "Please select a location";
            return;
        }

        _logger?.LogInformation("User selected location: {LocationName}", SelectedLocation.Name);
        _onLocationSelected(SelectedLocation);
    }

    [RelayCommand]
    private void Cancel()
    {
        _logger?.LogInformation("Location selection cancelled");
        _onLocationSelected(null);
    }
}
