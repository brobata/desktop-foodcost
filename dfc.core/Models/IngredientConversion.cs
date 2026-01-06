using Dfc.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfc.Core.Models;

/// <summary>
/// Represents a unit conversion for an ingredient (e.g., "1 Each = 8 Ounce").
/// Can be ingredient-specific or generic (for categories like "flour" or "eggs").
/// </summary>
public class IngredientConversion : BaseEntity
{
    /// <summary>
    /// The specific ingredient this conversion applies to.
    /// NULL = generic conversion (applies to ingredient category/type)
    /// </summary>
    public Guid? IngredientId { get; set; }

    /// <summary>
    /// Location-specific override.
    /// NULL = global conversion
    /// NOT NULL = only applies to this location
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// The quantity in the "from" unit (e.g., 1.0 for "1 Each")
    /// </summary>
    [Required]
    [Range(0.0001, 999999.9999)]
    public decimal FromQuantity { get; set; }

    /// <summary>
    /// The unit to convert from (e.g., Each, Cup, Tablespoon)
    /// </summary>
    [Required]
    public UnitType FromUnit { get; set; }

    /// <summary>
    /// The converted quantity (e.g., 8.0 for "8 Ounce")
    /// </summary>
    [Required]
    [Range(0.0001, 999999.9999)]
    public decimal ToQuantity { get; set; }

    /// <summary>
    /// The unit to convert to (e.g., Ounce, Gram, Pound)
    /// </summary>
    [Required]
    public UnitType ToUnit { get; set; }

    /// <summary>
    /// Source of this conversion
    /// - BuiltIn: From StandardConversions.json
    /// - USDA: Extracted from FoodData Central serving sizes
    /// - UserDefined: Manually entered by user
    /// - Migrated: Converted from old alternate_unit system
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Source { get; set; } = "UserDefined";

    /// <summary>
    /// Optional notes explaining the conversion
    /// (e.g., "Average chicken breast size", "USDA standard serving")
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// USDA FoodData Central ID if this conversion came from USDA
    /// Used for traceability and cache updates
    /// </summary>
    [MaxLength(50)]
    public string? UsdaFdcId { get; set; }

    // Navigation properties
    public virtual Ingredient? Ingredient { get; set; }
    public virtual Location? Location { get; set; }

    /// <summary>
    /// Applies this conversion to a quantity.
    /// Example: Convert 2.5 units using conversion "1 Each = 8 oz" → 20 oz
    /// </summary>
    public decimal ApplyConversion(decimal quantity)
    {
        // Convert: quantity * (ToQuantity / FromQuantity)
        // Example: 2.5 Each * (8 oz / 1 Each) = 20 oz
        return quantity * (ToQuantity / FromQuantity);
    }

    /// <summary>
    /// Returns a human-readable display of this conversion
    /// Example: "1 Each = 8.0 Ounce"
    /// </summary>
    [NotMapped]
    public string DisplayText => $"{FromQuantity} {FromUnit} = {ToQuantity} {ToUnit}";

    /// <summary>
    /// Returns a badge-style display of the source
    /// Example: "USDA", "Built-In", "Custom"
    /// </summary>
    [NotMapped]
    public string SourceBadge => Source switch
    {
        "BuiltIn" => "Built-In",
        "USDA" => "USDA",
        "UserDefined" => "Custom",
        "Migrated" => "Migrated",
        _ => Source
    };

    /// <summary>
    /// Checks if this conversion can be applied to convert from the specified unit to the target unit
    /// </summary>
    public bool CanConvert(UnitType fromUnit, UnitType toUnit)
    {
        return FromUnit == fromUnit && ToUnit == toUnit;
    }

    /// <summary>
    /// Validates that the conversion is logically correct
    /// </summary>
    public bool IsValid()
    {
        return FromQuantity > 0 &&
               ToQuantity > 0 &&
               FromUnit != ToUnit &&
               !string.IsNullOrWhiteSpace(Source);
    }
}
