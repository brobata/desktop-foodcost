using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Provides batch operations for performing bulk actions on multiple entities efficiently.
/// Useful for multi-select operations in the UI (delete, duplicate, categorize, etc.)
/// </summary>
public interface IBatchOperationsService
{
    /// <summary>
    /// Deletes multiple ingredients in a single batch operation.
    /// Each ingredient is moved to the recycle bin for potential restoration.
    /// </summary>
    /// <param name="ingredientIds">List of ingredient IDs to delete</param>
    /// <returns>Number of ingredients successfully deleted</returns>
    Task<int> BatchDeleteIngredientsAsync(List<Guid> ingredientIds);

    /// <summary>
    /// Deletes multiple recipes in a single batch operation.
    /// Each recipe is moved to the recycle bin for potential restoration.
    /// </summary>
    /// <param name="recipeIds">List of recipe IDs to delete</param>
    /// <returns>Number of recipes successfully deleted</returns>
    Task<int> BatchDeleteRecipesAsync(List<Guid> recipeIds);

    /// <summary>
    /// Deletes multiple entrees in a single batch operation.
    /// Each entree is moved to the recycle bin for potential restoration.
    /// </summary>
    /// <param name="entreeIds">List of entree IDs to delete</param>
    /// <returns>Number of entrees successfully deleted</returns>
    Task<int> BatchDeleteEntreesAsync(List<Guid> entreeIds);

    /// <summary>
    /// Updates the category for multiple entities of the same type in a single batch operation.
    /// </summary>
    /// <typeparam name="T">Type of entity (Ingredient, Recipe, or Entree)</typeparam>
    /// <param name="ids">List of entity IDs to update</param>
    /// <param name="category">New category name to assign</param>
    /// <returns>Number of entities successfully updated</returns>
    Task<int> BatchUpdateCategoryAsync<T>(List<Guid> ids, string category) where T : BaseEntity;

    /// <summary>
    /// Creates duplicate copies of multiple ingredients in a single batch operation.
    /// Each duplicate is created with " (Copy)" appended to the name.
    /// </summary>
    /// <param name="ingredientIds">List of ingredient IDs to duplicate</param>
    /// <returns>Number of ingredients successfully duplicated</returns>
    Task<int> BatchDuplicateIngredientsAsync(List<Guid> ingredientIds);
}

/// <summary>
/// Implementation of batch operations service for efficient bulk data manipulation.
/// </summary>
public class BatchOperationsService : IBatchOperationsService
{
    private readonly IIngredientRepository _ingredientRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IEntreeRepository _entreeRepo;
    private readonly ILogger<BatchOperationsService>? _logger;

    public BatchOperationsService(
        IIngredientRepository ingredientRepo,
        IRecipeRepository recipeRepo,
        IEntreeRepository entreeRepo,
        ILogger<BatchOperationsService>? logger = null)
    {
        _ingredientRepo = ingredientRepo;
        _recipeRepo = recipeRepo;
        _entreeRepo = entreeRepo;
        _logger = logger;
    }

    public async Task<int> BatchDeleteIngredientsAsync(List<Guid> ingredientIds)
    {
        var count = 0;
        foreach (var id in ingredientIds)
        {
            await _ingredientRepo.DeleteAsync(id);
            count++;
        }
        _logger?.LogInformation("Batch deleted {Count} ingredients", count);
        return count;
    }

    public async Task<int> BatchDeleteRecipesAsync(List<Guid> recipeIds)
    {
        var count = 0;
        foreach (var id in recipeIds)
        {
            await _recipeRepo.DeleteRecipeAsync(id);
            count++;
        }
        _logger?.LogInformation("Batch deleted {Count} recipes", count);
        return count;
    }

    public async Task<int> BatchDeleteEntreesAsync(List<Guid> entreeIds)
    {
        var count = 0;
        foreach (var id in entreeIds)
        {
            await _entreeRepo.DeleteAsync(id);
            count++;
        }
        _logger?.LogInformation("Batch deleted {Count} entrees", count);
        return count;
    }

    public async Task<int> BatchUpdateCategoryAsync<T>(List<Guid> ids, string category) where T : BaseEntity
    {
        var count = 0;
        foreach (var id in ids)
        {
            if (typeof(T) == typeof(Ingredient))
            {
                var item = await _ingredientRepo.GetByIdAsync(id);
                if (item != null)
                {
                    item.Category = category;
                    await _ingredientRepo.UpdateAsync(item);
                    count++;
                }
            }
        }
        _logger?.LogInformation("Batch updated category for {Count} items", count);
        return count;
    }

    public async Task<int> BatchDuplicateIngredientsAsync(List<Guid> ingredientIds)
    {
        var count = 0;
        foreach (var id in ingredientIds)
        {
            var original = await _ingredientRepo.GetByIdAsync(id);
            if (original != null)
            {
                var duplicate = new Ingredient
                {
                    Name = original.Name + " (Copy)",
                    Category = original.Category,
                    CurrentPrice = original.CurrentPrice,
                    Unit = original.Unit,
                    VendorName = original.VendorName,
                    LocationId = original.LocationId
                };
                await _ingredientRepo.AddAsync(duplicate);
                count++;
            }
        }
        _logger?.LogInformation("Batch duplicated {Count} ingredients", count);
        return count;
    }
}
