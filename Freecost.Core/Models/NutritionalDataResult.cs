using Freecost.Core.Enums;
using System;
using System.Collections.Generic;

namespace Freecost.Core.Models;

/// <summary>
/// Result from nutritional data API lookup (USDA FoodData Central)
/// </summary>
public class NutritionalDataResult
{
    /// <summary>
    /// USDA FDC ID for this food item
    /// </summary>
    public string FdcId { get; set; } = string.Empty;

    /// <summary>
    /// Food name from USDA database
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Food category (e.g., "Vegetables and Vegetable Products")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Nutritional data per 100g
    /// </summary>
    public NutritionalInfo? NutritionPer100g { get; set; }

    /// <summary>
    /// Common serving sizes with their weights
    /// </summary>
    public List<ServingSize> ServingSizes { get; set; } = new();

    /// <summary>
    /// Detected allergens based on ingredient analysis
    /// </summary>
    public List<AllergenType> DetectedAllergens { get; set; } = new();

    /// <summary>
    /// Match confidence score (0-100)
    /// </summary>
    public int MatchScore { get; set; }
}

/// <summary>
/// Nutritional information per 100g
/// </summary>
public class NutritionalInfo
{
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fat { get; set; }
    public decimal Fiber { get; set; }
    public decimal Sugar { get; set; }
    public decimal Sodium { get; set; }
}

/// <summary>
/// Common serving size with weight
/// </summary>
public class ServingSize
{
    /// <summary>
    /// Description (e.g., "1 medium", "1 cup chopped")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Weight in grams
    /// </summary>
    public decimal Grams { get; set; }

    /// <summary>
    /// Whether this is the preferred serving size for this food
    /// </summary>
    public bool IsPreferred { get; set; }
}
