using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class RecipeSharingService : IRecipeSharingService
{
    private readonly ISharedRecipeRepository _repository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserRepository _userRepository;

    public RecipeSharingService(
        ISharedRecipeRepository repository,
        IRecipeRepository recipeRepository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _recipeRepository = recipeRepository;
        _userRepository = userRepository;
    }

    public async Task<SharedRecipe> ShareRecipeAsync(
        Guid recipeId,
        Guid sharedByUserId,
        Guid sharedWithUserId,
        SharePermission permission,
        string? message = null,
        DateTime? expiresAt = null)
    {
        // Verify recipe exists
        var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);
        if (recipe == null)
        {
            throw new InvalidOperationException($"Recipe with ID {recipeId} not found");
        }

        // Verify users exist
        var sharedByUser = await _userRepository.GetByIdAsync(sharedByUserId);
        var sharedWithUser = await _userRepository.GetByIdAsync(sharedWithUserId);

        if (sharedByUser == null || sharedWithUser == null)
        {
            throw new InvalidOperationException("One or both users not found");
        }

        // Check if already shared
        var existingShare = await _repository.GetByRecipeAndUserAsync(recipeId, sharedWithUserId);
        if (existingShare != null && existingShare.IsActive)
        {
            // Update existing share
            existingShare.Permission = permission;
            existingShare.ExpiresAt = expiresAt;
            existingShare.Message = message;
            await _repository.UpdateAsync(existingShare);
            return existingShare;
        }

        // Create new share
        var sharedRecipe = new SharedRecipe
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            SharedByUserId = sharedByUserId,
            SharedWithUserId = sharedWithUserId,
            Permission = permission,
            SharedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(sharedRecipe);
    }

    public async Task RevokeAccessAsync(Guid sharedRecipeId)
    {
        var sharedRecipe = await _repository.GetByIdAsync(sharedRecipeId);
        if (sharedRecipe != null)
        {
            sharedRecipe.IsActive = false;
            await _repository.UpdateAsync(sharedRecipe);
        }
    }

    public async Task<List<SharedRecipe>> GetRecipesSharedWithUserAsync(Guid userId)
    {
        var shares = await _repository.GetSharedWithUserAsync(userId);

        // Filter out expired shares
        return shares
            .Where(s => s.IsActive && (!s.ExpiresAt.HasValue || s.ExpiresAt.Value > DateTime.UtcNow))
            .ToList();
    }

    public async Task<List<SharedRecipe>> GetRecipesSharedByUserAsync(Guid userId)
    {
        return await _repository.GetSharedByUserAsync(userId);
    }

    public async Task<bool> HasAccessAsync(Guid userId, Guid recipeId)
    {
        var permission = await GetPermissionAsync(userId, recipeId);
        return permission.HasValue;
    }

    public async Task<SharePermission?> GetPermissionAsync(Guid userId, Guid recipeId)
    {
        var share = await _repository.GetByRecipeAndUserAsync(recipeId, userId);

        if (share == null || !share.IsActive)
        {
            return null;
        }

        // Check if expired
        if (share.ExpiresAt.HasValue && share.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return null;
        }

        return share.Permission;
    }

    public async Task UpdatePermissionAsync(Guid sharedRecipeId, SharePermission newPermission)
    {
        var sharedRecipe = await _repository.GetByIdAsync(sharedRecipeId);
        if (sharedRecipe != null)
        {
            sharedRecipe.Permission = newPermission;
            await _repository.UpdateAsync(sharedRecipe);
        }
    }

    public async Task<List<SharedRecipe>> GetRecipeSharesAsync(Guid recipeId)
    {
        return await _repository.GetByRecipeAsync(recipeId);
    }
}
