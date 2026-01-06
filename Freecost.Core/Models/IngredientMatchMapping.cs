using System;
using System.ComponentModel.DataAnnotations;

namespace Freecost.Core.Models;

/// <summary>
/// Stores user-defined mappings between imported names and actual ingredients/recipes
/// This allows the system to "remember" previous matches during imports
/// </summary>
public class IngredientMatchMapping
{
    /// <summary>
    /// Primary key
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The imported name that needs to be matched (e.g., "Garlic, minced")
    /// This is normalized (lowercase, trimmed) for consistent matching
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ImportName { get; set; } = string.Empty;

    /// <summary>
    /// The ingredient this import name maps to (if matching to an ingredient)
    /// Either MatchedIngredientId OR MatchedRecipeId should be set, but not both
    /// </summary>
    public Guid? MatchedIngredientId { get; set; }

    /// <summary>
    /// The recipe this import name maps to (if matching to a recipe)
    /// Either MatchedIngredientId OR MatchedRecipeId should be set, but not both
    /// </summary>
    public Guid? MatchedRecipeId { get; set; }

    /// <summary>
    /// Location/Restaurant this mapping belongs to (for multi-tenant support)
    /// </summary>
    [Required]
    public Guid LocationId { get; set; }

    /// <summary>
    /// When this mapping was first created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this mapping was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    // Navigation properties
    public Ingredient? MatchedIngredient { get; set; }
    public Recipe? MatchedRecipe { get; set; }
    public Location? Location { get; set; }
}
