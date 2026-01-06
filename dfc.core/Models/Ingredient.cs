// Location: Dfc.Core/Models/Ingredient.cs
// Action: ADD these 4 properties to your existing Ingredient class

using Dfc.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dfc.Core.Models;

public class Ingredient : BaseEntity
{
    /// <summary>
    /// Ingredient name (e.g., "Chicken Breast, Boneless")
    /// Max 200 characters to prevent DoS attacks
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public Guid LocationId { get; set; }

    // Existing properties
    public decimal CurrentPrice { get; set; }
    public UnitType Unit { get; set; }

    /// <summary>
    /// Total quantity in a case (e.g., 48 for "6/8 CT")
    /// Used to recalculate unit price when case price changes
    /// </summary>
    public decimal CaseQuantity { get; set; } = 1;

    [MaxLength(200)]
    public string? VendorName { get; set; }

    [MaxLength(100)]
    public string? VendorSku { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Hex color code for this category (e.g., "#E91E63")
    /// Persisted to ensure consistent category colors
    /// </summary>
    [MaxLength(7)] // #RRGGBB format
    public string? CategoryColor { get; set; }

    // ========================================
    // ADD THESE 4 NEW PROPERTIES:
    // ========================================

    /// <summary>
    /// When true, recipes can use a different unit than the purchase unit
    /// </summary>
    public bool UseAlternateUnit { get; set; }

    /// <summary>
    /// The unit used in recipes (e.g., Each, Cup)
    /// </summary>
    public UnitType? AlternateUnit { get; set; }

    /// <summary>
    /// Conversion quantity (e.g., 1.0 for "1 ea = 1 oz")
    /// </summary>
    public decimal? AlternateConversionQuantity { get; set; }

    /// <summary>
    /// The unit in the conversion (e.g., Ounce for "1 ea = 1 oz")
    /// </summary>
    public UnitType? AlternateConversionUnit { get; set; }

    // ========================================
    // NUTRITION INFORMATION (per unit)
    // ========================================

    /// <summary>
    /// Calories per purchase unit (e.g., per lb, per oz, per case)
    /// </summary>
    public decimal? CaloriesPerUnit { get; set; }

    /// <summary>
    /// Protein in grams per purchase unit
    /// </summary>
    public decimal? ProteinPerUnit { get; set; }

    /// <summary>
    /// Carbohydrates in grams per purchase unit
    /// </summary>
    public decimal? CarbohydratesPerUnit { get; set; }

    /// <summary>
    /// Fat in grams per purchase unit
    /// </summary>
    public decimal? FatPerUnit { get; set; }

    /// <summary>
    /// Fiber in grams per purchase unit
    /// </summary>
    public decimal? FiberPerUnit { get; set; }

    /// <summary>
    /// Sugar in grams per purchase unit
    /// </summary>
    public decimal? SugarPerUnit { get; set; }

    /// <summary>
    /// Sodium in milligrams per purchase unit
    /// </summary>
    public decimal? SodiumPerUnit { get; set; }


    // ========================================
    // UNIVERSAL CONVERSION SYSTEM FIELDS
    // ========================================

    /// <summary>
    /// Enable automatic conversion lookup from USDA and built-in database.
    /// When true, system will search for conversions when ingredient is saved.
    /// </summary>
    public bool AutoConversionEnabled { get; set; } = true;

    /// <summary>
    /// Timestamp when conversions were last auto-detected for this ingredient.
    /// Used to determine if re-detection is needed (e.g., after 6 months).
    /// </summary>
    public DateTime? ConversionLastUpdated { get; set; }

    /// <summary>
    /// Last source that provided conversions for this ingredient.
    /// - USDA: From FoodData Central serving sizes
    /// - BuiltIn: From StandardConversions.json
    /// - Manual: User manually configured conversions
    /// </summary>
    [MaxLength(50)]
    public string? ConversionSource { get; set; }

    // Navigation properties
    public Location Location { get; set; } = null!;
    public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
    public List<IngredientAlias> Aliases { get; set; } = new();
    public List<PriceHistory> PriceHistory { get; set; } = new();
    public List<EntreeIngredient> EntreeIngredients { get; set; } = new();
    public List<IngredientAllergen> IngredientAllergens { get; set; } = new();
}