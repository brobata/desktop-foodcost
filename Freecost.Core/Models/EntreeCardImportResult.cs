using Freecost.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Core.Models;

/// <summary>
/// Result of parsing an entree card Excel file
/// </summary>
public class EntreeCardImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<EntreeCardPreview> EntreePreviews { get; set; } = new();
    public List<UnmatchedIngredient> UnmatchedIngredients { get; set; } = new();
    public List<UnmatchedRecipe> UnmatchedRecipes { get; set; } = new();
    public int TotalTabs { get; set; }
    public int ValidEntrees { get; set; }
    public int InvalidEntrees { get; set; }

    public string Summary => Success
        ? $"{ValidEntrees} valid entree(s) ready to import from {TotalTabs} tab(s)"
        : $"Failed to parse: {ErrorMessage}";
}

/// <summary>
/// Preview of a single entree to be imported
/// </summary>
public class EntreeCardPreview
{
    public string TabName { get; set; } = string.Empty;
    public int TabIndex { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();

    // Entree metadata
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? MenuPrice { get; set; }
    public string? PlatingEquipment { get; set; }
    public string? PhotoUrl { get; set; }

    // Direct ingredients
    public List<EntreeIngredientPreview> DirectIngredients { get; set; } = new();

    // Recipe components
    public List<EntreeRecipePreview> RecipeComponents { get; set; } = new();

    // Preparation procedures/instructions
    public List<string> Procedures { get; set; } = new();

    // Validation helpers
    public string ValidationStatus => IsValid ? "✓ Valid" : $"✗ {ValidationErrors.Count} error(s)";
    public string ComponentSummary
    {
        get
        {
            var matched = DirectIngredients.Count(i => i.IsMatched) + RecipeComponents.Count(r => r.IsMatched);
            var unmatched = DirectIngredients.Count(i => !i.IsMatched) + RecipeComponents.Count(r => !r.IsMatched);
            return $"{matched} matched, {unmatched} unmatched";
        }
    }
}

/// <summary>
/// Preview of a direct ingredient in an entree
/// </summary>
public class EntreeIngredientPreview
{
    public string OriginalText { get; set; } = string.Empty;
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; }

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
/// Preview of a recipe component in an entree
/// </summary>
public class EntreeRecipePreview
{
    public string OriginalText { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; }

    // Matching information
    public bool IsMatched { get; set; }
    public Guid? MatchedRecipeId { get; set; }
    public string? MatchedRecipeName { get; set; }
    public decimal? MatchConfidence { get; set; } // 0-100%
    public string? MatchMethod { get; set; } // "Exact", "Fuzzy", "Manual"

    public string MatchStatus => IsMatched
        ? $"✓ {MatchedRecipeName} ({MatchConfidence:F0}%)"
        : $"✗ No match for recipe '{RecipeName}'";
}

/// <summary>
/// Recipe referenced in entree card but not found in database
/// </summary>
public class UnmatchedRecipe
{
    public string Name { get; set; } = string.Empty;
    public List<string> AppearsInEntrees { get; set; } = new();
    public int UsageCount { get; set; }

    // Suggested matches (for user to select from)
    public List<RecipeMatchSuggestion> Suggestions { get; set; } = new();

    // User's manual mapping choice
    public Guid? MappedToRecipeId { get; set; }
}

/// <summary>
/// Suggested match for an unmatched recipe
/// </summary>
public class RecipeMatchSuggestion
{
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal Confidence { get; set; } // 0-100%
    public string Reason { get; set; } = string.Empty; // e.g., "Similar name", "Category match"
}
