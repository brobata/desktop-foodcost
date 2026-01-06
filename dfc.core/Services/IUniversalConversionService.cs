using Dfc.Core.Enums;
using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Universal conversion service with 3-layer fallback:
/// 1. Ingredient-specific conversions (from database)
/// 2. USDA/Built-in conversions (from ConversionDatabaseService + density profiles)
/// 3. Standard conversions (from UnitConversionService - e.g., oz to g, cups to ml)
/// </summary>
public interface IUniversalConversionService
{
    /// <summary>
    /// Converts a quantity from one unit to another using all available conversion sources.
    /// Returns null if no conversion path is found.
    /// </summary>
    /// <param name="quantity">The quantity to convert</param>
    /// <param name="fromUnit">The source unit</param>
    /// <param name="toUnit">The target unit</param>
    /// <param name="ingredientId">Optional ingredient ID for ingredient-specific conversions</param>
    /// <param name="ingredientName">Optional ingredient name for density profile matching</param>
    /// <param name="locationId">Optional location ID for location-specific conversions</param>
    /// <returns>Converted quantity, or null if conversion not possible</returns>
    Task<decimal?> ConvertAsync(
        decimal quantity,
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null);

    /// <summary>
    /// Checks if a conversion is possible between two units for a given ingredient.
    /// </summary>
    Task<bool> CanConvertAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null);

    /// <summary>
    /// Gets the conversion source that would be used for a conversion.
    /// Returns "IngredientSpecific", "USDA", "BuiltIn", "Standard", or null if not possible.
    /// </summary>
    Task<string?> GetConversionSourceAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null);

    /// <summary>
    /// Gets the conversion ratio between two units (e.g., 1 cup = 237 grams).
    /// Returns null if no conversion is available.
    /// </summary>
    Task<decimal?> GetConversionRatioAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null);
}
