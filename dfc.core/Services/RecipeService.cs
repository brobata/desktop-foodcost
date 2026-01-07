using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _repository;
    private readonly IRecipeCostCalculator _costCalculator;
    private readonly ILogger<RecipeService>? _logger;

    public RecipeService(
        IRecipeRepository repository,
        IRecipeCostCalculator costCalculator,
        ILogger<RecipeService>? logger = null)
    {
        _repository = repository;
        _costCalculator = costCalculator;
        _logger = logger;
    }

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"    [RECIPE SERVICE] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("    ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("    ║ [RECIPE SERVICE ERROR]                            ║");
        System.Diagnostics.Debug.WriteLine("    ╠═══════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"    {message}");
        if (ex != null)
        {
            System.Diagnostics.Debug.WriteLine($"    Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"    Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"    Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"    Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        System.Diagnostics.Debug.WriteLine("    ╚═══════════════════════════════════════════════════╝");
    }

    public async Task<List<Recipe>> GetAllRecipesAsync(Guid locationId)
    {
        var recipes = await _repository.GetAllRecipesAsync(locationId);
        var recipeList = recipes.ToList();

        var costTasks = recipeList.Select(recipe => _costCalculator.CalculateRecipeTotalCostAsync(recipe)).ToList();
        await Task.WhenAll(costTasks);

        return recipeList;
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
    {
        var recipe = await _repository.GetRecipeByIdAsync(id);

        if (recipe != null)
        {
            await _costCalculator.CalculateRecipeTotalCostAsync(recipe);
        }

        return recipe;
    }

    public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm, Guid locationId)
    {
        var recipes = await _repository.SearchRecipesAsync(searchTerm, locationId);
        var recipeList = recipes.ToList();

        var costTasks = recipeList.Select(recipe => _costCalculator.CalculateRecipeTotalCostAsync(recipe)).ToList();
        await Task.WhenAll(costTasks);

        return recipeList;
    }

    public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting CreateRecipeAsync                        ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Recipe Name: {recipe.Name}");
            LogDebug($"Recipe ID: {recipe.Id}");
            LogDebug($"Location ID: {recipe.LocationId}");
            LogDebug($"Ingredient Count: {recipe.RecipeIngredients?.Count ?? 0}");

            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                LogError("Recipe name is required");
                throw new ArgumentException("Recipe name is required");
            }

            if (recipe.LocationId == Guid.Empty)
            {
                LogError("Location ID is required");
                throw new ArgumentException("Location ID is required");
            }

            LogDebug("Validation passed");

            LogDebug("Calculating recipe costs...");
            try
            {
                await _costCalculator.CalculateRecipeTotalCostAsync(recipe);
                LogDebug($"Cost calculation complete. Total Cost: {recipe.TotalCost}");
            }
            catch (Exception ex)
            {
                LogError("Error calculating recipe costs", ex);
                LogDebug("Continuing with recipe creation despite cost calculation error");
            }

            LogDebug("Calling repository.CreateRecipeAsync...");
            var created = await _repository.CreateRecipeAsync(recipe);
            LogDebug($"✓ Recipe created successfully. ID: {created.Id}");

            LogDebug("╚═══════════════════════════════════════════════════╝");

            return created;
        }
        catch (Exception ex)
        {
            LogError($"Failed to create recipe '{recipe.Name}'", ex);
            throw;
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting UpdateRecipeAsync                        ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Recipe Name: {recipe.Name}");
            LogDebug($"Recipe ID: {recipe.Id}");
            LogDebug($"Location ID: {recipe.LocationId}");
            LogDebug($"Ingredient Count: {recipe.RecipeIngredients?.Count ?? 0}");

            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                LogError("Recipe name is required");
                throw new ArgumentException("Recipe name is required");
            }

            LogDebug("Checking if recipe exists...");
            var exists = await _repository.GetRecipeByIdAsync(recipe.Id);
            if (exists == null)
            {
                LogError($"Recipe not found: {recipe.Id}");
                throw new InvalidOperationException("Recipe not found");
            }
            LogDebug("Recipe exists - proceeding with update");

            LogDebug("Calculating recipe costs...");
            try
            {
                await _costCalculator.CalculateRecipeTotalCostAsync(recipe);
                LogDebug($"Cost calculation complete. Total Cost: {recipe.TotalCost}");
            }
            catch (Exception ex)
            {
                LogError("Error calculating recipe costs", ex);
                LogDebug("Continuing with recipe update despite cost calculation error");
            }

            LogDebug("Calling repository.UpdateRecipeAsync...");
            var updated = await _repository.UpdateRecipeAsync(recipe);
            LogDebug($"✓ Recipe updated successfully. ID: {updated.Id}");

            LogDebug("╚═══════════════════════════════════════════════════╝");

            return updated;
        }
        catch (Exception ex)
        {
            LogError($"Failed to update recipe '{recipe.Name}'", ex);
            throw;
        }
    }

    public async Task DeleteRecipeAsync(Guid id)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting DeleteRecipeAsync                        ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Recipe ID: {id}");

            LogDebug("Checking if recipe exists...");
            var recipe = await _repository.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                LogError($"Recipe not found: {id}");
                throw new InvalidOperationException("Recipe not found");
            }
            LogDebug($"Recipe exists: '{recipe.Name}'");

            LogDebug("Calling repository.DeleteRecipeAsync...");
            await _repository.DeleteRecipeAsync(id);
            LogDebug("✓ Recipe deleted successfully");

            LogDebug("╚═══════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            LogError($"Failed to delete recipe {id}", ex);
            throw;
        }
    }

    public async Task<bool> RecipeExistsAsync(string name, Guid locationId, Guid? excludeId = null)
    {
        return await _repository.RecipeExistsAsync(name, locationId, excludeId);
    }
}
