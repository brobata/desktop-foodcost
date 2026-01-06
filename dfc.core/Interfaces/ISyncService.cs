using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Interface for sync service
/// </summary>
public interface ISyncService
{
    bool IsSyncing { get; }
    DateTime? LastSyncTime { get; }
    event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;
    event EventHandler<SyncCompletedEventArgs>? SyncCompleted;
    Task<SyncResult> SyncAsync();
    Task<SyncResult> PushOnlyAsync();
    Task<SyncResult> PullOnlyAsync();
    Task<SyncResult> SyncRestaurantsAsync();
    Task<SyncResult> ForceUploadAllAsync();
}

/// <summary>
/// Result of a sync operation
/// </summary>
public class SyncResult
{
    public bool IsSuccess { get; set; }
    public int ItemsDownloaded { get; set; }
    public int ItemsUploaded { get; set; }
    public List<SyncConflict> Conflicts { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public TimeSpan SyncDuration { get; set; }

    public static SyncResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };

    public static SyncResult Success(string? message = null) => new()
    {
        IsSuccess = true,
        ErrorMessage = message
    };
}

/// <summary>
/// Represents a sync conflict between local and cloud data
/// </summary>
public class SyncConflict
{
    public string ItemType { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public DateTime LocalModifiedAt { get; set; }
    public DateTime CloudModifiedAt { get; set; }
}

/// <summary>
/// Event args for sync progress updates
/// </summary>
public class SyncProgressEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public int PercentComplete { get; set; }
}

/// <summary>
/// Event args for sync completion
/// </summary>
public class SyncCompletedEventArgs : EventArgs
{
    public SyncResult Result { get; set; } = new();
}
