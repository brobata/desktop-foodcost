using System;
using Freecost.Core.Models;

namespace Freecost.Core.Services;

/// <summary>
/// Manages the currently selected location for the user session
/// </summary>
public interface ICurrentLocationService
{
    /// <summary>
    /// The currently selected location ID
    /// </summary>
    Guid CurrentLocationId { get; }

    /// <summary>
    /// The currently selected location
    /// </summary>
    Location? CurrentLocation { get; }

    /// <summary>
    /// Event raised when the current location changes
    /// </summary>
    event EventHandler? CurrentLocationChanged;

    /// <summary>
    /// Set the current location
    /// </summary>
    void SetCurrentLocation(Location location);

    /// <summary>
    /// Check if a location is set
    /// </summary>
    bool HasLocation { get; }

    /// <summary>
    /// Load the last selected location ID from persisted storage
    /// </summary>
    Guid? LoadLastLocationId();
}
