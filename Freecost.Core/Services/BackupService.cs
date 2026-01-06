using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class BackupService : IBackupService
{
    private readonly string _databasePath;
    private readonly string _photosBasePath;
    private readonly string _backupDirectory;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;

    public BackupService(
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IEntreeRepository entreeRepository)
    {
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;

        // Get database path
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost"
        );

        _databasePath = Path.Combine(appDataPath, "freecost.db");
        _photosBasePath = Path.Combine(appDataPath, "Photos");
        _backupDirectory = Path.Combine(appDataPath, "Backups");

        // Ensure backup directory exists
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<string> CreateBackupAsync(
        string destinationPath,
        bool includePhotos = true,
        string? notes = null,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report(new BackupProgress
        {
            Stage = BackupStage.Initializing,
            PercentComplete = 0,
            CurrentOperation = "Preparing backup..."
        });

        // Create temp directory for backup contents
        var tempDir = Path.Combine(Path.GetTempPath(), $"freecost_backup_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Collect metadata
            var metadata = await CollectMetadataAsync(includePhotos, notes);

            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.ExportingDatabase,
                PercentComplete = 10,
                CurrentOperation = "Exporting database..."
            });

            // Copy database file
            var dbDestPath = Path.Combine(tempDir, "freecost.db");
            if (File.Exists(_databasePath))
            {
                File.Copy(_databasePath, dbDestPath, true);
                metadata.DatabaseSizeBytes = new FileInfo(_databasePath).Length;
            }

            // Save metadata
            var metadataPath = Path.Combine(tempDir, "metadata.json");
            var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken);

            long totalBytes = metadata.DatabaseSizeBytes;
            long processedBytes = metadata.DatabaseSizeBytes;

            // Copy photos if requested
            if (includePhotos && Directory.Exists(_photosBasePath))
            {
                progress?.Report(new BackupProgress
                {
                    Stage = BackupStage.CopyingPhotos,
                    PercentComplete = 30,
                    CurrentOperation = "Copying photos..."
                });

                var photosDestPath = Path.Combine(tempDir, "Photos");
                await CopyDirectoryAsync(_photosBasePath, photosDestPath, progress, cancellationToken);

                // Calculate total size
                var photoFiles = Directory.GetFiles(photosDestPath, "*.*", SearchOption.AllDirectories);
                var photosSize = photoFiles.Sum(f => new FileInfo(f).Length);
                totalBytes += photosSize;
                processedBytes += photosSize;
            }

            metadata.TotalBackupSizeBytes = totalBytes;

            // Update metadata with final size
            metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken);

            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.Compressing,
                PercentComplete = 70,
                CurrentOperation = "Compressing backup...",
                BytesProcessed = processedBytes,
                TotalBytes = totalBytes
            });

            // Create ZIP archive
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            ZipFile.CreateFromDirectory(tempDir, destinationPath, CompressionLevel.Optimal, false);

            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.Finalizing,
                PercentComplete = 95,
                CurrentOperation = "Finalizing backup..."
            });

            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.Completed,
                PercentComplete = 100,
                CurrentOperation = "Backup completed successfully!",
                BytesProcessed = totalBytes,
                TotalBytes = totalBytes
            });

            return destinationPath;
        }
        finally
        {
            // Cleanup temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    public async Task RestoreBackupAsync(
        string backupFilePath,
        bool mergeData = false,
        IProgress<BackupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("Backup file not found", backupFilePath);
        }

        progress?.Report(new BackupProgress
        {
            Stage = BackupStage.Validating,
            PercentComplete = 0,
            CurrentOperation = "Validating backup..."
        });

        // Validate backup first
        var (isValid, error, metadata) = await ValidateBackupAsync(backupFilePath);
        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid backup file: {error}");
        }

        // Create temp directory for extraction
        var tempDir = Path.Combine(Path.GetTempPath(), $"freecost_restore_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.ExtractingBackup,
                PercentComplete = 10,
                CurrentOperation = "Extracting backup..."
            });

            // Extract backup
            ZipFile.ExtractToDirectory(backupFilePath, tempDir);

            // If not merging, backup current database
            if (!mergeData)
            {
                progress?.Report(new BackupProgress
                {
                    Stage = BackupStage.ImportingDatabase,
                    PercentComplete = 30,
                    CurrentOperation = "Replacing database..."
                });

                // Create backup of current database
                if (File.Exists(_databasePath))
                {
                    var currentDbBackup = Path.Combine(_backupDirectory, $"pre_restore_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                    File.Copy(_databasePath, currentDbBackup, true);
                }

                // Replace database
                var extractedDb = Path.Combine(tempDir, "freecost.db");
                if (File.Exists(extractedDb))
                {
                    File.Copy(extractedDb, _databasePath, true);
                }
            }
            else
            {
                // TODO: Implement merge logic (more complex - would need to handle ID conflicts)
                throw new NotImplementedException("Merge functionality is not yet implemented. Please use replace mode.");
            }

            // Restore photos if included
            var photosSourcePath = Path.Combine(tempDir, "Photos");
            if (Directory.Exists(photosSourcePath))
            {
                progress?.Report(new BackupProgress
                {
                    Stage = BackupStage.RestoringPhotos,
                    PercentComplete = 60,
                    CurrentOperation = "Restoring photos..."
                });

                // Backup current photos
                if (Directory.Exists(_photosBasePath))
                {
                    var photosBackup = Path.Combine(_backupDirectory, $"pre_restore_photos_{DateTime.Now:yyyyMMdd_HHmmss}");
                    Directory.CreateDirectory(photosBackup);
                    await CopyDirectoryAsync(_photosBasePath, photosBackup, null, cancellationToken);
                }

                // Restore photos
                await CopyDirectoryAsync(photosSourcePath, _photosBasePath, progress, cancellationToken);
            }

            progress?.Report(new BackupProgress
            {
                Stage = BackupStage.Completed,
                PercentComplete = 100,
                CurrentOperation = "Restore completed successfully!"
            });
        }
        finally
        {
            // Cleanup temp directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    public async Task<(bool IsValid, string? Error, BackupMetadata? Metadata)> ValidateBackupAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
        {
            return (false, "Backup file not found", null);
        }

        if (!backupFilePath.EndsWith(".freecost", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Invalid backup file extension. Expected .freecost", null);
        }

        try
        {
            using var archive = ZipFile.OpenRead(backupFilePath);

            // Check for metadata
            var metadataEntry = archive.GetEntry("metadata.json");
            if (metadataEntry == null)
            {
                return (false, "Backup file is missing metadata.json", null);
            }

            // Read metadata
            using var metadataStream = metadataEntry.Open();
            using var reader = new StreamReader(metadataStream);
            var metadataJson = await reader.ReadToEndAsync();
            var metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

            if (metadata == null)
            {
                return (false, "Failed to parse backup metadata", null);
            }

            // Check for database file
            var dbEntry = archive.GetEntry("freecost.db");
            if (dbEntry == null)
            {
                return (false, "Backup file is missing database file", null);
            }

            return (true, null, metadata);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to validate backup: {ex.Message}", null);
        }
    }

    public async Task<List<BackupFileInfo>> GetAvailableBackupsAsync()
    {
        var backups = new List<BackupFileInfo>();

        if (!Directory.Exists(_backupDirectory))
        {
            return backups;
        }

        var backupFiles = Directory.GetFiles(_backupDirectory, "*.freecost");

        foreach (var backupFile in backupFiles)
        {
            var fileInfo = new FileInfo(backupFile);
            var (isValid, error, metadata) = await ValidateBackupAsync(backupFile);

            backups.Add(new BackupFileInfo
            {
                FilePath = backupFile,
                FileName = Path.GetFileName(backupFile),
                Metadata = metadata ?? new BackupMetadata(),
                FileCreatedAt = fileInfo.CreationTime,
                FileSizeBytes = fileInfo.Length,
                IsValid = isValid,
                ValidationError = error
            });
        }

        return backups.OrderByDescending(b => b.FileCreatedAt).ToList();
    }

    public async Task DeleteBackupAsync(string backupFilePath)
    {
        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }

        await Task.CompletedTask;
    }

    public async Task<BackupMetadata?> GetBackupMetadataAsync(string backupFilePath)
    {
        var (isValid, error, metadata) = await ValidateBackupAsync(backupFilePath);
        return metadata;
    }

    public async Task<string?> CreateAutomaticBackupAsync(int maxBackupsToKeep = 5)
    {
        var backupFileName = $"auto_backup_{DateTime.Now:yyyyMMdd_HHmmss}.freecost";
        var backupPath = Path.Combine(_backupDirectory, backupFileName);

        var createdBackup = await CreateBackupAsync(backupPath, includePhotos: true, notes: "Automatic backup");

        // Clean up old automatic backups
        var allBackups = await GetAvailableBackupsAsync();
        var autoBackups = allBackups
            .Where(b => b.FileName.StartsWith("auto_backup_"))
            .OrderByDescending(b => b.FileCreatedAt)
            .Skip(maxBackupsToKeep)
            .ToList();

        foreach (var oldBackup in autoBackups)
        {
            await DeleteBackupAsync(oldBackup.FilePath);
        }

        return createdBackup;
    }

    public string GetDefaultBackupDirectory()
    {
        return _backupDirectory;
    }

    // Helper methods

    private async Task<BackupMetadata> CollectMetadataAsync(bool includePhotos, string? notes)
    {
        // Get counts from repositories
        // Use a default location ID for getting counts
        var defaultLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var ingredients = await _ingredientRepository.GetAllAsync(defaultLocationId);
        var recipes = await _recipeRepository.GetAllRecipesAsync(defaultLocationId);
        var entrees = await _entreeRepository.GetAllAsync(defaultLocationId);

        var photoCount = 0;
        if (includePhotos && Directory.Exists(_photosBasePath))
        {
            photoCount = Directory.GetFiles(_photosBasePath, "*.*", SearchOption.AllDirectories).Length;
        }

        return new BackupMetadata
        {
            CreatedAt = DateTime.UtcNow,
            IngredientCount = ingredients.Count(),
            RecipeCount = recipes.Count(),
            EntreeCount = entrees.Count(),
            LocationCount = 1, // TODO: Get from location repository when implemented
            UserCount = 0, // TODO: Get from user repository
            PhotoCount = photoCount,
            IncludesPhotos = includePhotos,
            Notes = notes
        };
    }

    private async Task CopyDirectoryAsync(
        string sourcePath,
        string destPath,
        IProgress<BackupProgress>? progress,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destPath);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourcePath, file);
            var destFile = Path.Combine(destPath, relativePath);

            var destDir = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(file, destFile, true);
        }

        await Task.CompletedTask;
    }
}
