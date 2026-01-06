using System;
using System.Collections.Generic;

namespace Freecost.Core.Models;

/// <summary>
/// Metadata stored in backup files
/// </summary>
public class BackupMetadata
{
    public string BackupVersion { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ApplicationVersion { get; set; } = "0.6.0";
    public string MachineName { get; set; } = Environment.MachineName;
    public string Username { get; set; } = Environment.UserName;

    // Statistics
    public int IngredientCount { get; set; }
    public int RecipeCount { get; set; }
    public int EntreeCount { get; set; }
    public int LocationCount { get; set; }
    public int UserCount { get; set; }
    public int PhotoCount { get; set; }

    // Backup options
    public bool IncludesPhotos { get; set; }
    public bool IsCompressed { get; set; } = true;
    public bool IsEncrypted { get; set; }

    // Database info
    public long DatabaseSizeBytes { get; set; }
    public long TotalBackupSizeBytes { get; set; }

    // Additional info
    public string? Notes { get; set; }
    public Dictionary<string, string> CustomMetadata { get; set; } = new();

    public string GetReadableSize()
    {
        return FormatBytes(TotalBackupSizeBytes);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Represents a backup file on disk
/// </summary>
public class BackupFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public BackupMetadata Metadata { get; set; } = new();
    public DateTime FileCreatedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsValid { get; set; }
    public string? ValidationError { get; set; }

    public string GetReadableSize()
    {
        return Metadata.GetReadableSize();
    }
}
