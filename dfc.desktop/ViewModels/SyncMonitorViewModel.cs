using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Helpers;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

/// <summary>
/// ViewModel for Delta Sync Monitoring Dashboard
/// Provides real-time insights into sync performance and health
/// </summary>
public partial class SyncMonitorViewModel : ViewModelBase
{
    private readonly ILocalModificationService _modificationService;
    private readonly ISyncService _syncService;
    private readonly ICurrentLocationService _locationService;
    private readonly ILogger<SyncMonitorViewModel>? _logger;
    private readonly Action? _onClose;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    // Statistics
    [ObservableProperty]
    private int _totalModifications;

    [ObservableProperty]
    private int _unsyncedCount;

    [ObservableProperty]
    private int _syncedCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private double _successRate;

    [ObservableProperty]
    private int _apiCallsSaved;

    [ObservableProperty]
    private string _apiCallsSavedDisplay = "0";

    // Collections
    [ObservableProperty]
    private ObservableCollection<ModificationSummary> _modificationsByType = new();

    [ObservableProperty]
    private ObservableCollection<FailedModificationItem> _failedModifications = new();

    [ObservableProperty]
    private ObservableCollection<SyncHistoryItem> _recentSyncHistory = new();

    // Selections
    [ObservableProperty]
    private FailedModificationItem? _selectedFailedItem;

    // Cleanup options
    [ObservableProperty]
    private int _cleanupDays = 30;

    [ObservableProperty]
    private int _oldSyncedRecordsCount;

    public SyncMonitorViewModel(
        ILocalModificationService modificationService,
        ISyncService syncService,
        ICurrentLocationService locationService,
        Action? onClose = null,
        ILogger<SyncMonitorViewModel>? logger = null)
    {
        _modificationService = modificationService;
        _syncService = syncService;
        _locationService = locationService;
        _onClose = onClose;
        _logger = logger;

        _ = LoadStatistics();
    }

    [RelayCommand]
    private async Task LoadStatistics()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            if (!_locationService.HasLocation)
            {
                ErrorMessage = "No location selected";
                return;
            }

            var locationId = _locationService.CurrentLocationId;

            // Get all modifications for current location
            var allMods = await _modificationService.GetUnsyncedModificationsAsync(locationId);
            var syncedMods = await _modificationService.GetSyncedModificationsAsync(locationId, 100);

            TotalModifications = allMods.Count + syncedMods.Count;
            UnsyncedCount = allMods.Count(m => !m.IsSynced);
            SyncedCount = syncedMods.Count;
            FailedCount = allMods.Count(m => m.SyncAttempts > 0 && !m.IsSynced);

            // Calculate success rate
            if (TotalModifications > 0)
            {
                SuccessRate = Math.Round((double)SyncedCount / TotalModifications * 100, 1);
            }
            else
            {
                SuccessRate = 100.0;
            }

            // Calculate API calls saved (estimate: avg 300 entities per collection, 3 collections)
            // Without delta sync: ~900 API calls per sync
            // With delta sync: only unsynced modifications
            var fullSyncCalls = 900;
            var deltaSyncCalls = UnsyncedCount;
            ApiCallsSaved = Math.Max(0, fullSyncCalls - deltaSyncCalls);
            ApiCallsSavedDisplay = ApiCallsSaved >= 1000
                ? $"{(ApiCallsSaved / 1000.0):F1}k"
                : ApiCallsSaved.ToString();

            // Group by entity type
            ModificationsByType.Clear();
            var grouped = allMods.Concat(syncedMods)
                .GroupBy(m => m.EntityType)
                .Select(g => new ModificationSummary
                {
                    EntityType = g.Key,
                    TotalCount = g.Count(),
                    UnsyncedCount = g.Count(m => !m.IsSynced),
                    SyncedCount = g.Count(m => m.IsSynced),
                    FailedCount = g.Count(m => m.SyncAttempts > 0 && !m.IsSynced)
                });

            foreach (var summary in grouped.OrderByDescending(s => s.UnsyncedCount))
            {
                ModificationsByType.Add(summary);
            }

            // Load failed modifications
            FailedModifications.Clear();
            var failed = allMods.Where(m => m.SyncAttempts > 0 && !m.IsSynced)
                .OrderByDescending(m => m.ModifiedAt)
                .Take(20);

            foreach (var mod in failed)
            {
                FailedModifications.Add(new FailedModificationItem
                {
                    Modification = mod,
                    EntityTypeDisplay = mod.EntityType,
                    OperationDisplay = mod.ModificationType.ToString(),
                    AttemptsDisplay = $"{mod.SyncAttempts} attempt(s)",
                    LastAttemptDisplay = mod.ModifiedAt.ToString("MM/dd HH:mm"),
                    ErrorDisplay = string.IsNullOrEmpty(mod.LastSyncError)
                        ? "Unknown error"
                        : mod.LastSyncError
                });
            }

