using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Dfc.Desktop.Services;

/// <summary>
/// Implementation of network connectivity monitoring service.
/// Uses .NET's NetworkInterface to detect connectivity changes.
/// </summary>
public class NetworkConnectivityService : INetworkConnectivityService
{
    private readonly ILogger<NetworkConnectivityService>? _logger;
    private bool _isMonitoring;
    private bool _isConnected;

    /// <inheritdoc/>
    public bool IsConnected => _isConnected;

    /// <inheritdoc/>
    public event EventHandler<NetworkConnectivityChangedEventArgs>? ConnectivityChanged;

    /// <summary>
    /// Initializes a new instance of the NetworkConnectivityService.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics</param>
    public NetworkConnectivityService(ILogger<NetworkConnectivityService>? logger = null)
    {
        _logger = logger;
        _isConnected = CheckConnectivityInternal();
    }

    /// <inheritdoc/>
    public void StartMonitoring()
    {
        if (_isMonitoring)
        {
            _logger?.LogWarning("Network monitoring already started");
            return;
        }

        try
        {
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            _isMonitoring = true;
            _logger?.LogInformation("Network connectivity monitoring started. Initial state: {IsConnected}",
                _isConnected ? "Connected" : "Disconnected");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting network connectivity monitoring");
        }
    }

    /// <inheritdoc/>
    public void StopMonitoring()
    {
        if (!_isMonitoring)
        {
            return;
        }

        try
        {
            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
            _isMonitoring = false;
            _logger?.LogInformation("Network connectivity monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping network connectivity monitoring");
        }
    }

    /// <inheritdoc/>
    public bool CheckConnectivity()
    {
        bool currentStatus = CheckConnectivityInternal();

        if (currentStatus != _isConnected)
        {
            _isConnected = currentStatus;
            RaiseConnectivityChanged(currentStatus);
        }

        return currentStatus;
    }

    private void OnNetworkAddressChanged(object? sender, EventArgs e)
    {
        _logger?.LogDebug("Network address changed - checking connectivity");
        CheckConnectivity();
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        _logger?.LogDebug("Network availability changed to: {Available}", e.IsAvailable);
        CheckConnectivity();
    }

    private bool CheckConnectivityInternal()
    {
        try
        {
            // Check if any network interface is up and operational
            bool hasConnection = NetworkInterface.GetIsNetworkAvailable();

            if (!hasConnection)
            {
                return false;
            }

            // More thorough check: look for active network interfaces with valid IP
            var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .ToList();

            bool hasActiveInterface = activeInterfaces.Any(ni =>
            {
                var ipProperties = ni.GetIPProperties();
                // Check for valid unicast addresses (excluding link-local)
                return ipProperties.UnicastAddresses
                    .Any(addr => !addr.Address.ToString().StartsWith("169.254")); // Not link-local
            });

            return hasActiveInterface;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking network connectivity");
            // Default to connected to avoid blocking operations
            return true;
        }
    }

    private void RaiseConnectivityChanged(bool isConnected)
    {
        var args = new NetworkConnectivityChangedEventArgs
        {
            IsConnected = isConnected
        };

        _logger?.LogInformation("Network connectivity changed: {Status}",
            isConnected ? "Connected" : "Disconnected");

        ConnectivityChanged?.Invoke(this, args);
    }
}
