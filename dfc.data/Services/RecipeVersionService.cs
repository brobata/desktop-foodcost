using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dfc.Data.Services;

/// <summary>
/// Service for managing recipe version control and history
/// </summary>
public class RecipeVersionService : IRecipeVersionService
{
    private readonly IRecipeVersionRepository _versionRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<RecipeVersionService>? _logger;

    public RecipeVersionService(
        IRecipeVersionRepository versionRepository,
        IRecipeRepository recipeRepository,
        ILogger<RecipeVersionService>? logger = null)
    {
        _versionRepository = versionRepository;
        _recipeRepository = recipeRepository;
        _logger = logger;
    }

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"  [RECIPE VERSION SERVICE] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("  ║ [RECIPE VERSION SERVICE ERROR]                    ║");
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

    public async Task<RecipeVersion> CreateVersionAsync(Recipe recipe, string? changedBy, string? changeDescription)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Creating Recipe Version                           ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Recipe: {recipe.Name} (ID: {recipe.Id})");
            LogDebug($"Changed By: {changedBy ?? "System"}");
            LogDebug($"Description: {changeDescription ?? "Auto-generated"}");

            // Get the latest version number
            LogDebug("Step 1: Getting latest version number...");
            var latestVersionNumber = await _versionRepository.GetLatestVersionNumberAsync(recipe.Id);
            var newVersionNumber = latestVersionNumber + 1;
            LogDebug($"✓ New version number: {newVersionNumber}");

            // Create version snapshot
            LogDebug("Step 2: Creating version snapshot...");
            var version = RecipeVersion.CreateFromRecipe(recipe, changedBy, changeDescription);
            version.VersionNumber = newVersionNumber;
            LogDebug($"✓ Snapshot created");
            LogDebug($"  Name: {version.Name}");
            LogDebug($"  Ingredients: {version.IngredientsJson?.Length ?? 0} chars");
            LogDebug($"  Total Cost: ${version.TotalCost:F2}");

            // Save version
            LogDebug("Step 3: Saving version to database...");
            var savedVersion = await _versionRepository.CreateVersionAsync(version);
            LogDebug($"✓ Version saved (ID: {savedVersion.Id})");

            LogDebug($"✓ Recipe version {newVersionNumber} created successfully");
            LogDebug("╚═══════════════════════════════════════════════════╝");

            _logger?.LogInformation(
                "Created version {VersionNumber} for recipe '{RecipeName}' (ID: {RecipeId})",
                newVersionNumber, recipe.Name, recipe.Id);

