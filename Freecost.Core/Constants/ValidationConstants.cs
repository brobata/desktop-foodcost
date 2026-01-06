namespace Freecost.Core.Constants;

/// <summary>
/// Constants for validation thresholds and limits across the application
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Name validation limits
    /// </summary>
    public static class NameLimits
    {
        public const int MIN_LENGTH = 2;
        public const int MAX_LENGTH = 200;
    }

    /// <summary>
    /// Recipe yield validation limits
    /// </summary>
    public static class YieldLimits
    {
        public const int MAX_QUANTITY = 10000;
        public const int HIGH_YIELD_WARNING_THRESHOLD = 100;
    }

    /// <summary>
    /// Price validation limits
    /// </summary>
    public static class PriceLimits
    {
        public const decimal MAX_INGREDIENT_PRICE = 10000m;
        public const decimal MAX_MENU_PRICE = 200m;
    }

    /// <summary>
    /// Quantity validation limits
    /// </summary>
    public static class QuantityLimits
    {
        public const decimal MAX_CASE_QUANTITY = 10000m;
    }

    /// <summary>
    /// Time validation limits (in minutes)
    /// </summary>
    public static class TimeLimits
    {
        public const int MAX_PREP_TIME_MINUTES = 1440; // 24 hours
    }

    /// <summary>
    /// Nutrition validation thresholds (per serving)
    /// </summary>
    public static class NutritionLimits
    {
        public const decimal CALORIES_EXTREME_HIGH = 5000m;
        public const decimal CALORIES_HIGH = 2000m;
        public const decimal PROTEIN_HIGH = 200m;
        public const decimal SODIUM_DAILY_LIMIT = 2300m;
        public const decimal SUGAR_HIGH = 50m;
    }

    /// <summary>
    /// Food cost percentage thresholds
    /// </summary>
    public static class FoodCostLimits
    {
        public const decimal HIGH_THRESHOLD_PERCENT = 40m;
        public const decimal LOW_THRESHOLD_PERCENT = 15m;
        public const decimal INDUSTRY_MIN_PERCENT = 25m;
        public const decimal INDUSTRY_MAX_PERCENT = 35m;
    }

    /// <summary>
    /// Content length validation
    /// </summary>
    public static class ContentLimits
    {
        public const int MIN_INSTRUCTION_LENGTH = 10;
        public const int MIN_DESCRIPTION_LENGTH = 10;
    }

    /// <summary>
    /// Unit conversion validation
    /// </summary>
    public static class ConversionLimits
    {
        public const decimal MIN_RATIO = 0.001m;
        public const decimal MAX_RATIO = 10000m;
    }
}
