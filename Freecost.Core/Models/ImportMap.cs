using System;
using System.Collections.Generic;

namespace Freecost.Core.Models;

/// <summary>
/// Defines how to map vendor CSV/Excel columns to Freecost ingredient fields
/// </summary>
public class ImportMap : BaseEntity
{
    public string MapName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Detection patterns - if file headers contain these, auto-select this map
    /// Examples: "SUPC,Brand,Desc" for Sysco
    /// </summary>
    public string DetectionPattern { get; set; } = string.Empty;

    /// <summary>
    /// CSV delimiter (comma, tab, pipe, etc.)
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Which row contains headers (usually 1)
    /// </summary>
    public int HeaderRow { get; set; } = 1;

    // Field mappings - column name to Freecost field
    public string? NameColumn { get; set; }
    public string? BrandColumn { get; set; }
    public string? PriceColumn { get; set; }
    public string? SkuColumn { get; set; }
    public string? VendorColumn { get; set; }
    public string? CategoryColumn { get; set; }

    // Quantity parsing configuration
    public QuantityParseMode ParseMode { get; set; } = QuantityParseMode.Combined;

    /// <summary>
    /// For Combined mode: column containing "Pack/Size Unit" (e.g., "6/5 LB")
    /// </summary>
    public string? CombinedQuantityColumn { get; set; }

    /// <summary>
    /// Character that splits pack from size in combined format (usually "/")
    /// </summary>
    public string SplitCharacter { get; set; } = "/";

    /// <summary>
    /// For Separate mode: Pack column (e.g., "6")
    /// </summary>
    public string? PackColumn { get; set; }

    /// <summary>
    /// For Separate mode: Size column (e.g., "5 LB")
    /// </summary>
    public string? SizeColumn { get; set; }

    /// <summary>
    /// For Separate mode: Optional unit column if unit is in separate column
    /// </summary>
    public string? UnitColumn { get; set; }

    /// <summary>
    /// Optional column that indicates if price is already per unit (e.g., "Per Lb" = Y/N)
    /// If Y, price won't be divided by quantity. If N or empty, price is for entire case.
    /// </summary>
    public string? PriceIsPerUnitColumn { get; set; }

    public bool IsSystemMap { get; set; } // Built-in maps can't be deleted
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;
}

public enum QuantityParseMode
{
    /// <summary>
    /// Pack and size in one column: "6/5 LB" or "2/10 LB"
    /// </summary>
    Combined,

    /// <summary>
    /// Pack, size, and optionally unit in separate columns
    /// </summary>
    Separate,

    /// <summary>
    /// Brothers Produce 3-row format:
    /// Row N: SKU (col A), Price (col C)
    /// Row N+1: Name/Qty/UOM (col B)
    /// Row N+2: Empty
    /// </summary>
    BrothersProduceThreeRow
}
