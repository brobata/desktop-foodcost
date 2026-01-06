using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IRecipeSharingService
{
    /// <summary>
    /// Share a recipe with another user
    /// </summary>
    Task<SharedRecipe> ShareRecipeAsync(
        Guid recipeId,
        Guid sharedByUserId,
        Guid sharedWithUserId,
        SharePermission permission,
        string? message = null,
        DateTime? expiresAt = null);

    /// <summary>
    /// Revoke access to a shared recipe
    /// </summary>
    Task RevokeAccessAsync(Guid sharedRecipeId);

    /// <summary>
    /// Get recipes shared with a user
    /// </summary>
    Task<List<SharedRecipe>> GetRecipesSharedWithUserAsync(Guid userId);

    /// <summary>
    /// Get recipes shared by a user
    /// </summary>
    Task<List<SharedRecipe>> GetRecipesSharedByUserAsync(Guid userId);

    /// <summary>
    /// Check if user has access to a recipe
    /// </summary>
    Task<bool> HasAccessAsync(Guid userId, Guid recipeId);

    /// <summary>
    /// Get user's permission level for a recipe
    /// </summary>
    Task<SharePermission?> GetPermissionAsync(Guid userId, Guid recipeId);

    /// <summary>
    /// Update share permission
    /// </summary>
    Task UpdatePermissionAsync(Guid sharedRecipeId, SharePermission newPermission);

    /// <summary>
    /// Get all users a recipe is shared with
    /// </summary>
    Task<List<SharedRecipe>> GetRecipeSharesAsync(Guid recipeId);
}
