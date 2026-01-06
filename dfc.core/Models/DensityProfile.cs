using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Dfc.Core.Models;

/// <summary>
/// Represents volume-to-weight conversion factors for an ingredient category.
/// Enables conversions like "1 cup flour → 120 grams" without ingredient-specific data.
/// </summary>
public class DensityProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Category name (e.g., "Flour-AllPurpose", "Sugar-White", "Liquid-Water")
    /// Must be unique
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Array of keywords for fuzzy matching ingredient names.
    /// Example: ["flour", "all-purpose", "ap", "wheat flour", "plain flour"]
    /// Case-insensitive matching in queries.
    /// </summary>
    [Required]
    public string[] Keywords { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Grams per cup (volume).
    /// Example: All-purpose flour = 120 g/cup
    /// </summary>
    [Range(0.01, 100000)]
    public decimal? GramsPerCup { get; set; }

    /// <summary>
    /// Grams per tablespoon (volume).
    /// Example: All-purpose flour = 7.5 g/tbsp
    /// </summary>
    [Range(0.01, 10000)]
    public decimal? GramsPerTablespoon { get; set; }

    /// <summary>
    /// Grams per teaspoon (volume).
    /// Example: All-purpose flour = 2.5 g/tsp
    /// </summary>
    [Range(0.01, 10000)]
    public decimal? GramsPerTeaspoon { get; set; }

    /// <summary>
    /// Grams per fluid ounce (volume).
    /// Example: Water = 29.6 g/fl oz
    /// </summary>
    [Range(0.01, 10000)]
    public decimal? GramsPerFluidOunce { get; set; }

    /// <summary>
    /// Source of this density data
    /// - BuiltIn: From StandardConversions.json
    /// - USDA: From FoodData Central
    /// - UserDefined: Manually entered
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Source { get; set; } = "BuiltIn";

    /// <summary>
    /// Optional notes about this density profile
    /// Example: "Spooned and leveled, not packed"
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// USDA FoodData Central ID if applicable
    /// </summary>
    [MaxLength(50)]
    public string? UsdaFdcId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if an ingredient name matches this density profile's keywords.
    /// Case-insensitive, checks if ingredient contains any keyword.
    /// </summary>
    public bool MatchesIngredient(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || Keywords == null || Keywords.Length == 0)
            return false;

        var lowerName = ingredientName.ToLowerInvariant();
        return Keywords.Any(keyword =>
            lowerName.Contains(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Returns the density factor for a specific unit, or null if not available.
    /// </summary>
    public decimal? GetDensityForUnit(Enums.UnitType unit)
    {
        return unit switch
        {
            Enums.UnitType.Cup => GramsPerCup,
            Enums.UnitType.Tablespoon => GramsPerTablespoon,
            Enums.UnitType.Teaspoon => GramsPerTeaspoon,
            Enums.UnitType.FluidOunce => GramsPerFluidOunce,
            _ => null
        };
    }

    /// <summary>
    /// Checks if this profile has at least one density value
    /// </summary>
    [NotMapped]
    public bool HasAnyDensity =>
        GramsPerCup.HasValue ||
        GramsPerTablespoon.HasValue ||
        GramsPerTeaspoon.HasValue ||
        GramsPerFluidOunce.HasValue;

    /// <summary>
    /// Returns a display-friendly keyword list
    /// </summary>
    [NotMapped]
    public string KeywordsDisplay => string.Join(", ", Keywords);

    /// <summary>
    /// Validates that the profile has required data
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Category) &&
               Keywords != null &&
               Keywords.Length > 0 &&
               HasAnyDensity &&
               !string.IsNullOrWhiteSpace(Source);
    }
}
