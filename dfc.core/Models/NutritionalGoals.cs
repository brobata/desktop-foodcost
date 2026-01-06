namespace Dfc.Core.Models;

/// <summary>
/// User-defined nutritional goals/targets (typically daily or per-serving goals)
/// </summary>
public class NutritionalGoals
{
    public decimal? TargetCalories { get; set; }
    public decimal? TargetProtein { get; set; } // grams
    public decimal? TargetCarbohydrates { get; set; } // grams
    public decimal? TargetFat { get; set; } // grams
    public decimal? TargetFiber { get; set; } // grams
    public decimal? TargetSugar { get; set; } // grams
    public decimal? TargetSodium { get; set; } // milligrams

    public bool HasAnyGoals => TargetCalories.HasValue || TargetProtein.HasValue ||
                                TargetCarbohydrates.HasValue || TargetFat.HasValue ||
                                TargetFiber.HasValue || TargetSugar.HasValue ||
                                TargetSodium.HasValue;
}

/// <summary>
/// Comparison result showing how a recipe matches against nutritional goals
/// </summary>
public class NutritionalGoalComparison
{
    public string Nutrient { get; set; } = string.Empty;
    public decimal? ActualValue { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? PercentOfGoal { get; set; }
    public ComplianceLevel Compliance { get; set; }
}

public enum ComplianceLevel
{
    Under,      // <50% of goal
    Good,       // 50-100% of goal
    Meets,      // 100-120% of goal
    Exceeds     // >120% of goal
}
