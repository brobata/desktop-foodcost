using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class EntreeService : IEntreeService
{
    private readonly IEntreeRepository _entreeRepository;
    private readonly IRecipeCostCalculator _costCalculator;
    private readonly IPhotoService? _photoService;
    private readonly ILogger<EntreeService>? _logger;

    public EntreeService(
        IEntreeRepository entreeRepository,
        IRecipeCostCalculator costCalculator,
        IPhotoService? photoService = null,
        ILogger<EntreeService>? logger = null)
    {
        _entreeRepository = entreeRepository;
        _costCalculator = costCalculator;
        _photoService = photoService;
        _logger = logger;
    }

    public async Task<List<Entree>> GetAllEntreesAsync(Guid locationId)
    {
        var entrees = (await _entreeRepository.GetAllAsync(locationId)).ToList();
        foreach (var entree in entrees)
        {
            entree.TotalCost = await CalculateEntreeCostAsync(entree);
        }
        return entrees;
    }

    public async Task<Entree?> GetEntreeByIdAsync(Guid id)
    {
        var entree = await _entreeRepository.GetByIdAsync(id);
        if (entree != null)
        {
            entree.TotalCost = await CalculateEntreeCostAsync(entree);
        }
        return entree;
    }

    public async Task<Entree> CreateEntreeAsync(Entree entree)
    {
        return await _entreeRepository.CreateAsync(entree);
    }

    public async Task<Entree> UpdateEntreeAsync(Entree entree)
    {
        return await _entreeRepository.UpdateAsync(entree);
    }

    public async Task DeleteEntreeAsync(Guid id)
    {
        // Get entree to check for photo before deleting
        var entree = await _entreeRepository.GetByIdAsync(id);

        // Delete the entree from database
        await _entreeRepository.DeleteAsync(id);

        // Delete photo if exists
        if (entree != null && !string.IsNullOrEmpty(entree.PhotoUrl) && _photoService != null)
        {
            try
            {
                Debug.WriteLine($"[EntreeService] Deleting photo for entree: {entree.PhotoUrl}");
                await _photoService.DeletePhotoAsync(entree.PhotoUrl);
                Debug.WriteLine($"[EntreeService] Photo deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EntreeService] Failed to delete photo: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to delete photo for entree {EntreeId}", id);
            }
        }
    }

    public async Task<decimal> CalculateEntreeCostAsync(Entree entree)
    {
        decimal totalCost = 0;

        // Calculate cost from recipe components
        if (entree.EntreeRecipes != null)
        {
            foreach (var entreeRecipe in entree.EntreeRecipes.Where(er => er.Recipe != null))
            {
                var recipeCost = await _costCalculator.CalculateRecipeTotalCostAsync(entreeRecipe.Recipe);
                totalCost += recipeCost * entreeRecipe.Quantity;
            }
        }

        // Calculate cost from direct ingredient components
        if (entree.EntreeIngredients != null)
        {
            foreach (var entreeIngredient in entree.EntreeIngredients.Where(ei => ei.Ingredient != null))
            {
                var tempRecipeIngredient = new RecipeIngredient
                {
                    Ingredient = entreeIngredient.Ingredient,
                    IngredientId = entreeIngredient.IngredientId,
                    Quantity = entreeIngredient.Quantity,
                    Unit = entreeIngredient.Unit
                };
                totalCost += await _costCalculator.CalculateIngredientCostAsync(tempRecipeIngredient);
            }
        }

        return totalCost;
    }
}
