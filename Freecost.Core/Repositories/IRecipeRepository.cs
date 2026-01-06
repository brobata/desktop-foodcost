// ⚠️ DON'T CREATE THIS FILE - IT ALREADY EXISTS!
// The file exists at: Freecost.Core/Services/IRecipeRepository.cs
// 
// ACTION REQUIRED:
// 1. DELETE the file at Freecost.Core/Services/IRecipeRepository.cs
// 2. CREATE this file at Freecost.Core/Repositories/IRecipeRepository.cs
// 3. Copy the code below:

using Freecost.Core.Models;

namespace Freecost.Core.Repositories;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllRecipesAsync(Guid locationId);
    Task<Recipe?> GetRecipeByIdAsync(Guid id);
    Task<IEnumerable<Recipe>> SearchRecipesAsync(string searchTerm, Guid locationId);
    Task<Recipe> CreateRecipeAsync(Recipe recipe);
    Task<Recipe> UpdateRecipeAsync(Recipe recipe);
    Task DeleteRecipeAsync(Guid id);
    Task<bool> RecipeExistsAsync(string name, Guid locationId, Guid? excludeId = null);
    Task<Recipe?> GetByNameAsync(string name, Guid locationId);
}