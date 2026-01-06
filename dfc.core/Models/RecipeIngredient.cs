using Dfc.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfc.Core.Models;

public class RecipeIngredient
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }

    /// <summary>
    /// Foreign key to Ingredient. NULL if ingredient doesn't exist in database yet.
    /// </summary>
    public Guid? IngredientId { get; set; }

    /// <summary>
    /// When IngredientId is null, stores the ingredient name from recipe card import.
    /// This allows recipes to be created even when ingredients don't exist yet.
    /// </summary>
    public string? UnmatchedIngredientName { get; set; }

    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; }
    public string? DisplayText { get; set; }
    public bool IsOptional { get; set; } = false;
    public int SortOrder { get; set; } = 0;

    // Navigation
    public Recipe Recipe { get; set; } = null!;
    public Ingredient? Ingredient { get; set; }

    // Calculated properties (not stored in DB)
    [NotMapped]
    public decimal CalculatedCost { get; set; }

    [NotMapped]
    public bool HasValidCost { get; set; }

    [NotMapped]
    public string? CostWarningMessage { get; set; }

    /// <summary>
    /// True if this ingredient is missing from the database (not yet added to ingredients list)
    /// </summary>
    [NotMapped]
    public bool IsMissingIngredient => !IngredientId.HasValue && !string.IsNullOrWhiteSpace(UnmatchedIngredientName);

    /// <summary>
    /// Display name for this ingredient (either from database or unmatched name)
    /// </summary>
    [NotMapped]
    public string IngredientDisplayName => Ingredient?.Name ?? UnmatchedIngredientName ?? "Unknown";

    /// <summary>
    /// Calculated nutritional information for this ingredient quantity
    /// Based on ingredient's nutrition per unit × quantity used
    /// </summary>
    [NotMapped]
    public NutritionInfo CalculatedNutrition
    {
        get
        {
            if (Ingredient == null) return NutritionInfo.Zero;

            try
            {
                // Convert recipe quantity to ingredient's purchase unit
                var convertedQuantity = Helpers.UnitConverter.CanConvert(Unit, Ingredient.Unit)
                    ? Helpers.UnitConverter.Convert(Quantity, Unit, Ingredient.Unit)
                    : Quantity;  // If can't convert, use quantity as-is (may be inaccurate)

                return new NutritionInfo
                {
                    Calories = (Ingredient.CaloriesPerUnit ?? 0) * convertedQuantity,
                    Protein = (Ingredient.ProteinPerUnit ?? 0) * convertedQuantity,
                    Carbohydrates = (Ingredient.CarbohydratesPerUnit ?? 0) * convertedQuantity,
                    Fat = (Ingredient.FatPerUnit ?? 0) * convertedQuantity,
                    Fiber = (Ingredient.FiberPerUnit ?? 0) * convertedQuantity,
                    Sugar = (Ingredient.SugarPerUnit ?? 0) * convertedQuantity,
                    Sodium = (Ingredient.SodiumPerUnit ?? 0) * convertedQuantity
                };
            }
            catch
            {
                // If conversion fails, return zero nutrition
                return NutritionInfo.Zero;
            }
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
