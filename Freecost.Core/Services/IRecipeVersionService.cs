using Freecost.Core.Models;

namespace Freecost.Core.Services;

/// <summary>
/// Service for managing recipe version control and history
/// </summary>
public interface IRecipeVersionService
{
    /// <summary>
    /// Creates a new version snapshot of a recipe
    /// </summary>
    Task<RecipeVersion> CreateVersionAsync(Recipe recipe, string? changedBy, string? changeDescription);

    /// <summary>
    /// Gets all versions for a specific recipe
    /// </summary>
    Task<List<RecipeVersion>> GetVersionHistoryAsync(Guid recipeId);

    /// <summary>
    /// Gets a specific version by ID
    /// </summary>
    Task<RecipeVersion?> GetVersionAsync(Guid versionId);

    /// <summary>
    /// Gets the version count for a recipe
    /// </summary>
    Task<int> GetVersionCountAsync(Guid recipeId);

    /// <summary>
    /// Gets recent version activity across all recipes
    /// </summary>
    Task<List<RecipeVersion>> GetRecentActivityAsync(int count = 20);

    /// <summary>
    /// Compares two versions and returns the differences
    /// </summary>
    Task<VersionComparison> CompareVersionsAsync(Guid versionId1, Guid versionId2);

    /// <summary>
    /// Deletes all versions for a recipe (used when recipe is permanently deleted)
    /// </summary>
    Task DeleteVersionHistoryAsync(Guid recipeId);
}

/// <summary>
/// Result of comparing two recipe versions
/// </summary>
public class VersionComparison
{
    public RecipeVersion Version1 { get; set; } = null!;
    public RecipeVersion Version2 { get; set; } = null!;
    public List<FieldDifference> Differences { get; set; } = new();

    public bool HasDifferences => Differences.Any();
}

/// <summary>
/// Represents a difference between two versions
/// </summary>
public class FieldDifference
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DifferenceType Type { get; set; }
}

public enum DifferenceType
{
    Added,
    Removed,
    Modified
}
