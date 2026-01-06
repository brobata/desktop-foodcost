using Freecost.Core.Models;

namespace Freecost.Core.Repositories;

/// <summary>
/// Repository for managing recipe version history
/// </summary>
public interface IRecipeVersionRepository
{
    /// <summary>
    /// Creates a new version snapshot of a recipe
    /// </summary>
    Task<RecipeVersion> CreateVersionAsync(RecipeVersion version);

    /// <summary>
    /// Gets all versions for a specific recipe, ordered by version number descending
    /// </summary>
    Task<List<RecipeVersion>> GetVersionsByRecipeIdAsync(Guid recipeId);

    /// <summary>
    /// Gets a specific version by ID
    /// </summary>
    Task<RecipeVersion?> GetVersionByIdAsync(Guid versionId);

    /// <summary>
    /// Gets the latest version number for a recipe
    /// </summary>
    Task<int> GetLatestVersionNumberAsync(Guid recipeId);

    /// <summary>
    /// Gets the most recent N versions across all recipes
    /// </summary>
    Task<List<RecipeVersion>> GetRecentVersionsAsync(int count = 20);

    /// <summary>
    /// Deletes all versions for a recipe (used when recipe is permanently deleted)
    /// </summary>
    Task DeleteVersionsByRecipeIdAsync(Guid recipeId);

    /// <summary>
    /// Gets version count for a recipe
    /// </summary>
    Task<int> GetVersionCountAsync(Guid recipeId);
}
