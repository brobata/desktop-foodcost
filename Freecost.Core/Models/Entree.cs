using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Freecost.Core.Models;

public class Entree : BaseEntity
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

    public decimal? MenuPrice { get; set; }
    public Guid LocationId { get; set; }

    // Photo support
    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    // Plating equipment
    [MaxLength(500)]
    public string? PlatingEquipment { get; set; }

    // Preparation instructions/procedures
    public string? PreparationInstructions { get; set; }

    // Calculated properties (not stored in DB)
    [NotMapped]
    public decimal TotalCost { get; set; }

    [NotMapped]
    public decimal FoodCostPercentage => MenuPrice.HasValue && MenuPrice > 0 && TotalCost > 0
        ? (TotalCost / MenuPrice.Value) * 100
        : 0;

    /// <summary>
    /// Total nutritional information for entire entree
    /// Aggregates nutrition from direct ingredients and recipes
    /// </summary>
    [NotMapped]
    public NutritionInfo CalculatedNutrition
    {
        get
        {
            var directIngredientNutrition = EntreeIngredients?
                .Select(ei => ei.CalculatedNutrition)
                .Aggregate(NutritionInfo.Zero, (a, b) => a + b) ?? NutritionInfo.Zero;

            var recipeNutrition = EntreeRecipes?
                .Where(er => er.Recipe != null)
                .Select(er => er.Recipe.CalculatedNutrition * er.Quantity)  // Multiply by recipe quantity
                .Aggregate(NutritionInfo.Zero, (a, b) => a + b) ?? NutritionInfo.Zero;

            return directIngredientNutrition + recipeNutrition;
        }
    }

    // Navigation properties
    public Location Location { get; set; } = null!;
    public ICollection<EntreeRecipe> EntreeRecipes { get; set; } = new List<EntreeRecipe>();
    public ICollection<EntreeIngredient> EntreeIngredients { get; set; } = new List<EntreeIngredient>();
    public ICollection<EntreeAllergen> EntreeAllergens { get; set; } = new List<EntreeAllergen>();
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
}
