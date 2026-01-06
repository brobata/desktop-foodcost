using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Repositories;

public interface ISharedRecipeRepository
{
    Task<SharedRecipe?> GetByIdAsync(Guid id);
    Task<SharedRecipe?> GetByRecipeAndUserAsync(Guid recipeId, Guid userId);
    Task<List<SharedRecipe>> GetByRecipeAsync(Guid recipeId);
    Task<List<SharedRecipe>> GetSharedWithUserAsync(Guid userId);
    Task<List<SharedRecipe>> GetSharedByUserAsync(Guid userId);
    Task<SharedRecipe> CreateAsync(SharedRecipe sharedRecipe);
    Task UpdateAsync(SharedRecipe sharedRecipe);
    Task DeleteAsync(Guid id);
}
