namespace Dfc.Core.Models;

public class EntreeAllergen : BaseEntity
{
    public Guid EntreeId { get; set; }
    public Guid AllergenId { get; set; }

    /// <summary>
    /// True if this allergen was automatically detected from ingredients/recipes
    /// </summary>
    public bool IsAutoDetected { get; set; }

    /// <summary>
    /// True if this allergen should appear on recipe cards (can be disabled by user)
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Comma-separated list of ingredient/recipe names that triggered this allergen
    /// Example: "Country Fried Shrimp (Shellfish), Gorgonzola Cream (Milk)"
    /// </summary>
    public string? SourceIngredients { get; set; }

    public Entree Entree { get; set; } = null!;
    public Allergen Allergen { get; set; } = null!;
}