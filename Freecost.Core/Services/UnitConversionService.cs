using Freecost.Core.Enums;

namespace Freecost.Core.Services;

public class UnitConversionService
{
    // Base unit conversions to fluid ounces for volume
    private static readonly Dictionary<UnitType, decimal> VolumeToFluidOunces = new()
    {
        { UnitType.Teaspoon, 0.166667m },
        { UnitType.Tablespoon, 0.5m },
        { UnitType.FluidOunce, 1m },
        { UnitType.Cup, 8m },
        { UnitType.Pint, 16m },
        { UnitType.Quart, 32m },
        { UnitType.Gallon, 128m },
        { UnitType.Milliliter, 0.033814m },
        { UnitType.Liter, 33.814m }
    };

    // Base unit conversions to ounces for weight
    private static readonly Dictionary<UnitType, decimal> WeightToOunces = new()
    {
        { UnitType.Ounce, 1m },
        { UnitType.Pound, 16m },
        { UnitType.Gram, 0.035274m },
        { UnitType.Kilogram, 35.274m }
    };

    public bool CanConvert(UnitType from, UnitType to)
    {
        if (from == to) return true;

        // Handle "Each" and "Dozen" - can only convert between these two
        if (from == UnitType.Each || to == UnitType.Each ||
            from == UnitType.Dozen || to == UnitType.Dozen)
        {
            return (from == UnitType.Each && to == UnitType.Dozen) ||
                   (from == UnitType.Dozen && to == UnitType.Each);
        }

        // Volume to volume or weight to weight
        return (VolumeToFluidOunces.ContainsKey(from) && VolumeToFluidOunces.ContainsKey(to)) ||
               (WeightToOunces.ContainsKey(from) && WeightToOunces.ContainsKey(to));
    }

    public decimal Convert(decimal quantity, UnitType from, UnitType to)
    {
        if (from == to) return quantity;

        // Handle "Each" and "Dozen" - no conversion except between these two
        if (from == UnitType.Each && to == UnitType.Each) return quantity;
        if (from == UnitType.Dozen && to == UnitType.Dozen) return quantity;
        if (from == UnitType.Each && to == UnitType.Dozen) return quantity / 12m;
        if (from == UnitType.Dozen && to == UnitType.Each) return quantity * 12m;

        // Can't convert count units to/from other types
        if (from == UnitType.Each || to == UnitType.Each ||
            from == UnitType.Dozen || to == UnitType.Dozen)
        {
            return quantity; // Return original quantity if no conversion possible
        }

        // Volume conversions
        if (VolumeToFluidOunces.ContainsKey(from) && VolumeToFluidOunces.ContainsKey(to))
        {
            decimal baseQuantity = quantity * VolumeToFluidOunces[from];
            return baseQuantity / VolumeToFluidOunces[to];
        }

        // Weight conversions
        if (WeightToOunces.ContainsKey(from) && WeightToOunces.ContainsKey(to))
        {
            decimal baseQuantity = quantity * WeightToOunces[from];
            return baseQuantity / WeightToOunces[to];
        }

        // Can't convert between volume and weight
        return quantity;
    }

    public string GetUnitAbbreviation(UnitType unit)
    {
        return unit switch
        {
            UnitType.Teaspoon => "tsp",
            UnitType.Tablespoon => "tbsp",
            UnitType.FluidOunce => "fl oz",
            UnitType.Cup => "cup",
            UnitType.Pint => "pt",
            UnitType.Quart => "qt",
            UnitType.Gallon => "gal",
            UnitType.Ounce => "oz",
            UnitType.Pound => "lb",
            UnitType.Milliliter => "ml",
            UnitType.Liter => "L",
            UnitType.Gram => "g",
            UnitType.Kilogram => "kg",
            UnitType.Each => "ea",
            UnitType.Dozen => "doz",
            _ => unit.ToString()
        };
    }

    public bool IsVolumeUnit(UnitType unit)
    {
        return VolumeToFluidOunces.ContainsKey(unit);
    }

    public bool IsWeightUnit(UnitType unit)
    {
        return WeightToOunces.ContainsKey(unit);
    }

    public bool IsCountUnit(UnitType unit)
    {
        return unit == UnitType.Each || unit == UnitType.Dozen;
    }
}