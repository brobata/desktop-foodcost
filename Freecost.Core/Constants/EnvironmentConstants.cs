namespace Freecost.Core.Constants;

/// <summary>
/// Constants for environment variable names used throughout the application
/// </summary>
public static class EnvironmentConstants
{
    /// <summary>
    /// Supabase URL environment variable
    /// </summary>
    public const string SUPABASE_URL = "SUPABASE_URL";

    /// <summary>
    /// Supabase anonymous key environment variable
    /// </summary>
    public const string SUPABASE_ANON_KEY = "SUPABASE_ANON_KEY";

    /// <summary>
    /// USDA API key environment variable (for nutritional data lookups)
    /// </summary>
    public const string USDA_API_KEY = "FREECOST_USDA_API_KEY";
}
