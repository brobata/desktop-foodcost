using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Supabase.Storage;

namespace Freecost.Core.Services;

/// <summary>
/// Manages photo storage in Supabase Storage with local caching
/// Strategy:
/// - Photos stored in Supabase Storage (cloud) - accessible to all users
/// - Local cache for fast access - avoid re-downloading
/// - Timestamp-based sync - only download if cloud version is newer
/// </summary>
public class SupabasePhotoService
{
    private readonly ILogger<SupabasePhotoService>? _logger;
    private readonly string _localCachePath;
    private const string StorageBucketName = "photos";

    public SupabasePhotoService(ILogger<SupabasePhotoService>? logger = null)
    {
        _logger = logger;

        // Local cache path: AppData/Freecost/PhotoCache/
        _localCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost",
            "PhotoCache"
        );

        Directory.CreateDirectory(_localCachePath);
    }

    /// <summary>
    /// Upload a photo to Supabase Storage
    /// Returns the public URL and file metadata
    /// </summary>
    public async Task<PhotoUploadResult> UploadPhotoAsync(
        string localFilePath,
        string entityType,
        Guid entityId,
        string? caption = null)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return PhotoUploadResult.Failure($"File not found: {localFilePath}");
            }

            var client = await SupabaseClientProvider.GetClientAsync();
            var storage = client.Storage;

            // Generate storage path: photos/{entity_type}/{entity_id}/{filename}
            var fileName = Path.GetFileName(localFilePath);
            var storagePath = $"{entityType}/{entityId}/{fileName}";

            Debug.WriteLine($"[SupabasePhoto] Uploading photo to: {storagePath}");

            // Read file bytes
            var fileBytes = await File.ReadAllBytesAsync(localFilePath);
            var fileInfo = new FileInfo(localFilePath);

            // Upload to Supabase Storage
            await storage
                .From(StorageBucketName)
                .Upload(fileBytes, storagePath, new Supabase.Storage.FileOptions
                {
                    ContentType = GetContentType(fileName),
                    Upsert = true // Overwrite if exists
                });

            // Get public URL
            var publicUrl = storage.From(StorageBucketName).GetPublicUrl(storagePath);

            // Save to local cache
            var cachedPath = await SaveToLocalCache(fileBytes, storagePath);

            Debug.WriteLine($"[SupabasePhoto] ✓ Photo uploaded successfully: {publicUrl}");
            _logger?.LogInformation("Uploaded photo to Supabase Storage: {Path}", storagePath);

            return PhotoUploadResult.Success(new PhotoMetadata
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                FileName = fileName,
                StoragePath = storagePath,
                PublicUrl = publicUrl,
                LocalCachePath = cachedPath,
                FileSize = fileInfo.Length,
                ModifiedAt = DateTime.UtcNow,
                Caption = caption
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SupabasePhoto] ❌ Upload failed: {ex.Message}");
            _logger?.LogError(ex, "Failed to upload photo to Supabase Storage");
            return PhotoUploadResult.Failure($"Upload failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get photo from local cache or download from Supabase Storage
    /// Only downloads if:
    /// 1. Not in cache, OR
    /// 2. Cloud version has newer timestamp
    /// </summary>
    public async Task<PhotoDownloadResult> GetPhotoAsync(PhotoMetadata photoMetadata)
    {
        try
        {
            // Check local cache first
            var cachedPath = GetLocalCachePath(photoMetadata.StoragePath);

            if (File.Exists(cachedPath))
            {
                // Check if cached version is current
                var cachedFileInfo = new FileInfo(cachedPath);
                var cachedModifiedTime = cachedFileInfo.LastWriteTimeUtc;

                if (cachedModifiedTime >= photoMetadata.ModifiedAt)
                {
                    // Cache is current - use it
                    Debug.WriteLine($"[SupabasePhoto] ✓ Using cached photo: {cachedPath}");
                    return PhotoDownloadResult.Success(cachedPath, fromCache: true);
                }
                else
                {
                    Debug.WriteLine($"[SupabasePhoto] Cache outdated, re-downloading...");
                }
            }

            // Download from Supabase Storage
            Debug.WriteLine($"[SupabasePhoto] Downloading photo from: {photoMetadata.StoragePath}");
            var client = await SupabaseClientProvider.GetClientAsync();
            var storage = client.Storage;

            var photoBytes = await storage
                .From(StorageBucketName)
                .Download(photoMetadata.StoragePath, null);

            if (photoBytes == null || photoBytes.Length == 0)
            {
                return PhotoDownloadResult.Failure("Downloaded photo is empty");
            }

            // Save to local cache
            cachedPath = await SaveToLocalCache(photoBytes, photoMetadata.StoragePath);

            // Update modified time to match cloud
            File.SetLastWriteTimeUtc(cachedPath, photoMetadata.ModifiedAt);

            Debug.WriteLine($"[SupabasePhoto] ✓ Photo downloaded and cached: {cachedPath}");
            _logger?.LogInformation("Downloaded photo from Supabase Storage: {Path}", photoMetadata.StoragePath);

            return PhotoDownloadResult.Success(cachedPath, fromCache: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SupabasePhoto] ❌ Get photo failed: {ex.Message}");
            _logger?.LogError(ex, "Failed to get photo from Supabase Storage");
            return PhotoDownloadResult.Failure($"Get photo failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete photo from Supabase Storage and local cache
    /// </summary>
    public async Task<bool> DeletePhotoAsync(string storagePath)
    {
        try
        {
            Debug.WriteLine($"[SupabasePhoto] Deleting photo: {storagePath}");
            var client = await SupabaseClientProvider.GetClientAsync();
            var storage = client.Storage;

            // Delete from Supabase Storage
            await storage
                .From(StorageBucketName)
                .Remove(storagePath);

            // Delete from local cache
            var cachedPath = GetLocalCachePath(storagePath);
            if (File.Exists(cachedPath))
            {
                File.Delete(cachedPath);
                Debug.WriteLine($"[SupabasePhoto] ✓ Deleted cached photo: {cachedPath}");
            }

            Debug.WriteLine($"[SupabasePhoto] ✓ Photo deleted successfully");
            _logger?.LogInformation("Deleted photo from Supabase Storage: {Path}", storagePath);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SupabasePhoto] ❌ Delete failed: {ex.Message}");
            _logger?.LogError(ex, "Failed to delete photo from Supabase Storage");
            return false;
        }
    }

    /// <summary>
    /// Clear entire local photo cache
    /// Useful for troubleshooting or freeing disk space
    /// </summary>
    public void ClearLocalCache()
    {
        try
        {
            if (Directory.Exists(_localCachePath))
            {
                Directory.Delete(_localCachePath, recursive: true);
                Directory.CreateDirectory(_localCachePath);
                Debug.WriteLine($"[SupabasePhoto] ✓ Local cache cleared");
                _logger?.LogInformation("Cleared local photo cache");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SupabasePhoto] ⚠ Failed to clear cache: {ex.Message}");
            _logger?.LogWarning(ex, "Failed to clear local photo cache");
        }
    }

    /// <summary>
    /// Get local cache size in bytes
    /// </summary>
    public long GetCacheSize()
    {
        try
        {
            if (!Directory.Exists(_localCachePath))
                return 0;

            var dirInfo = new DirectoryInfo(_localCachePath);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories)
                .Sum(file => file.Length);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Save photo bytes to local cache
    /// </summary>
    private async Task<string> SaveToLocalCache(byte[] photoBytes, string storagePath)
    {
        var cachedPath = GetLocalCachePath(storagePath);
        var cacheDir = Path.GetDirectoryName(cachedPath);

        if (!string.IsNullOrEmpty(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        await File.WriteAllBytesAsync(cachedPath, photoBytes);
        return cachedPath;
    }

    /// <summary>
    /// Get local cache path for a storage path
    /// </summary>
    private string GetLocalCachePath(string storagePath)
    {
        // Convert storage path to local path
        // photos/Ingredient/abc-123/photo.jpg -> AppData/Freecost/PhotoCache/Ingredient/abc-123/photo.jpg
        return Path.Combine(_localCachePath, storagePath);
    }

    /// <summary>
    /// Get content type from file extension
    /// </summary>
    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}

/// <summary>
/// Photo metadata stored in database
/// </summary>
public class PhotoMetadata
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty; // Ingredient, Recipe, Entree
    public Guid EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty; // Path in Supabase Storage
    public string PublicUrl { get; set; } = string.Empty; // Public URL from Supabase
    public string? LocalCachePath { get; set; } // Local cache path
    public long FileSize { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? Caption { get; set; }
}

/// <summary>
/// Result of photo upload operation
/// </summary>
public class PhotoUploadResult
{
    public bool IsSuccess { get; set; }
    public PhotoMetadata? Photo { get; set; }
    public string? ErrorMessage { get; set; }

    public static PhotoUploadResult Success(PhotoMetadata photo) => new()
    {
        IsSuccess = true,
        Photo = photo
    };

    public static PhotoUploadResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}

/// <summary>
/// Result of photo download/get operation
/// </summary>
public class PhotoDownloadResult
{
    public bool IsSuccess { get; set; }
    public string? LocalPath { get; set; }
    public bool FromCache { get; set; }
    public string? ErrorMessage { get; set; }

    public static PhotoDownloadResult Success(string localPath, bool fromCache) => new()
    {
        IsSuccess = true,
        LocalPath = localPath,
        FromCache = fromCache
    };

    public static PhotoDownloadResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}
