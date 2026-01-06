using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Enums;
using Freecost.Core.Helpers;
using Freecost.Core.Models;
using System;

namespace Freecost.Desktop.Models;

/// <summary>
/// Unified display model for recipe components (both ingredients and sub-recipes)
/// Matches the ComponentDisplayModel pattern from entrees
/// </summary>
public partial class RecipeComponentDisplayModel : ObservableObject
{
    public object Component { get; }
    public string Name { get; }
    public string QuantityDisplay { get; }
    public decimal Cost { get; private set; }

    [ObservableProperty]
    private bool _hasMapping;

    [ObservableProperty]
    private Guid? _mappingId;

    // Track if this component points to a placeholder (unmatched import)
    public bool IsPlaceholder { get; private set; }

    // True if connected to a real ingredient/recipe, false if placeholder
    public bool IsConnected => !IsPlaceholder;

    // Constructor for RecipeRecipe (sub-recipe component)
    public RecipeComponentDisplayModel(RecipeRecipe rr)
    {
        Component = rr;
        // Use DisplayName if available (from recipe card import), otherwise use full recipe name
        Name = !string.IsNullOrWhiteSpace(rr.DisplayName) ? rr.DisplayName : rr.ComponentRecipe.Name;

        // Handle "To Taste" special case (0.01 tsp)
        if (rr.Quantity == 0.01m && rr.Unit == UnitType.Teaspoon)
        {
            QuantityDisplay = "To Taste";
        }
        else
        {
            // Format quantity: show whole numbers without decimals (e.g., "2" not "2.00")
            var qtyFormat = rr.Quantity % 1 == 0 ? rr.Quantity.ToString("0") : rr.Quantity.ToString("0.##");
            var unitAbbrev = rr.Unit != UnitType.Each ? $" {GetUnitAbbreviation(rr.Unit)}" : " portion(s)";
            QuantityDisplay = $"{qtyFormat}{unitAbbrev}";
        }

        // Check if this is a placeholder recipe from import
        IsPlaceholder = rr.ComponentRecipe.Category == "[UNMATCHED - Import]";

        Cost = CalculateRecipeCost(rr);
    }

    // Constructor for RecipeIngredient
    public RecipeComponentDisplayModel(RecipeIngredient ri, decimal cost)
    {
        Component = ri;
        // Use the existing ingredient display name
        Name = ri.IngredientDisplayName;

        // Handle "To Taste" special case (0.01 tsp)
        if (ri.Quantity == 0.01m && ri.Unit == UnitType.Teaspoon)
        {
            QuantityDisplay = "To Taste";
        }
        else
        {
            // Format quantity: show whole numbers without decimals
            var qtyFormat = ri.Quantity % 1 == 0 ? ri.Quantity.ToString("0") : ri.Quantity.ToString("0.##");
            QuantityDisplay = $"{qtyFormat} {GetUnitAbbreviation(ri.Unit)}";
        }
        Cost = cost;

        // Check if this is a placeholder ingredient from import (unmatched/missing)
        IsPlaceholder = ri.IsMissingIngredient;
    }

    private decimal CalculateRecipeCost(RecipeRecipe rr)
    {
        try
        {
            if (rr.Unit == UnitType.Each)
            {
                return rr.ComponentRecipe.Yield > 0 ? (rr.ComponentRecipe.TotalCost / rr.ComponentRecipe.Yield) * rr.Quantity : 0;
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

            decimal convertedQuantity;
            if (rr.Unit == yieldUnitType)
            {
                convertedQuantity = rr.Quantity;
            }
            else if (UnitConverter.CanConvert(rr.Unit, yieldUnitType))
            {
                try
                {
                    convertedQuantity = UnitConverter.Convert(rr.Quantity, rr.Unit, yieldUnitType);
                }
                catch
                {
                    // Conversion failed (e.g., volume to weight without density)
                    // Fall back to simple multiplication
                    return rr.ComponentRecipe.TotalCost * rr.Quantity;
                }
            }
            else
            {
                // Units are incompatible - fall back to simple multiplication
                return rr.ComponentRecipe.TotalCost * rr.Quantity;
            }

            return rr.ComponentRecipe.Yield > 0 ? (convertedQuantity / rr.ComponentRecipe.Yield) * rr.ComponentRecipe.TotalCost : 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating recipe cost: {ex.Message}");
            // Fall back to simple cost calculation
            return rr.ComponentRecipe.TotalCost * rr.Quantity;
        }
    }

    private string GetUnitAbbreviation(UnitType unit)
    {
        return unit switch
        {
            UnitType.Gram => "g",
            UnitType.Kilogram => "kg",
            UnitType.Ounce => "oz",
            UnitType.Pound => "lb",
            UnitType.Milliliter => "mL",
            UnitType.Liter => "L",
            UnitType.FluidOunce => "fl oz",
            UnitType.Cup => "cup",
            UnitType.Pint => "pt",
            UnitType.Quart => "qt",
            UnitType.Gallon => "gal",
            UnitType.Tablespoon => "tbsp",
            UnitType.Teaspoon => "tsp",
            UnitType.Each => "ea",
            _ => unit.ToString()
        };
    }
}
