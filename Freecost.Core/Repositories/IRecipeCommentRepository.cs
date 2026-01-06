using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IRecipeCommentRepository
{
    Task<RecipeComment?> GetByIdAsync(Guid id);
    Task<List<RecipeComment>> GetByRecipeAsync(Guid recipeId);
    Task<List<RecipeComment>> GetThreadAsync(Guid parentCommentId);
    Task<List<RecipeComment>> GetByUserAsync(Guid userId);
    Task<List<RecipeComment>> GetMentioningUserAsync(Guid userId);
    Task<RecipeComment> CreateAsync(RecipeComment comment);
    Task UpdateAsync(RecipeComment comment);
    Task DeleteAsync(Guid id);
}
