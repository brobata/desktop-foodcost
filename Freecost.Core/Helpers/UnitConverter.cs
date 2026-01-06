// Location: Freecost.Core/Helpers/UnitConverter.cs
// Action: REPLACE entire file

using Freecost.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Core.Helpers;

public static class UnitConverter
{
    private static readonly Dictionary<UnitType, decimal> VolumeConversions = new()
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

    private static readonly Dictionary<UnitType, decimal> WeightConversions = new()
    {
        { UnitType.Ounce, 1m },
        { UnitType.Pound, 16m },
        { UnitType.Gram, 0.035274m },
        { UnitType.Kilogram, 35.274m }
    };

    public static bool IsVolumeUnit(UnitType unit)
    {
        return VolumeConversions.ContainsKey(unit);
    }

    public static bool IsWeightUnit(UnitType unit)
    {
        return WeightConversions.ContainsKey(unit);
    }

    public static bool IsCountUnit(UnitType unit)
    {
        return unit == UnitType.Each || unit == UnitType.Count || unit == UnitType.Dozen;
    }

    public static string GetUnitCategory(UnitType unit)
    {
        if (IsWeightUnit(unit)) return "Weight";
        if (IsVolumeUnit(unit)) return "Volume";
        if (IsCountUnit(unit)) return "Count";
        return "Unknown";
    }

    public static bool CanConvert(UnitType from, UnitType to)
    {
        if (from == to) return true;

        if (IsCountUnit(from) && IsCountUnit(to))
        {
            return true;
        }

        return (IsVolumeUnit(from) && IsVolumeUnit(to)) ||
               (IsWeightUnit(from) && IsWeightUnit(to));
    }

    public static decimal Convert(decimal quantity, UnitType from, UnitType to)
    {
        if (from == to) return quantity;
        if (!CanConvert(from, to))
            throw new System.InvalidOperationException($"Cannot convert from {from} ({GetUnitCategory(from)}) to {to} ({GetUnitCategory(to)})");

        // Handle count unit conversions (Each, Count, Dozen)
        if (IsCountUnit(from) && IsCountUnit(to))
        {
            // Count and Each are equivalent
            if ((from == UnitType.Count || from == UnitType.Each) && (to == UnitType.Count || to == UnitType.Each))
                return quantity;

            if ((from == UnitType.Each || from == UnitType.Count) && to == UnitType.Dozen)
                return quantity / 12m;

            if (from == UnitType.Dozen && (to == UnitType.Each || to == UnitType.Count))
                return quantity * 12m;
        }

        if (IsVolumeUnit(from) && IsVolumeUnit(to))
        {
            decimal baseQuantity = quantity * VolumeConversions[from];
            return baseQuantity / VolumeConversions[to];
        }

        if (IsWeightUnit(from) && IsWeightUnit(to))
        {
            decimal baseQuantity = quantity * WeightConversions[from];
            return baseQuantity / WeightConversions[to];
        }

        throw new System.InvalidOperationException($"Cannot convert from {from} to {to}");
    }

    public static string GetAbbreviation(UnitType unit)
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
            UnitType.Count => "ct",
            UnitType.Dozen => "doz",
            _ => unit.ToString()
        };
    }

    public static List<UnitType> GetUnitsInSameCategory(UnitType unit)
    {
        if (IsWeightUnit(unit))
        {
            return WeightConversions.Keys.ToList();
        }

        if (IsVolumeUnit(unit))
        {
            return VolumeConversions.Keys.ToList();
        }

        if (IsCountUnit(unit))
        {
            return new List<UnitType> { UnitType.Each, UnitType.Count, UnitType.Dozen };
        }

        return new List<UnitType> { unit };
    }
}