using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Freecost.Data.Services;

/// <summary>
/// Provides safety checks and backups before running database migrations
/// </summary>
public interface IMigrationSafetyService
{
    /// <summary>
    /// Validates database state and creates backup before migration
    /// </summary>
    Task<MigrationSafetyResult> ValidateAndBackupAsync(FreecostDbContext context);
}

public class MigrationSafetyResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? BackupPath { get; set; }

    public static MigrationSafetyResult Success(string? backupPath = null) =>
        new() { IsValid = true, BackupPath = backupPath };

    public static MigrationSafetyResult Failure(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage };
}

public class MigrationSafetyService : IMigrationSafetyService
{
    private readonly ILogger<MigrationSafetyService>? _logger;

    public MigrationSafetyService(ILogger<MigrationSafetyService>? logger = null)
    {
        _logger = logger;
    }

    public async Task<MigrationSafetyResult> ValidateAndBackupAsync(FreecostDbContext context)
    {
        try
        {
            _logger?.LogInformation("Starting migration safety checks...");

            // Check if there are pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (!pendingMigrations.Any())
            {
                _logger?.LogInformation("No pending migrations. Skipping backup.");
                return MigrationSafetyResult.Success();
            }

            _logger?.LogInformation("Found {Count} pending migrations", pendingMigrations.Count());

            // Validate database can be accessed
            if (!await CanAccessDatabaseAsync(context))
            {
                return MigrationSafetyResult.Failure("Cannot access database. Migration aborted.");
            }

            // Create backup before migration
            var backupPath = await CreateBackupAsync(context);
            if (string.IsNullOrEmpty(backupPath))
            {
                _logger?.LogWarning("Backup creation failed, but proceeding with migration");
            }
            else
            {
                _logger?.LogInformation("Backup created at: {BackupPath}", backupPath);
            }

            return MigrationSafetyResult.Success(backupPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Migration safety check failed");
            return MigrationSafetyResult.Failure($"Safety check failed: {ex.Message}");
        }
    }

    private async Task<bool> CanAccessDatabaseAsync(FreecostDbContext context)
    {
        try
        {
            await context.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Database connection check failed");
            return false;
        }
    }

    private async Task<string?> CreateBackupAsync(FreecostDbContext context)
    {
        try
        {
            var dbPath = context.Database.GetConnectionString()?.Replace("Data Source=", "");
            if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
            {
                _logger?.LogWarning("Database file not found at: {DbPath}", dbPath);
                return null;
            }

            // Create backup directory
            var backupDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Freecost",
                "Backups",
                "PreMigration"
            );
            Directory.CreateDirectory(backupDir);

            // Create backup filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"freecost_premigration_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            // Copy database file
            File.Copy(dbPath, backupPath, overwrite: true);

            _logger?.LogInformation("Pre-migration backup created: {BackupPath}", backupPath);

            // Clean up old backups (keep last 10)
            await CleanupOldBackupsAsync(backupDir);

            return backupPath;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create pre-migration backup");
            return null;
        }
    }

    private async Task CleanupOldBackupsAsync(string backupDir)
    {
        try
        {
            var backupFiles = Directory.GetFiles(backupDir, "freecost_premigration_*.db")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            if (backupFiles.Count > 10)
            {
                var filesToDelete = backupFiles.Skip(10);
                foreach (var file in filesToDelete)
                {
                    file.Delete();
                    _logger?.LogInformation("Deleted old backup: {FileName}", file.Name);
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to cleanup old backups");
        }
    }
}
