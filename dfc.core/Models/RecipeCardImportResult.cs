using Dfc.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dfc.Core.Models;

/// <summary>
/// Result of parsing a recipe card Excel file
/// </summary>
public class RecipeCardImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<RecipeCardPreview> RecipePreviews { get; set; } = new();
    public List<UnmatchedIngredient> UnmatchedIngredients { get; set; } = new();
    public int TotalTabs { get; set; }
    public int ValidRecipes { get; set; }
    public int InvalidRecipes { get; set; }

    public string Summary => Success
        ? $"{ValidRecipes} valid recipe(s) ready to import from {TotalTabs} tab(s)"
        : $"Failed to parse: {ErrorMessage}";
}

/// <summary>
/// Preview of a single recipe to be imported
/// </summary>
public class RecipeCardPreview
{
    public string TabName { get; set; } = string.Empty;
    public int TabIndex { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();

    // Recipe metadata
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Instructions { get; set; }
    public decimal Yield { get; set; }
    public string YieldUnit { get; set; } = string.Empty;
    public int? PrepTimeMinutes { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }

    // Ingredient information
    public List<RecipeIngredientPreview> Ingredients { get; set; } = new();

    // Validation helpers
    public string ValidationStatus => IsValid ? "✓ Valid" : $"✗ {ValidationErrors.Count} error(s)";
    public string IngredientSummary => $"{Ingredients.Count(i => i.IsMatched)} matched, {Ingredients.Count(i => !i.IsMatched)} unmatched";
}

/// <summary>
/// Preview of a recipe ingredient
/// </summary>
public class RecipeIngredientPreview
{
    public string OriginalText { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; }
    public string? DisplayText { get; set; }
    public bool IsOptional { get; set; }

    // Matching information
    public bool IsMatched { get; set; }
    public Guid? MatchedIngredientId { get; set; }
    public string? MatchedIngredientName { get; set; }
    public decimal? MatchConfidence { get; set; } // 0-100%
    public string? MatchMethod { get; set; } // "Exact", "Fuzzy", "Alias", "Manual"

    public string MatchStatus => IsMatched
        ? $"✓ {MatchedIngredientName} ({MatchConfidence:F0}%)"
        : $"✗ No match for '{IngredientName}'";
}

/// <summary>
/// Ingredient referenced in recipe card but not found in database
/// </summary>
public class UnmatchedIngredient
{
    public string Name { get; set; } = string.Empty;
    public List<string> AppearsInRecipes { get; set; } = new();
    public int UsageCount { get; set; }

    // Suggested matches (for user to select from)
    public ObservableCollection<IngredientMatchSuggestion> Suggestions { get; set; } = new();

    // User's manual mapping choice
    public Guid? MappedToIngredientId { get; set; }
}

/// <summary>
/// Suggested match for an unmatched ingredient
/// </summary>
public class IngredientMatchSuggestion
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal Confidence { get; set; } // 0-100%
    public string Reason { get; set; } = string.Empty; // e.g., "Similar name", "Category match"
}
