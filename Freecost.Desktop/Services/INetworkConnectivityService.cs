using System;

namespace Freecost.Desktop.Services;

/// <summary>
/// Service for detecting and monitoring network connectivity status.
/// Provides real-time notifications when connectivity changes.
/// </summary>
public interface INetworkConnectivityService
{
    /// <summary>
    /// Gets whether the device currently has network connectivity.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when network connectivity status changes.
    /// </summary>
    event EventHandler<NetworkConnectivityChangedEventArgs>? ConnectivityChanged;

    /// <summary>
    /// Starts monitoring network connectivity changes.
    /// Should be called once during application startup.
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops monitoring network connectivity changes.
    /// Should be called during application shutdown.
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Manually checks current network connectivity status.
    /// Updates IsConnected property and raises ConnectivityChanged event if status changed.
    /// </summary>
    /// <returns>True if connected, false otherwise</returns>
    bool CheckConnectivity();
}

/// <summary>
/// Event args for network connectivity status changes.
/// </summary>
public class NetworkConnectivityChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the device is now connected to a network.
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Gets the timestamp when the connectivity change was detected.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
