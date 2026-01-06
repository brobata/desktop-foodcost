using Freecost.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Freecost.Core.Models;

public class EntreeIngredient : BaseEntity
{
    public Guid EntreeId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public UnitType Unit { get; set; }

    /// <summary>
    /// Display name for this ingredient (e.g., "Linguini", "Canola Oil")
    /// Preserves the original name from entree card import, which may be simpler than the full database ingredient name
    /// </summary>
    public string? DisplayName { get; set; }

    // Navigation properties
    public Entree Entree { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;

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
                // Convert entree quantity to ingredient's purchase unit
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
}
