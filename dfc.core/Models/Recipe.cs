using Dfc.Core.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Dfc.Core.Models;

public class Recipe : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Hex color code for this category (e.g., "#E91E63")
    /// Persisted to ensure consistent category colors
    /// </summary>
    [MaxLength(7)] // #RRGGBB format
    public string? CategoryColor { get; set; }

    [MaxLength(10000)]
    public string? Instructions { get; set; }

    public decimal Yield { get; set; }

    [MaxLength(50)]
    public string YieldUnit { get; set; } = string.Empty;

    public int? PrepTimeMinutes { get; set; }
    public Guid LocationId { get; set; }
    public bool IsShared { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    // v1.2.0 additions
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.NotSet;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated tags

    // v1.3.0 - DEPRECATED: Nutrition is now calculated from ingredients
    // These fields are kept for backward compatibility but should not be used
    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Calories { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Protein { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Carbohydrates { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Fat { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Fiber { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Sugar { get; set; }

    [Obsolete("Use CalculatedNutrition property instead. Nutrition is now calculated from ingredients.")]
    public decimal? Sodium { get; set; }

    public string? DietaryLabels { get; set; } // Comma-separated: Vegan, GlutenFree, DairyFree, etc.

    // v1.4.0 additions - Pricing for standalone sellable recipes
    public decimal? SuggestedMenuPrice { get; set; } // Price per serving if sold as standalone item

    // Calculated properties (not stored) - calculated from ingredients AND sub-recipes
    [NotMapped]
    public decimal TotalCost
    {
        get
        {
            decimal ingredientsCost = RecipeIngredients?.Sum(ri => ri.CalculatedCost) ?? 0;
            decimal recipesCost = RecipeRecipes?.Sum(rr => CalculateRecipeComponentCost(rr)) ?? 0;
            return ingredientsCost + recipesCost;
        }
    }

    private decimal CalculateRecipeComponentCost(RecipeRecipe rr)
    {
        try
        {
            if (rr.Unit == UnitType.Each)
            {
                return rr.ComponentRecipe.Yield > 0
                    ? (rr.ComponentRecipe.TotalCost / rr.ComponentRecipe.Yield) * rr.Quantity
                    : 0;
            }

            if (!Enum.TryParse<UnitType>(rr.ComponentRecipe.YieldUnit, true, out var yieldUnitType))
            {
                yieldUnitType = rr.ComponentRecipe.YieldUnit.ToLower() switch
                {
                    "oz" or "ounce" or "ounces" => UnitType.Ounce,
                    "lb" or "pound" or "pounds" => UnitType.Pound,
                    "g" or "gram" or "grams" => UnitType.Gram,
                    "kg" or "kilogram" or "kilograms" => UnitType.Kilogram,
                    "cup" or "cups" => UnitType.Cup,
                    "pint" or "pints" or "pt" => UnitType.Pint,
                    "quart" or "quarts" or "qt" => UnitType.Quart,
                    "gallon" or "gallons" or "gal" => UnitType.Gallon,
                    "ml" or "milliliter" or "milliliters" => UnitType.Milliliter,
                    "liter" or "liters" or "l" => UnitType.Liter,
                    "fl oz" or "floz" or "fluid ounce" or "fluid ounces" => UnitType.FluidOunce,
                    "tbsp" or "tablespoon" or "tablespoons" => UnitType.Tablespoon,
                    "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
                    _ => UnitType.Each
                };
            }

            decimal convertedQuantity = rr.Quantity;
            if (rr.Unit != yieldUnitType && Core.Helpers.UnitConverter.CanConvert(rr.Unit, yieldUnitType))
            {
                try
                {
                    convertedQuantity = Core.Helpers.UnitConverter.Convert(rr.Quantity, rr.Unit, yieldUnitType);
                }
                catch
                {
                    return rr.ComponentRecipe.TotalCost * rr.Quantity;
                }
            }

            return rr.ComponentRecipe.Yield > 0
                ? (convertedQuantity / rr.ComponentRecipe.Yield) * rr.ComponentRecipe.TotalCost
                : 0;
        }
        catch
        {
            return rr.ComponentRecipe.TotalCost * rr.Quantity;
        }
    }

    [NotMapped]
    public decimal CostPerServing => Yield > 0 ? TotalCost / Yield : 0;

    /// <summary>
    /// Total nutritional information for entire recipe (ingredients + sub-recipes combined)
    /// </summary>
    [NotMapped]
    public NutritionInfo CalculatedNutrition
    {
        get
        {
            var ingredientsNutrition = RecipeIngredients?.Any() == true
                ? RecipeIngredients.Select(ri => ri.CalculatedNutrition).Aggregate((a, b) => a + b)
                : NutritionInfo.Zero;

            var recipesNutrition = RecipeRecipes?.Any() == true
                ? RecipeRecipes.Select(rr => CalculateRecipeComponentNutrition(rr)).Aggregate((a, b) => a + b)
                : NutritionInfo.Zero;

            return ingredientsNutrition + recipesNutrition;
        }
    }

    private NutritionInfo CalculateRecipeComponentNutrition(RecipeRecipe rr)
    {
        try
        {
            // Get nutrition for the entire component recipe
            var componentNutrition = rr.ComponentRecipe.CalculatedNutrition;

            // Scale based on quantity used vs total yield
            if (rr.Unit == UnitType.Each && rr.ComponentRecipe.Yield > 0)
            {
                var portionFraction = rr.Quantity / rr.ComponentRecipe.Yield;
                return componentNutrition * portionFraction;
            }

            // For other units, attempt unit conversion to yield unit
            if (!Enum.TryParse<UnitType>(rr.ComponentRecipe.YieldUnit, true, out var yieldUnitType))
            {
                // Fallback: assume simple proportion
                return componentNutrition * rr.Quantity;
            }

            decimal convertedQuantity = rr.Quantity;
            if (rr.Unit != yieldUnitType && Core.Helpers.UnitConverter.CanConvert(rr.Unit, yieldUnitType))
            {
                try
                {
                    convertedQuantity = Core.Helpers.UnitConverter.Convert(rr.Quantity, rr.Unit, yieldUnitType);
                }
                catch
                {
                    return componentNutrition * rr.Quantity;
                }
            }

            if (rr.ComponentRecipe.Yield > 0)
            {
                var portionFraction = convertedQuantity / rr.ComponentRecipe.Yield;
                return componentNutrition * portionFraction;
            }

            return NutritionInfo.Zero;
        }
        catch
        {
            return NutritionInfo.Zero;
        }
    }

    /// <summary>
    /// Nutritional information per serving
    /// </summary>
    [NotMapped]
    public NutritionInfo NutritionPerServing => Yield > 0 ? CalculatedNutrition / Yield : NutritionInfo.Zero;

    // Navigation properties
    public Location Location { get; set; } = null!;
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeRecipe> RecipeRecipes { get; set; } = new List<RecipeRecipe>(); // Recipe components
    public ICollection<EntreeRecipe> EntreeRecipes { get; set; } = new List<EntreeRecipe>();
    public ICollection<RecipeAllergen> RecipeAllergens { get; set; } = new List<RecipeAllergen>();
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
