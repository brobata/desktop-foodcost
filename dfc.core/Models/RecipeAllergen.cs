namespace Dfc.Core.Models;

public class RecipeAllergen : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Guid AllergenId { get; set; }

    /// <summary>
    /// True if this allergen was automatically detected from ingredients
    /// </summary>
    public bool IsAutoDetected { get; set; }

    /// <summary>
    /// True if this allergen should appear on recipe cards (can be disabled by user)
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Comma-separated list of ingredient names that triggered this allergen
    /// Example: "Shrimp, Crab Meat"
    /// </summary>
    public string? SourceIngredients { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public Allergen Allergen { get; set; } = null!;
}