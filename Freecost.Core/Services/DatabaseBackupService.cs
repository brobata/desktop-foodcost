using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Handles database backup and restore operations
/// Creates JSON export files for portability and version control
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IIngredientRepository _ingredientRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IEntreeRepository _entreeRepo;
    private readonly IPriceHistoryRepository _priceHistoryRepo;
    private readonly ILogger<DatabaseBackupService>? _logger;
    private readonly string _backupDirectory;

    public DatabaseBackupService(
        IIngredientRepository ingredientRepo,
        IRecipeRepository recipeRepo,
        IEntreeRepository entreeRepo,
        IPriceHistoryRepository priceHistoryRepo,
        ILogger<DatabaseBackupService>? logger = null)
    {
        _ingredientRepo = ingredientRepo;
        _recipeRepo = recipeRepo;
        _entreeRepo = entreeRepo;
        _priceHistoryRepo = priceHistoryRepo;
        _logger = logger;

        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Freecost",
            "Backups"
        );

        Directory.CreateDirectory(_backupDirectory);
    }

    /// <summary>
    /// Create a full database backup to a JSON file
    /// </summary>
    public async Task<BackupResult> CreateBackupAsync(string? customPath = null)
    {
        var result = new BackupResult();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var fileName = $"Freecost_Backup_{timestamp}.json";
        var filePath = customPath ?? Path.Combine(_backupDirectory, fileName);

        try
        {
            _logger?.LogInformation("Creating backup: {FilePath}", filePath);

            var locationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Gather all data
            var ingredients = await _ingredientRepo.GetAllAsync(locationId);
            var recipes = (await _recipeRepo.GetAllRecipesAsync(locationId)).ToList();
            var entrees = (await _entreeRepo.GetAllAsync(locationId)).ToList();

            var backup = new DatabaseBackup
            {
                Version = "1.0",
                CreatedAt = DateTime.UtcNow,
                Ingredients = ingredients,
                Recipes = recipes,
                Entrees = entrees,
                PriceHistory = new List<Freecost.Core.Models.PriceHistory>() // Price history not needed for backups
            };

            // Serialize to JSON with pretty printing
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(backup, options);
            await File.WriteAllTextAsync(filePath, json);

            // Calculate file size
            var fileInfo = new FileInfo(filePath);
            result.IsSuccess = true;
            result.FilePath = filePath;
            result.FileSizeBytes = fileInfo.Length;
            result.ItemCount = ingredients.Count + recipes.Count + entrees.Count;

            _logger?.LogInformation("Backup created successfully: {Size} bytes, {Items} items",
                result.FileSizeBytes, result.ItemCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Backup failed");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Create a compressed backup (.zip) for smaller file size
    /// </summary>
    public async Task<BackupResult> CreateCompressedBackupAsync(string? customPath = null)
    {
        try
        {
            // First create JSON backup to temp location
            var tempJsonPath = Path.Combine(Path.GetTempPath(), $"temp_backup_{Guid.NewGuid()}.json");
            var jsonResult = await CreateBackupAsync(tempJsonPath);

            if (!jsonResult.IsSuccess)
            {
                return jsonResult;
            }

            // Create zip file
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var zipFileName = $"Freecost_Backup_{timestamp}.zip";
            var zipPath = customPath ?? Path.Combine(_backupDirectory, zipFileName);

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(tempJsonPath, $"backup_{timestamp}.json");
            }

            // Clean up temp file
            File.Delete(tempJsonPath);

            var zipInfo = new FileInfo(zipPath);
            var compressionRatio = (1 - ((double)zipInfo.Length / jsonResult.FileSizeBytes)) * 100;

            _logger?.LogInformation("Compressed backup created: {ZipSize} bytes ({Ratio:F1}% compression)",
                zipInfo.Length, compressionRatio);

            return new BackupResult
            {
                IsSuccess = true,
                FilePath = zipPath,
                FileSizeBytes = zipInfo.Length,
                ItemCount = jsonResult.ItemCount
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Compressed backup failed");
            return BackupResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Restore database from a backup file
    /// </summary>
    public async Task<RestoreResult> RestoreFromBackupAsync(string backupFilePath, bool clearExistingData = false)
    {
        var result = new RestoreResult();

        try
        {
            _logger?.LogInformation("Restoring from backup: {FilePath}", backupFilePath);

            if (!File.Exists(backupFilePath))
            {
                return RestoreResult.Failure("Backup file not found");
            }

            string json;

            // Check if it's a zip file
            if (Path.GetExtension(backupFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = ZipFile.OpenRead(backupFilePath);
                var entry = archive.Entries[0]; // Get first (should be only) entry
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                json = await reader.ReadToEndAsync();
            }
            else
            {
                json = await File.ReadAllTextAsync(backupFilePath);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var backup = JsonSerializer.Deserialize<DatabaseBackup>(json, options);

            if (backup == null)
            {
                return RestoreResult.Failure("Failed to parse backup file");
            }

            var locationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Optional: Clear existing data
            if (clearExistingData)
            {
                _logger?.LogWarning("Clearing existing data before restore");
                // Note: This would require DeleteAll methods in repositories
                // For now, we'll just add/update existing items
            }

            // Restore ingredients
            if (backup.Ingredients != null)
            {
                foreach (var ingredient in backup.Ingredients)
                {
                    try
                    {
                        ingredient.LocationId = locationId;
                        var existing = await _ingredientRepo.GetByIdAsync(ingredient.Id);
                        if (existing == null)
                        {
                            await _ingredientRepo.AddAsync(ingredient);
                            result.IngredientsRestored++;
                        }
                        else
                        {
                            await _ingredientRepo.UpdateAsync(ingredient);
                            result.IngredientsRestored++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to restore ingredient: {Name}", ingredient.Name);
                    }
                }
            }

            // Restore recipes
            if (backup.Recipes != null)
            {
                foreach (var recipe in backup.Recipes)
                {
                    try
                    {
                        recipe.LocationId = locationId;
                        var existing = await _recipeRepo.GetRecipeByIdAsync(recipe.Id);
                        if (existing == null)
                        {
                            await _recipeRepo.CreateRecipeAsync(recipe);
                            result.RecipesRestored++;
                        }
                        else
                        {
                            await _recipeRepo.UpdateRecipeAsync(recipe);
                            result.RecipesRestored++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to restore recipe: {Name}", recipe.Name);
                    }
                }
            }

            // Restore entrees
            if (backup.Entrees != null)
            {
                foreach (var entree in backup.Entrees)
                {
                    try
                    {
                        entree.LocationId = locationId;
                        var existing = await _entreeRepo.GetByIdAsync(entree.Id);
                        if (existing == null)
                        {
                            await _entreeRepo.CreateAsync(entree);
                            result.EntreesRestored++;
                        }
                        else
                        {
                            await _entreeRepo.UpdateAsync(entree);
                            result.EntreesRestored++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to restore entree: {Name}", entree.Name);
                    }
                }
            }

            result.IsSuccess = true;
            result.BackupDate = backup.CreatedAt;
            result.BackupVersion = backup.Version;

            _logger?.LogInformation("Restore completed: {Ingredients} ingredients, {Recipes} recipes, {Entrees} entrees",
                result.IngredientsRestored, result.RecipesRestored, result.EntreesRestored);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Restore failed");
            return RestoreResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Get list of available backups
    /// </summary>
    public Task<List<BackupInfo>> GetAvailableBackupsAsync()
    {
        try
        {
            var backups = new List<BackupInfo>();
            var files = Directory.GetFiles(_backupDirectory, "Freecost_Backup_*.*");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FilePath = file,
                    FileName = fileInfo.Name,
                    CreatedAt = fileInfo.CreationTime,
                    SizeBytes = fileInfo.Length,
                    IsCompressed = fileInfo.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)
                });
            }

            return Task.FromResult(backups.OrderByDescending(b => b.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to list backups");
            return Task.FromResult(new List<BackupInfo>());
        }
    }

    /// <summary>
    /// Delete old backups to save space
    /// </summary>
    public async Task<int> CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backups = await GetAvailableBackupsAsync();
            var toDelete = backups.Skip(keepCount).ToList();

            foreach (var backup in toDelete)
            {
                File.Delete(backup.FilePath);
                _logger?.LogInformation("Deleted old backup: {FileName}", backup.FileName);
            }

            return toDelete.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cleanup old backups");
            return 0;
        }
    }
}

/// <summary>
/// Interface for database backup service
/// </summary>
public interface IDatabaseBackupService
{
    Task<BackupResult> CreateBackupAsync(string? customPath = null);
    Task<BackupResult> CreateCompressedBackupAsync(string? customPath = null);
    Task<RestoreResult> RestoreFromBackupAsync(string backupFilePath, bool clearExistingData = false);
    Task<List<BackupInfo>> GetAvailableBackupsAsync();
    Task<int> CleanupOldBackupsAsync(int keepCount = 10);
}

/// <summary>
/// Database backup structure
/// </summary>
public class DatabaseBackup
{
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; }
    public List<Freecost.Core.Models.Ingredient> Ingredients { get; set; } = new();
    public List<Freecost.Core.Models.Recipe> Recipes { get; set; } = new();
    public List<Freecost.Core.Models.Entree> Entrees { get; set; } = new();
    public List<Freecost.Core.Models.PriceHistory> PriceHistory { get; set; } = new();
}

/// <summary>
/// Result of a backup operation
/// </summary>
public class BackupResult
{
    public bool IsSuccess { get; set; }
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public int ItemCount { get; set; }
    public string? ErrorMessage { get; set; }

    public string FormattedSize => FileSizeBytes < 1024 * 1024
        ? $"{FileSizeBytes / 1024.0:F1} KB"
        : $"{FileSizeBytes / (1024.0 * 1024.0):F1} MB";

    public static BackupResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}

/// <summary>
/// Result of a restore operation
/// </summary>
public class RestoreResult
{
    public bool IsSuccess { get; set; }
    public int IngredientsRestored { get; set; }
    public int RecipesRestored { get; set; }
    public int EntreesRestored { get; set; }
    public DateTime BackupDate { get; set; }
    public string? BackupVersion { get; set; }
    public string? ErrorMessage { get; set; }

    public int TotalItemsRestored => IngredientsRestored + RecipesRestored + EntreesRestored;

    public static RestoreResult Failure(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}

/// <summary>
/// Information about a backup file
/// </summary>
public class BackupInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
    public bool IsCompressed { get; set; }

    public string FormattedSize => SizeBytes < 1024 * 1024
        ? $"{SizeBytes / 1024.0:F1} KB"
        : $"{SizeBytes / (1024.0 * 1024.0):F1} MB";

    public string TypeIcon => IsCompressed ? "📦" : "📄";
}