            return savedVersion;
        }
        catch (Exception ex)
        {
            LogError($"Failed to create version for recipe '{recipe.Name}'", ex);
            _logger?.LogError(ex, "Failed to create version for recipe {RecipeId}", recipe.Id);
            throw;
        }
    }

    public async Task<List<RecipeVersion>> GetVersionHistoryAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Fetching version history for recipe {recipeId}");
            var versions = await _versionRepository.GetVersionsByRecipeIdAsync(recipeId);
            LogDebug($"✓ Retrieved {versions.Count} versions");
            return versions;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch version history for recipe {recipeId}", ex);
            _logger?.LogError(ex, "Failed to fetch version history for recipe {RecipeId}", recipeId);
            throw;
        }
    }

    public async Task<RecipeVersion?> GetVersionAsync(Guid versionId)
    {
        try
        {
            LogDebug($"Fetching version {versionId}");
            var version = await _versionRepository.GetVersionByIdAsync(versionId);
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
            _logger?.LogError(ex, "Failed to fetch version {VersionId}", versionId);
            throw;
        }
    }

    public async Task<int> GetVersionCountAsync(Guid recipeId)
    {
        try
        {
            var count = await _versionRepository.GetVersionCountAsync(recipeId);
            LogDebug($"Recipe {recipeId} has {count} versions");
            return count;
        }
        catch (Exception ex)
        {
            LogError($"Failed to get version count for recipe {recipeId}", ex);
            _logger?.LogError(ex, "Failed to get version count for recipe {RecipeId}", recipeId);
            throw;
        }
    }

    public async Task<List<RecipeVersion>> GetRecentActivityAsync(int count = 20)
    {
        try
        {
            LogDebug($"Fetching {count} most recent versions");
            var versions = await _versionRepository.GetRecentVersionsAsync(count);
            LogDebug($"✓ Retrieved {versions.Count} recent versions");
            return versions;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch recent version activity", ex);
            _logger?.LogError(ex, "Failed to fetch recent version activity");
            throw;
        }
    }

    public async Task<VersionComparison> CompareVersionsAsync(Guid versionId1, Guid versionId2)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Comparing Recipe Versions                         ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Version 1 ID: {versionId1}");
            LogDebug($"Version 2 ID: {versionId2}");

            var version1 = await _versionRepository.GetVersionByIdAsync(versionId1);
            var version2 = await _versionRepository.GetVersionByIdAsync(versionId2);

            if (version1 == null || version2 == null)
            {
                LogDebug("✗ One or both versions not found");
                throw new InvalidOperationException("One or both versions not found");
            }

            LogDebug($"Version 1: v{version1.VersionNumber} - {version1.Name}");
            LogDebug($"Version 2: v{version2.VersionNumber} - {version2.Name}");

            var comparison = new VersionComparison
            {
                Version1 = version1,
                Version2 = version2,
                Differences = new List<FieldDifference>()
            };

            // Compare fields
            LogDebug("Comparing fields...");
            CompareField(comparison, "Name", version1.Name, version2.Name);
            CompareField(comparison, "Description", version1.Description, version2.Description);
            CompareField(comparison, "Category", version1.Category, version2.Category);
            CompareField(comparison, "Instructions", version1.Instructions, version2.Instructions);
            CompareField(comparison, "Yield", version1.Yield.ToString(), version2.Yield.ToString());
            CompareField(comparison, "YieldUnit", version1.YieldUnit, version2.YieldUnit);
            CompareField(comparison, "PrepTimeMinutes", version1.PrepTimeMinutes?.ToString(), version2.PrepTimeMinutes?.ToString());
            CompareField(comparison, "Difficulty", version1.Difficulty, version2.Difficulty);
            CompareField(comparison, "Tags", version1.Tags, version2.Tags);
            CompareField(comparison, "DietaryLabels", version1.DietaryLabels, version2.DietaryLabels);
            CompareField(comparison, "Notes", version1.Notes, version2.Notes);
            CompareField(comparison, "TotalCost", $"${version1.TotalCost:F2}", $"${version2.TotalCost:F2}");
            CompareField(comparison, "Ingredients", version1.IngredientsJson, version2.IngredientsJson);
            CompareField(comparison, "Allergens", version1.AllergensJson, version2.AllergensJson);

            LogDebug($"✓ Found {comparison.Differences.Count} differences");
            LogDebug("╚═══════════════════════════════════════════════════╝");

            return comparison;
        }
        catch (Exception ex)
        {
            LogError($"Failed to compare versions {versionId1} and {versionId2}", ex);
            _logger?.LogError(ex, "Failed to compare versions {VersionId1} and {VersionId2}", versionId1, versionId2);
            throw;
        }
    }

    public async Task DeleteVersionHistoryAsync(Guid recipeId)
    {
        try
        {
            LogDebug($"Deleting version history for recipe {recipeId}");
            await _versionRepository.DeleteVersionsByRecipeIdAsync(recipeId);
            LogDebug($"✓ Version history deleted");
            _logger?.LogInformation("Deleted version history for recipe {RecipeId}", recipeId);
        }
        catch (Exception ex)
        {
            LogError($"Failed to delete version history for recipe {recipeId}", ex);
            _logger?.LogError(ex, "Failed to delete version history for recipe {RecipeId}", recipeId);
            throw;
        }
    }

    private void CompareField(VersionComparison comparison, string fieldName, string? value1, string? value2)
    {
        if (value1 != value2)
        {
            comparison.Differences.Add(new FieldDifference
            {
                FieldName = fieldName,
                OldValue = value1,
                NewValue = value2,
                Type = string.IsNullOrEmpty(value1) ? DifferenceType.Added :
                       string.IsNullOrEmpty(value2) ? DifferenceType.Removed :
                       DifferenceType.Modified
            });
        }
    }
}
