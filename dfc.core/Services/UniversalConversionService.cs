using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dfc.Core.Services;

/// <summary>
/// Universal conversion service with 3-layer fallback system.
/// Tries ingredient-specific conversions first, then built-in/USDA, then standard conversions.
/// </summary>
public class UniversalConversionService : IUniversalConversionService
{
    private readonly IIngredientConversionRepository _conversionRepository;
    private readonly ConversionDatabaseService _conversionDatabase;
    private readonly UnitConversionService _unitConversionService;
    private readonly ILogger<UniversalConversionService>? _logger;

    public UniversalConversionService(
        IIngredientConversionRepository conversionRepository,
        ConversionDatabaseService conversionDatabase,
        UnitConversionService unitConversionService,
        ILogger<UniversalConversionService>? logger = null)
    {
        _conversionRepository = conversionRepository;
        _conversionDatabase = conversionDatabase;
        _unitConversionService = unitConversionService;
        _logger = logger;
    }

    public async Task<decimal?> ConvertAsync(
        decimal quantity,
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null)
    {
        // Short-circuit if same unit
        if (fromUnit == toUnit)
            return quantity;

        Debug.WriteLine($"[UniversalConversion] Converting {quantity} {fromUnit} → {toUnit} (Ingredient: {ingredientName ?? "N/A"})");

        // Layer 1: Try ingredient-specific conversion from database
        if (ingredientId.HasValue)
        {
            var ingredientConversion = await _conversionRepository.GetConversionAsync(
                ingredientId.Value,
                fromUnit,
                toUnit,
                locationId);

            if (ingredientConversion != null)
            {
                var result = quantity * (ingredientConversion.ToQuantity / ingredientConversion.FromQuantity);
                Debug.WriteLine($"[UniversalConversion] ✓ Layer 1 (Ingredient-Specific): {result} {toUnit}");
                _logger?.LogDebug("Converted {Quantity} {FromUnit} → {Result} {ToUnit} using ingredient-specific conversion",
                    quantity, fromUnit, result, toUnit);
                return result;
            }
        }

        // Layer 2a: Try USDA/Built-in conversion from ConversionDatabaseService
        if (!string.IsNullOrWhiteSpace(ingredientName))
        {
            var builtInResult = await TryBuiltInConversionAsync(quantity, fromUnit, toUnit, ingredientName);
            if (builtInResult.HasValue)
            {
                Debug.WriteLine($"[UniversalConversion] ✓ Layer 2 (Built-In/USDA): {builtInResult.Value} {toUnit}");
                _logger?.LogDebug("Converted {Quantity} {FromUnit} → {Result} {ToUnit} using built-in conversion for {IngredientName}",
                    quantity, fromUnit, builtInResult.Value, toUnit, ingredientName);
                return builtInResult.Value;
            }
        }

        // Layer 3: Try standard unit conversion (oz→g, cups→ml, etc.)
        if (_unitConversionService.CanConvert(fromUnit, toUnit))
        {
            var result = _unitConversionService.Convert(quantity, fromUnit, toUnit);
            Debug.WriteLine($"[UniversalConversion] ✓ Layer 3 (Standard): {result} {toUnit}");
            _logger?.LogDebug("Converted {Quantity} {FromUnit} → {Result} {ToUnit} using standard conversion",
                quantity, fromUnit, result, toUnit);
            return result;
        }

        // No conversion found
        Debug.WriteLine($"[UniversalConversion] ✗ No conversion found for {fromUnit} → {toUnit}");
        _logger?.LogWarning("No conversion found for {FromUnit} → {ToUnit} (Ingredient: {IngredientName})",
            fromUnit, toUnit, ingredientName ?? "N/A");
        return null;
    }

