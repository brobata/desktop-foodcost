using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IBackupService
{
    /// <summary>
    /// Create a backup of the database and optionally photos
    /// </summary>
    Task<string> CreateBackupAsync(
        string destinationPath,
        bool includePhotos = true,
        string? notes = null,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restore from a backup file
    /// </summary>
    Task RestoreBackupAsync(
        string backupFilePath,
        bool mergeData = false,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a backup file without restoring
    /// </summary>
    Task<(bool IsValid, string? Error, BackupMetadata? Metadata)> ValidateBackupAsync(string backupFilePath);

    /// <summary>
    /// Get all backups in the default backup directory
    /// </summary>
    Task<List<BackupFileInfo>> GetAvailableBackupsAsync();

    /// <summary>
    /// Delete a backup file
    /// </summary>
    Task DeleteBackupAsync(string backupFilePath);

    /// <summary>
    /// Get backup metadata from a backup file
    /// </summary>
    Task<BackupMetadata?> GetBackupMetadataAsync(string backupFilePath);

    /// <summary>
    /// Create an automatic backup (scheduled)
    /// </summary>
    Task<string?> CreateAutomaticBackupAsync(int maxBackupsToKeep = 5);

    /// <summary>
    /// Get default backup directory
    /// </summary>
    string GetDefaultBackupDirectory();
}

/// <summary>
/// Progress information for backup/restore operations
/// </summary>
public class BackupProgress
{
    public BackupStage Stage { get; set; }
    public int PercentComplete { get; set; }
    public string CurrentOperation { get; set; } = string.Empty;
    public long BytesProcessed { get; set; }
    public long TotalBytes { get; set; }
}

public enum BackupStage
{
    Initializing,
    ExportingDatabase,
    CopyingPhotos,
    Compressing,
    Finalizing,
    Validating,
    ExtractingBackup,
    ImportingDatabase,
    RestoringPhotos,
    Completed
}
