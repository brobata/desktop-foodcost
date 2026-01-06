using System;
using System.ComponentModel.DataAnnotations;
using Freecost.Core.Enums;

namespace Freecost.Core.Models;

/// <summary>
/// Global vendor import map stored in Firebase
/// Defines how to parse Excel/CSV files from specific vendors
/// </summary>
public class GlobalVendorMap
{
    /// <summary>
    /// Firestore document ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this map (e.g., "Sysco", "US Foods")
    /// </summary>
    [Required]
    public string MapName { get; set; } = string.Empty;

    /// <summary>
    /// Vendor name (used to set VendorName on ingredients)
    /// </summary>
    public string? VendorName { get; set; }

    /// <summary>
    /// Comma-separated list of column headers that identify this vendor's format
    /// Example: "SUPC,Case $,Split $" for Sysco
    /// </summary>
    [Required]
    public string DetectionPattern { get; set; } = string.Empty;

    /// <summary>
    /// CSV delimiter (usually ",")
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Which row contains the column headers (1-based index)
    /// </summary>
    public int HeaderRow { get; set; } = 1;

    /// <summary>
    /// Column name containing ingredient name
    /// </summary>
    public string? NameColumn { get; set; }

    /// <summary>
    /// Column name containing brand
    /// </summary>
    public string? BrandColumn { get; set; }

    /// <summary>
    /// Column name containing price
    /// </summary>
    public string? PriceColumn { get; set; }

    /// <summary>
    /// Column name containing SKU/product code
    /// </summary>
    public string? SkuColumn { get; set; }

    /// <summary>
    /// Column name containing category
    /// </summary>
    public string? CategoryColumn { get; set; }

    /// <summary>
    /// How to parse quantity (Combined, Separate, BrothersProduceThreeRow)
    /// Stored as string, mapped to enum
    /// </summary>
    public string ParseMode { get; set; } = "Separate";

    /// <summary>
    /// Column containing pack quantity (if ParseMode = Separate)
    /// </summary>
    public string? PackColumn { get; set; }

    /// <summary>
    /// Column or literal value for size (if ParseMode = Separate)
    /// </summary>
    public string? SizeColumn { get; set; }

    /// <summary>
    /// Column containing unit (if ParseMode = Separate)
    /// </summary>
    public string? UnitColumn { get; set; }

    /// <summary>
    /// Column containing combined quantity (if ParseMode = Combined)
    /// Example: "12/32oz" for US Foods
    /// </summary>
    public string? CombinedQuantityColumn { get; set; }

    /// <summary>
    /// Character to split combined quantity (if ParseMode = Combined)
    /// Example: "/" for "12/32oz"
    /// </summary>
    public string? SplitCharacter { get; set; }

    /// <summary>
    /// Column indicating if price is per unit vs per case
    /// Example: Sysco's "Per Lb" column (Y/N)
    /// </summary>
    public string? PriceIsPerUnitColumn { get; set; }

    /// <summary>
    /// Column containing vendor name (overrides VendorName if present)
    /// </summary>
    public string? VendorColumn { get; set; }

    /// <summary>
    /// Is this a system/built-in map (vs user-created)
    /// </summary>
    public bool IsSystemMap { get; set; } = true;

    /// <summary>
    /// When this map was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this map was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Who created/manages this mapping (admin username or email)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Optional notes about this vendor map
    /// </summary>
    public string? Notes { get; set; }
}