    public async Task<bool> CanConvertAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null)
    {
        if (fromUnit == toUnit)
            return true;

        // Check ingredient-specific conversion
        if (ingredientId.HasValue)
        {
            var ingredientConversion = await _conversionRepository.GetConversionAsync(
                ingredientId.Value,
                fromUnit,
                toUnit,
                locationId);

            if (ingredientConversion != null)
                return true;
        }

        // Check built-in conversions
        if (!string.IsNullOrWhiteSpace(ingredientName))
        {
            var builtInResult = await TryBuiltInConversionAsync(1.0m, fromUnit, toUnit, ingredientName);
            if (builtInResult.HasValue)
                return true;
        }

        // Check standard conversions
        return _unitConversionService.CanConvert(fromUnit, toUnit);
    }

    public async Task<string?> GetConversionSourceAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null)
    {
        if (fromUnit == toUnit)
            return "SameUnit";

        // Check ingredient-specific
        if (ingredientId.HasValue)
        {
            var ingredientConversion = await _conversionRepository.GetConversionAsync(
                ingredientId.Value,
                fromUnit,
                toUnit,
                locationId);

            if (ingredientConversion != null)
                return "IngredientSpecific";
        }

        // Check built-in
        if (!string.IsNullOrWhiteSpace(ingredientName))
        {
            var builtInResult = await TryBuiltInConversionAsync(1.0m, fromUnit, toUnit, ingredientName);
            if (builtInResult.HasValue)
                return "BuiltIn";
        }

        // Check standard
        if (_unitConversionService.CanConvert(fromUnit, toUnit))
            return "Standard";

        return null;
    }

    public async Task<decimal?> GetConversionRatioAsync(
        UnitType fromUnit,
        UnitType toUnit,
        Guid? ingredientId = null,
        string? ingredientName = null,
        Guid? locationId = null)
    {
        // Just convert 1 unit to get the ratio
        return await ConvertAsync(1.0m, fromUnit, toUnit, ingredientId, ingredientName, locationId);
    }

    /// <summary>
    /// Tries to convert using built-in conversion database (density profiles + standard conversions).
    /// </summary>
    private Task<decimal?> TryBuiltInConversionAsync(
        decimal quantity,
        UnitType fromUnit,
        UnitType toUnit,
        string ingredientName)
    {
        // Try density profile for volume → weight conversions
        var densityProfile = _conversionDatabase.FindDensityProfile(ingredientName);
        if (densityProfile != null)
        {
            var volumeToWeightResult = TryDensityProfileConversion(quantity, fromUnit, toUnit, densityProfile);
            if (volumeToWeightResult.HasValue)
                return Task.FromResult<decimal?>(volumeToWeightResult.Value);
        }

        // Try standard conversions from database (e.g., "1 each egg = 50g")
        var standardConversions = _conversionDatabase.FindStandardConversions(ingredientName);
        foreach (var conversion in standardConversions)
        {
            // Check if this conversion matches our units
            if (conversion.FromUnit == fromUnit && conversion.ToUnit == toUnit)
            {
                return Task.FromResult<decimal?>(quantity * (conversion.ToQuantity / conversion.FromQuantity));
            }

            // Try reverse conversion
            if (conversion.FromUnit == toUnit && conversion.ToUnit == fromUnit)
            {
                return Task.FromResult<decimal?>(quantity / (conversion.ToQuantity / conversion.FromQuantity));
            }
        }

        return Task.FromResult<decimal?>(null);
    }

    /// <summary>
    /// Tries to convert using density profile (volume ↔ weight).
    /// </summary>
    private decimal? TryDensityProfileConversion(
        decimal quantity,
        UnitType fromUnit,
        UnitType toUnit,
        DensityProfile densityProfile)
    {
        // Volume → Weight conversions
        if (IsVolumeUnit(fromUnit) && IsWeightUnit(toUnit))
        {
            decimal? gramsPerUnit = GetGramsPerUnit(fromUnit, densityProfile);
            if (gramsPerUnit.HasValue)
            {
                var grams = quantity * gramsPerUnit.Value;

                // Convert grams to target weight unit
                if (toUnit == UnitType.Gram)
                    return grams;
                else if (_unitConversionService.CanConvert(UnitType.Gram, toUnit))
                    return _unitConversionService.Convert(grams, UnitType.Gram, toUnit);
            }
        }

        // Weight → Volume conversions
        if (IsWeightUnit(fromUnit) && IsVolumeUnit(toUnit))
        {
            // First convert to grams
            decimal grams = quantity;
            if (fromUnit != UnitType.Gram && _unitConversionService.CanConvert(fromUnit, UnitType.Gram))
                grams = _unitConversionService.Convert(quantity, fromUnit, UnitType.Gram);

            decimal? gramsPerUnit = GetGramsPerUnit(toUnit, densityProfile);
            if (gramsPerUnit.HasValue && gramsPerUnit.Value > 0)
            {
                return grams / gramsPerUnit.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets grams per unit from density profile.
    /// </summary>
    private decimal? GetGramsPerUnit(UnitType unit, DensityProfile profile)
    {
        return unit switch
        {
            UnitType.Cup => profile.GramsPerCup,
            UnitType.Tablespoon => profile.GramsPerTablespoon,
            UnitType.Teaspoon => profile.GramsPerTeaspoon,
            UnitType.FluidOunce => profile.GramsPerFluidOunce,
            _ => null
        };
    }

    /// <summary>
    /// Checks if a unit is a volume unit.
    /// </summary>
    private bool IsVolumeUnit(UnitType unit)
    {
        return unit == UnitType.Cup ||
               unit == UnitType.Tablespoon ||
               unit == UnitType.Teaspoon ||
               unit == UnitType.FluidOunce ||
               unit == UnitType.Gallon ||
               unit == UnitType.Quart ||
               unit == UnitType.Pint ||
               unit == UnitType.Liter ||
               unit == UnitType.Milliliter;
    }

    /// <summary>
    /// Checks if a unit is a weight unit.
    /// </summary>
    private bool IsWeightUnit(UnitType unit)
    {
        return unit == UnitType.Gram ||
               unit == UnitType.Kilogram ||
               unit == UnitType.Ounce ||
               unit == UnitType.Pound;
    }
}