            // Load recent sync history (last 7 days)
            RecentSyncHistory.Clear();
            var history = syncedMods
                .Where(m => m.SyncedAt.HasValue && m.SyncedAt.Value != default)
                .GroupBy(m => m.SyncedAt!.Value.Date)
                .Select(g => new SyncHistoryItem
                {
                    Date = g.Key,
                    DateDisplay = g.Key.ToString("MM/dd/yyyy"),
                    ItemCount = g.Count(),
                    ItemsDisplay = $"{g.Count()} items"
                })
                .OrderByDescending(h => h.Date)
                .Take(7);

            foreach (var item in history)
            {
                RecentSyncHistory.Add(item);
            }

            // Count old synced records for cleanup
            var cutoffDate = DateTime.UtcNow.AddDays(-CleanupDays);
            OldSyncedRecordsCount = syncedMods.Count(m => m.SyncedAt.HasValue && m.SyncedAt.Value < cutoffDate);

            _logger?.LogInformation("Sync statistics loaded: {Total} total, {Unsynced} unsynced, {Failed} failed",
                TotalModifications, UnsyncedCount, FailedCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load sync statistics");
            ErrorMessage = ErrorMessageHelper.GetDatabaseErrorMessage(ex, "loading sync statistics");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RetryFailed()
    {
        if (FailedCount == 0)
        {
            ErrorMessage = "No failed modifications to retry";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            _logger?.LogInformation("Manually triggering sync to retry {Count} failed modifications", FailedCount);

            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                SuccessMessage = $"Sync completed: {result.ItemsUploaded} items uploaded";
                await LoadStatistics();
            }
            else
            {
                ErrorMessage = $"Sync failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retry sync");
            ErrorMessage = ErrorMessageHelper.GetSyncErrorMessage(ex, "retrying failed sync");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RetrySelected()
    {
        if (SelectedFailedItem == null)
        {
            ErrorMessage = "Please select a failed modification to retry";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Trigger full sync (individual retry not implemented yet)
            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                SuccessMessage = "Sync completed successfully";
                await LoadStatistics();
            }
            else
            {
                ErrorMessage = $"Sync failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retry selected modification");
            ErrorMessage = ErrorMessageHelper.GetSyncErrorMessage(ex, "retrying modification");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CleanupOldRecords()
    {
        if (OldSyncedRecordsCount == 0)
        {
            ErrorMessage = "No old records to clean up";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var cutoffDate = DateTime.UtcNow.AddDays(-CleanupDays);
            await _modificationService.ClearOldSyncedModificationsAsync(cutoffDate);

            SuccessMessage = $"Cleaned up {OldSyncedRecordsCount} old records";
            _logger?.LogInformation("Manually cleaned up {Count} old synced modifications", OldSyncedRecordsCount);

            await LoadStatistics();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cleanup old records");
            ErrorMessage = ErrorMessageHelper.GetDatabaseErrorMessage(ex, "cleaning up old records");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task TriggerSync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                SuccessMessage = $"Sync completed: ↓{result.ItemsDownloaded} ↑{result.ItemsUploaded}";
                await LoadStatistics();
            }
            else
            {
                ErrorMessage = $"Sync failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to trigger sync");
            ErrorMessage = ErrorMessageHelper.GetSyncErrorMessage(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        _onClose?.Invoke();
    }
}

/// <summary>
/// Summary of modifications by entity type
/// </summary>
public class ModificationSummary
{
    public string EntityType { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int UnsyncedCount { get; set; }
    public int SyncedCount { get; set; }
    public int FailedCount { get; set; }

    public string TotalDisplay => $"{TotalCount} total";
    public string UnsyncedDisplay => $"{UnsyncedCount} unsynced";
    public string SyncedDisplay => $"{SyncedCount} synced";
    public string FailedDisplay => FailedCount > 0 ? $"{FailedCount} failed" : "0 failed";
    public string StatusColor => FailedCount > 0 ? "#EF5350" : (UnsyncedCount > 0 ? "#FFA726" : "#66BB6A");
}

/// <summary>
/// Display model for failed modification
/// </summary>
public class FailedModificationItem
{
    public LocalModification Modification { get; set; } = null!;
    public string EntityTypeDisplay { get; set; } = string.Empty;
    public string OperationDisplay { get; set; } = string.Empty;
    public string AttemptsDisplay { get; set; } = string.Empty;
    public string LastAttemptDisplay { get; set; } = string.Empty;
    public string ErrorDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Display model for sync history by date
/// </summary>
public class SyncHistoryItem
{
    public DateTime Date { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string ItemsDisplay { get; set; } = string.Empty;
}
