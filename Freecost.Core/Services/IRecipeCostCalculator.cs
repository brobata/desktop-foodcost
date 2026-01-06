using Freecost.Core.Models;

namespace Freecost.Core.Services;

/// <summary>
/// Interface for recipe cost calculation service
/// </summary>
public interface IRecipeCostCalculator
{
    /// <summary>
    /// Calculate the cost of a single recipe ingredient
    /// </summary>
    Task<decimal> CalculateIngredientCostAsync(RecipeIngredient recipeIngredient);

    /// <summary>
    /// Calculate the total cost for all ingredients in a recipe
    /// </summary>
    Task<decimal> CalculateRecipeTotalCostAsync(Recipe recipe);

    /// <summary>
    /// Calculate the cost per serving for a recipe
    /// </summary>
    Task<decimal> CalculateCostPerServingAsync(Recipe recipe);

    /// <summary>
    /// Calculate costs for all recipes in a list
    /// </summary>
    Task<Dictionary<Guid, RecipeCostSummary>> CalculateBatchCostsAsync(IEnumerable<Recipe> recipes);
}