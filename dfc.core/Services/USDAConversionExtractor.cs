using Dfc.Core.Enums;
using Dfc.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dfc.Core.Services;

/// <summary>
/// Extracts unit conversions from USDA serving size data.
/// Maps serving size descriptions (e.g., "1 cup", "1 medium") to IngredientConversion objects.
/// </summary>
public class USDAConversionExtractor
{
    private readonly ILogger<USDAConversionExtractor>? _logger;
    private readonly UnitConversionService _unitConverter;

    // Patterns for matching serving size descriptions to unit types
    private static readonly Dictionary<UnitType, List<string>> UnitPatterns = new()
    {
        { UnitType.Cup, new() { "cup", "cups", "c." } },
        { UnitType.Tablespoon, new() { "tablespoon", "tablespoons", "tbsp", "tbs", "tb" } },
        { UnitType.Teaspoon, new() { "teaspoon", "teaspoons", "tsp", "ts" } },
        { UnitType.FluidOunce, new() { "fluid ounce", "fluid ounces", "fl oz", "fl. oz" } },
        { UnitType.Ounce, new() { "ounce", "ounces", "oz" } },
        { UnitType.Pound, new() { "pound", "pounds", "lb", "lbs" } },
        { UnitType.Gram, new() { "gram", "grams", "g" } },
        { UnitType.Kilogram, new() { "kilogram", "kilograms", "kg" } },
        { UnitType.Each, new() { "each", "whole", "medium", "large", "small", "piece", "item", "unit" } }
    };

    public USDAConversionExtractor(
        UnitConversionService unitConverter,
        ILogger<USDAConversionExtractor>? logger = null)
    {
        _unitConverter = unitConverter;
        _logger = logger;
    }

    /// <summary>
    /// Extracts conversions from USDA nutritional data result.
    /// Returns list of IngredientConversion objects ready to be saved.
    /// </summary>
    public List<IngredientConversion> ExtractConversions(
        NutritionalDataResult usdaResult,
        Guid? ingredientId = null,
        Guid? locationId = null)
    {
        var conversions = new List<IngredientConversion>();

        if (usdaResult.ServingSizes == null || !usdaResult.ServingSizes.Any())
        {
            _logger?.LogDebug("No serving sizes found in USDA result for {Description}", usdaResult.Description);
            return conversions;
        }

        _logger?.LogDebug("Extracting conversions from {Count} serving sizes for {Description}",
            usdaResult.ServingSizes.Count,
            usdaResult.Description);

        foreach (var serving in usdaResult.ServingSizes)
        {
            var conversion = ExtractConversionFromServingSize(serving, usdaResult.FdcId, ingredientId, locationId);
            if (conversion != null)
            {
                conversions.Add(conversion);
                _logger?.LogDebug("Extracted conversion: {DisplayText}", conversion.DisplayText);
            }
        }

        _logger?.LogInformation("Extracted {Count} conversions from USDA data for {Description}",
            conversions.Count,
            usdaResult.Description);

        return conversions;
    }

    /// <summary>
    /// Converts a single USDA serving size to an IngredientConversion.
    /// </summary>
    private IngredientConversion? ExtractConversionFromServingSize(
        ServingSize serving,
        string usdaFdcId,
        Guid? ingredientId,
        Guid? locationId)
    {
        // Parse the serving description to extract quantity and unit
        var parsed = ParseServingDescription(serving.Description);
        if (parsed == null)
        {
            _logger?.LogDebug("Could not parse serving description: {Description}", serving.Description);
            return null;
        }

        var (quantity, unit) = parsed.Value;

        // Create conversion: fromQuantity [unit] = serving.Grams [Gram]
        return new IngredientConversion
        {
            IngredientId = ingredientId,
            LocationId = locationId,
            FromQuantity = quantity,
            FromUnit = unit,
            ToQuantity = serving.Grams,
            ToUnit = UnitType.Gram,
            Source = "USDA",
            UsdaFdcId = usdaFdcId,
            Notes = $"From USDA serving size: {serving.Description}",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Parses a serving description and extracts quantity and unit type.
    /// Examples:
    /// - "1 cup" → (1.0, Cup)
    /// - "2 tablespoons" → (2.0, Tablespoon)
    /// - "1 medium" → (1.0, Each)
    /// - "3 oz" → (3.0, Ounce)
    /// </summary>
    private (decimal Quantity, UnitType Unit)? ParseServingDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var desc = description.ToLowerInvariant().Trim();

        // Try to extract a leading number (quantity)
        // Pattern: optional number (can be decimal like "0.5" or fraction like "1/2"), followed by unit
        var numberMatch = Regex.Match(desc, @"^(\d+\.?\d*|\d+/\d+)");
        decimal quantity = 1.0m;

        if (numberMatch.Success)
        {
            var numberStr = numberMatch.Value;

            // Handle fractions (e.g., "1/2")
            if (numberStr.Contains('/'))
            {
                var parts = numberStr.Split('/');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out var numerator) &&
                    decimal.TryParse(parts[1], out var denominator) &&
                    denominator != 0)
                {
                    quantity = numerator / denominator;
                }
            }
            // Handle decimals
            else if (decimal.TryParse(numberStr, out var parsed))
            {
                quantity = parsed;
            }

            // Remove the number from the description for unit matching
            desc = desc.Substring(numberMatch.Length).Trim();
        }

