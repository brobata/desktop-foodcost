namespace Dfc.Core.Models;

/// <summary>
/// Represents calculated nutritional information for a recipe ingredient or entree component
/// </summary>
public class NutritionInfo
{
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }  // in grams
    public decimal Carbohydrates { get; set; }  // in grams
    public decimal Fat { get; set; }  // in grams
    public decimal Fiber { get; set; }  // in grams
    public decimal Sugar { get; set; }  // in grams
    public decimal Sodium { get; set; }  // in milligrams

    /// <summary>
    /// Returns a NutritionInfo with all values set to zero
    /// </summary>
    public static NutritionInfo Zero => new NutritionInfo();

    /// <summary>
    /// Adds two NutritionInfo objects together
    /// </summary>
    public static NutritionInfo operator +(NutritionInfo a, NutritionInfo b)
    {
        return new NutritionInfo
        {
            Calories = a.Calories + b.Calories,
            Protein = a.Protein + b.Protein,
            Carbohydrates = a.Carbohydrates + b.Carbohydrates,
            Fat = a.Fat + b.Fat,
            Fiber = a.Fiber + b.Fiber,
            Sugar = a.Sugar + b.Sugar,
            Sodium = a.Sodium + b.Sodium
        };
    }

    /// <summary>
    /// Multiplies all nutritional values by a scalar
    /// </summary>
    public static NutritionInfo operator *(NutritionInfo info, decimal multiplier)
    {
        return new NutritionInfo
        {
            Calories = info.Calories * multiplier,
            Protein = info.Protein * multiplier,
            Carbohydrates = info.Carbohydrates * multiplier,
            Fat = info.Fat * multiplier,
            Fiber = info.Fiber * multiplier,
            Sugar = info.Sugar * multiplier,
            Sodium = info.Sodium * multiplier
        };
    }

    /// <summary>
    /// Divides all nutritional values by a scalar
    /// </summary>
    public static NutritionInfo operator /(NutritionInfo info, decimal divisor)
    {
        if (divisor == 0) return Zero;

        return new NutritionInfo
        {
            Calories = info.Calories / divisor,
            Protein = info.Protein / divisor,
            Carbohydrates = info.Carbohydrates / divisor,
            Fat = info.Fat / divisor,
            Fiber = info.Fiber / divisor,
            Sugar = info.Sugar / divisor,
            Sodium = info.Sodium / divisor
        };
    }
}
