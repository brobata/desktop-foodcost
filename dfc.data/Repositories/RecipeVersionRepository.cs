using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

/// <summary>
/// Repository for managing recipe version history
/// </summary>
public class RecipeVersionRepository : IRecipeVersionRepository
{
    private readonly DfcDbContext _context;

    public RecipeVersionRepository(DfcDbContext context)
    {
        _context = context;
    }

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"  [RECIPE VERSION REPO] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("  ║ [RECIPE VERSION REPO ERROR]                       ║");
        System.Diagnostics.Debug.WriteLine("  ╠═══════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"  {message}");
        if (ex != null)
        {
            System.Diagnostics.Debug.WriteLine($"  Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"  Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        System.Diagnostics.Debug.WriteLine("  ╚═══════════════════════════════════════════════════╝");
    }

    public async Task<RecipeVersion> CreateVersionAsync(RecipeVersion version)
    {
        try
        {
            LogDebug($"Creating version {version.VersionNumber} for recipe {version.RecipeId}");
            LogDebug($"  Recipe Name: {version.Name}");
            LogDebug($"  Changed By: {version.CreatedBy}");
            LogDebug($"  Change Description: {version.ChangeDescription}");

            // Clear navigation property to avoid EF issues
            version.Recipe = null;

            _context.RecipeVersions.Add(version);
            await _context.SaveChangesAsync();

            LogDebug($"✓ Version created successfully (ID: {version.Id})");
            return version;
        }
        catch (Exception ex)
        {
            LogError($"Failed to create version for recipe {version.RecipeId}", ex);
            throw;
        }
    }

    public async Task<List<RecipeVersion>> GetVersionsByRecipeIdAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Fetching all versions for recipe {recipeId}");

            var versions = await _context.RecipeVersions
                .Where(v => v.RecipeId == recipeId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            LogDebug($"✓ Found {versions.Count} versions");
            return versions;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch versions for recipe {recipeId}", ex);
            throw;
        }
    }

    public async Task<RecipeVersion?> GetVersionByIdAsync(Guid versionId)
    {
        try
        {
            LogDebug($"Fetching version by ID: {versionId}");

            var version = await _context.RecipeVersions
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (version != null)
            {
                LogDebug($"✓ Version found: v{version.VersionNumber} of '{version.Name}'");
            }
            else
            {
                LogDebug($"✗ Version not found");
            }

            return version;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch version {versionId}", ex);
            throw;
        }
    }

    public async Task<int> GetLatestVersionNumberAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Fetching latest version number for recipe {recipeId}");

            var latestVersion = await _context.RecipeVersions
                .Where(v => v.RecipeId == recipeId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            LogDebug($"✓ Latest version: {latestVersion}");
            return latestVersion;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch latest version number for recipe {recipeId}", ex);
            throw;
        }
    }

    public async Task<List<RecipeVersion>> GetRecentVersionsAsync(int count = 20)
    {
        try
        {
            LogDebug($"Fetching {count} most recent versions across all recipes");

            var versions = await _context.RecipeVersions
                .OrderByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();

            LogDebug($"✓ Found {versions.Count} recent versions");
            return versions;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch recent versions", ex);
            throw;
        }
    }

    public async Task DeleteVersionsByRecipeIdAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Deleting all versions for recipe {recipeId}");

            var versions = await _context.RecipeVersions
                .Where(v => v.RecipeId == recipeId)
                .ToListAsync();

            LogDebug($"Found {versions.Count} versions to delete");

            _context.RecipeVersions.RemoveRange(versions);
            await _context.SaveChangesAsync();

            LogDebug($"✓ Deleted {versions.Count} versions");
        }
        catch (Exception ex)
        {
            LogError($"Failed to delete versions for recipe {recipeId}", ex);
            throw;
        }
    }

    public async Task<int> GetVersionCountAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Counting versions for recipe {recipeId}");

            var count = await _context.RecipeVersions
                .CountAsync(v => v.RecipeId == recipeId);

            LogDebug($"✓ Version count: {count}");
            return count;
        }
        catch (Exception ex)
        {
            LogError($"Failed to count versions for recipe {recipeId}", ex);
            throw;
        }
    }
}
