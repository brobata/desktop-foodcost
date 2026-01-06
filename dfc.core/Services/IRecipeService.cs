using Dfc.Core.Models;

namespace Dfc.Core.Services;

public interface IRecipeService
{
    Task<List<Recipe>> GetAllRecipesAsync(Guid locationId);
    Task<Recipe?> GetRecipeByIdAsync(Guid id);
    Task<List<Recipe>> SearchRecipesAsync(string searchTerm, Guid locationId);
    Task<Recipe> CreateRecipeAsync(Recipe recipe);
    Task<Recipe> UpdateRecipeAsync(Recipe recipe);
    Task DeleteRecipeAsync(Guid id);
    Task<bool> RecipeExistsAsync(string name, Guid locationId, Guid? excludeId = null);
}