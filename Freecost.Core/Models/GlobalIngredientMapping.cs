using System;
using System.ComponentModel.DataAnnotations;

namespace Freecost.Core.Models;

/// <summary>
/// Represents a global ingredient/recipe mapping stored in Firebase
/// These are admin-curated mappings that work for all users/locations
/// Unlike IngredientMatchMapping, these store target NAMES instead of IDs
/// since ingredient/recipe IDs are location-specific
/// </summary>
public class GlobalIngredientMapping
{
    /// <summary>
    /// Firestore document ID (auto-generated or based on ImportName)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The imported name that needs to be matched (e.g., "Garlic, minced")
    /// This is normalized (lowercase, trimmed) for consistent matching
    /// </summary>
    [Required]
    public string ImportName { get; set; } = string.Empty;

    /// <summary>
    /// The ingredient NAME this import name should map to (if matching to an ingredient)
    /// Either MatchedIngredientName OR MatchedRecipeName should be set, but not both
    /// The actual ingredient ID will be looked up locally using fuzzy matching
    /// </summary>
    public string? MatchedIngredientName { get; set; }

    /// <summary>
    /// The recipe NAME this import name should map to (if matching to a recipe)
    /// Either MatchedIngredientName OR MatchedRecipeName should be set, but not both
    /// The actual recipe ID will be looked up locally using fuzzy matching
    /// </summary>
    public string? MatchedRecipeName { get; set; }

    /// <summary>
    /// Optional notes about this mapping (e.g., "Common variation in Excel imports")
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this mapping was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this mapping was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Who created/manages this mapping (admin username or email)
    /// </summary>
    public string? CreatedBy { get; set; }
}
