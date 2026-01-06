using Dfc.Core.Interfaces;

namespace Dfc.Core.Services;

/// <summary>
/// Local-only sync service that does nothing.
/// Used when running in offline/local-only mode without cloud sync.
/// </summary>
public class LocalOnlySyncService : ISyncService
{
    public bool IsSyncing => false;
    public DateTime? LastSyncTime => null;

    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;
    public event EventHandler<SyncCompletedEventArgs>? SyncCompleted;

    public Task<SyncResult> SyncAsync()
    {
        return Task.FromResult(SyncResult.Success("Local-only mode - sync disabled"));
    }

    public Task<SyncResult> PushOnlyAsync()
    {
        return Task.FromResult(SyncResult.Success("Local-only mode - sync disabled"));
    }

    public Task<SyncResult> PullOnlyAsync()
    {
        return Task.FromResult(SyncResult.Success("Local-only mode - sync disabled"));
    }

    public Task<SyncResult> SyncRestaurantsAsync()
    {
        return Task.FromResult(SyncResult.Success("Local-only mode - sync disabled"));
    }

    public Task<SyncResult> ForceUploadAllAsync()
    {
        return Task.FromResult(SyncResult.Success("Local-only mode - sync disabled"));
    }
}
