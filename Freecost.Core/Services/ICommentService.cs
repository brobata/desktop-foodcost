using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface ICommentService
{
    // Recipe Comments
    Task<RecipeComment> AddRecipeCommentAsync(Guid recipeId, Guid userId, string content, Guid? parentCommentId = null);
    Task<RecipeComment> EditRecipeCommentAsync(Guid commentId, string newContent);
    Task DeleteRecipeCommentAsync(Guid commentId);
    Task<List<RecipeComment>> GetRecipeCommentsAsync(Guid recipeId);
    Task<List<RecipeComment>> GetRecipeCommentThreadAsync(Guid parentCommentId);
    Task<RecipeComment?> GetRecipeCommentByIdAsync(Guid commentId);

    // Entree Comments
    Task<EntreeComment> AddEntreeCommentAsync(Guid entreeId, Guid userId, string content, Guid? parentCommentId = null);
    Task<EntreeComment> EditEntreeCommentAsync(Guid commentId, string newContent);
    Task DeleteEntreeCommentAsync(Guid commentId);
    Task<List<EntreeComment>> GetEntreeCommentsAsync(Guid entreeId);
    Task<List<EntreeComment>> GetEntreeCommentThreadAsync(Guid parentCommentId);
    Task<EntreeComment?> GetEntreeCommentByIdAsync(Guid commentId);

    // Mentions
    Task<List<RecipeComment>> GetCommentsMentioningUserAsync(Guid userId);
    Task<List<EntreeComment>> GetEntreeCommentsMentioningUserAsync(Guid userId);
}
