namespace Dfc.Core.Models;

public class IngredientAllergen : BaseEntity
{
    public Guid IngredientId { get; set; }
    public Guid AllergenId { get; set; }

    /// <summary>
    /// True if this allergen was automatically detected from ingredient name
    /// </summary>
    public bool IsAutoDetected { get; set; }

    /// <summary>
    /// True if this allergen should be propagated to recipes/entrees (can be disabled by user)
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Source that triggered this allergen (usually the ingredient name itself)
    /// Example: "Chicken Breast"
    /// </summary>
    public string? SourceIngredients { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Allergen Allergen { get; set; } = null!;
}