        // Try to match the remaining text to a unit type
        foreach (var (unitType, patterns) in UnitPatterns)
        {
            foreach (var pattern in patterns)
            {
                // Check if description contains this pattern
                if (desc.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    // Special handling for ounce vs fluid ounce
                    if (unitType == UnitType.Ounce && desc.Contains("fl"))
                    {
                        continue; // Skip regular ounce if "fl" is present (fluid ounce)
                    }

                    if (unitType == UnitType.FluidOunce && !desc.Contains("fl"))
                    {
                        continue; // Skip fluid ounce if "fl" is not present
                    }

                    return (quantity, unitType);
                }
            }
        }

        // No match found
        _logger?.LogDebug("Could not match unit in description: {Description}", description);
        return null;
    }

    /// <summary>
    /// Filters conversions to avoid duplicates and select the most useful ones.
    /// Prefers "Each" conversions and removes redundant weight-to-weight conversions.
    /// </summary>
    public List<IngredientConversion> FilterOptimalConversions(List<IngredientConversion> conversions)
    {
        if (!conversions.Any())
            return conversions;

        var filtered = new List<IngredientConversion>();

        // Group by FromUnit
        var grouped = conversions.GroupBy(c => c.FromUnit);

        foreach (var group in grouped)
        {
            // For Each unit, prefer the "medium" or "whole" serving (already marked as IsPreferred in USDA data)
            // Take all "Each" conversions since they're valuable (small, medium, large sizes)
            if (group.Key == UnitType.Each)
            {
                filtered.AddRange(group);
            }
            // For volume/weight units, take the first one (they should all be similar)
            else
            {
                filtered.Add(group.First());
            }
        }

        // Remove conversions that are Gram → Gram (redundant)
        filtered = filtered.Where(c => !(c.FromUnit == UnitType.Gram && c.ToUnit == UnitType.Gram)).ToList();

        // Remove conversions where FromQuantity is very small (< 0.01) - likely parsing errors
        filtered = filtered.Where(c => c.FromQuantity >= 0.01m).ToList();

        _logger?.LogDebug("Filtered {OriginalCount} conversions down to {FilteredCount} optimal ones",
            conversions.Count,
            filtered.Count);

        return filtered;
    }

    /// <summary>
    /// Checks if a conversion from USDA would be useful.
    /// Returns false for redundant conversions (e.g., weight-to-weight that UnitConversionService already handles).
    /// </summary>
    public bool IsUsefulConversion(IngredientConversion conversion)
    {
        // Always useful if it's an Each conversion (count-to-weight)
        if (conversion.FromUnit == UnitType.Each)
            return true;

        // Check if UnitConversionService can already handle this
        if (_unitConverter.CanConvert(conversion.FromUnit, conversion.ToUnit))
        {
            // This conversion is already handled by the basic unit converter
            _logger?.LogDebug("Skipping redundant conversion: {DisplayText} (already handled by UnitConversionService)",
                conversion.DisplayText);
            return false;
        }

        // Volume-to-weight conversions are useful (e.g., Cup → Gram)
        return true;
    }

    /// <summary>
    /// Validates that a conversion makes sense (no absurd values).
    /// </summary>
    public bool ValidateConversion(IngredientConversion conversion)
    {
        // Check for valid quantities
        if (conversion.FromQuantity <= 0 || conversion.ToQuantity <= 0)
            return false;

        // Check for reasonable ratios (catch parsing errors)
        var ratio = conversion.ToQuantity / conversion.FromQuantity;

        // Ratio should be between 0.01 and 10000 (catches absurd conversions like 1 cup = 0.001g or 1 cup = 1000000g)
        if (ratio < 0.01m || ratio > 10000m)
        {
            _logger?.LogWarning("Conversion has suspicious ratio: {DisplayText} (ratio: {Ratio:F2})",
                conversion.DisplayText,
                ratio);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts and filters optimal conversions from USDA data.
    /// This is the main public method to use.
    /// </summary>
    public List<IngredientConversion> ExtractOptimalConversions(
        NutritionalDataResult usdaResult,
        Guid? ingredientId = null,
        Guid? locationId = null)
    {
        // Step 1: Extract all possible conversions
        var allConversions = ExtractConversions(usdaResult, ingredientId, locationId);

        // Step 2: Filter to keep only useful conversions
        var usefulConversions = allConversions.Where(IsUsefulConversion).ToList();

        // Step 3: Validate conversions
        var validConversions = usefulConversions.Where(ValidateConversion).ToList();

        // Step 4: Filter to optimal set (remove duplicates, prefer Each)
        var optimalConversions = FilterOptimalConversions(validConversions);

        return optimalConversions;
    }
}
