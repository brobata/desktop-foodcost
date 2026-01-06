namespace Dfc.Core.Models;

/// <summary>
/// Maps common food names to their known allergens.
/// Used for enhanced allergen detection beyond simple keyword matching.
/// Stored in Firebase: global_food_allergens collection
/// </summary>
public class GlobalFoodAllergen
{
    /// <summary>
    /// Unique identifier (auto-generated)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Normalized food name (lowercase, trimmed)
    /// Example: "worcestershire sauce", "mayonnaise", "pesto"
    /// </summary>
    public string FoodName { get; set; } = string.Empty;

    /// <summary>
    /// List of allergen types this food contains
    /// Stored as string array in Firebase (e.g., ["Fish", "Soybeans"])
    /// </summary>
    public List<string> AllergenTypes { get; set; } = new();

    /// <summary>
    /// Optional notes explaining why these allergens are present
    /// Example: "Contains anchovies (fish) and soy sauce"
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this mapping was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this mapping was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Who created this mapping (admin email or "System")
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Whether this is a system-provided mapping or user-contributed
    /// </summary>
    public bool IsSystemMapping { get; set; } = true;

    /// <summary>
    /// Alternate names for this food (for better matching)
    /// Example: ["mayo", "mayonaise"] for "mayonnaise"
    /// </summary>
    public List<string>? AlternateNames { get; set; }
}
