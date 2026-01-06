using Freecost.Core.Enums;
using Freecost.Core.Models;

namespace Freecost.Core.Services;

public interface IAllergenDetectionService
{
    /// <summary>
    /// Detects allergens from ingredient names
    /// </summary>
    /// <param name="ingredientName">Name of the ingredient to check</param>
    /// <returns>List of detected allergen types</returns>
    List<AllergenType> DetectAllergensFromIngredient(string ingredientName);

    /// <summary>
    /// Detects all allergens from a recipe's ingredients
    /// </summary>
    /// <param name="recipe">Recipe to analyze</param>
    /// <returns>Dictionary mapping allergen types to source ingredient names</returns>
    Dictionary<AllergenType, List<string>> DetectAllergensFromRecipe(Recipe recipe);

    /// <summary>
    /// Detects all allergens from an entree's ingredients and recipes
    /// </summary>
    /// <param name="entree">Entree to analyze</param>
    /// <returns>Dictionary mapping allergen types to source ingredient/recipe names</returns>
    Dictionary<AllergenType, List<string>> DetectAllergensFromEntree(Entree entree);
}
