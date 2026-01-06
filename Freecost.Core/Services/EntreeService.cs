using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class EntreeService : IEntreeService
{
    private readonly IEntreeRepository _entreeRepository;
    private readonly IRecipeCostCalculator _costCalculator;
    private readonly IUserSessionService? _sessionService;
    private readonly SupabaseDataService? _dataService;
    private readonly IPhotoService? _photoService;
    private readonly ILogger<EntreeService>? _logger;

    public EntreeService(
        IEntreeRepository entreeRepository, 
        IRecipeCostCalculator costCalculator,
        IUserSessionService? sessionService = null,
        SupabaseDataService? dataService = null,
        IPhotoService? photoService = null,
        ILogger<EntreeService>? logger = null)
    {
        _entreeRepository = entreeRepository;
        _costCalculator = costCalculator;
        _sessionService = sessionService;
        _dataService = dataService;
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
        var created = await _entreeRepository.CreateAsync(entree);

        // Auto-sync to Supabase if authenticated
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (!string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine($"[EntreeService] Auto-syncing entree '{created.Name}' to Supabase");
                    SyncDebugLogger.WriteInfo($"Auto-syncing entree '{created.Name}' to Supabase");

                    var supabaseEntree = created.ToSupabase();
                    var result = await _dataService.UpsertAsync(supabaseEntree);

                    if (result.IsSuccess)
                    {
                        SyncDebugLogger.WriteSuccess($"Entree '{created.Name}' synced to Supabase");
                        Debug.WriteLine($"[EntreeService] Successfully synced entree to Supabase");
                    }
                    else
                    {
                        SyncDebugLogger.WriteError("Auto-sync entree failed", new Exception(result.Error ?? "Unknown"));
                        _logger?.LogWarning("Failed to auto-sync entree to Supabase: {Error}", result.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError("Auto-sync entree exception", ex);
                Debug.WriteLine($"[EntreeService] Exception auto-syncing entree: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to auto-sync entree to Supabase");
            }
        }

        return created;
    }

    public async Task<Entree> UpdateEntreeAsync(Entree entree)
    {
        var updated = await _entreeRepository.UpdateAsync(entree);

        // Auto-sync to Supabase if authenticated
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (!string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine($"[EntreeService] Auto-syncing updated entree '{updated.Name}' to Supabase");
                    SyncDebugLogger.WriteInfo($"Auto-syncing updated entree '{updated.Name}' to Supabase");

                    var supabaseEntree = updated.ToSupabase();
                    var result = await _dataService.UpsertAsync(supabaseEntree);

                    if (result.IsSuccess)
                    {
                        SyncDebugLogger.WriteSuccess($"Updated entree '{updated.Name}' synced to Supabase");
                        Debug.WriteLine($"[EntreeService] Successfully synced updated entree to Supabase");
                    }
                    else
                    {
                        SyncDebugLogger.WriteError("Auto-sync updated entree failed", new Exception(result.Error ?? "Unknown"));
                        _logger?.LogWarning("Failed to auto-sync updated entree to Supabase: {Error}", result.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError("Auto-sync update entree exception", ex);
                Debug.WriteLine($"[EntreeService] Exception auto-syncing updated entree: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to auto-sync updated entree to Supabase");
            }
        }

        return updated;
    }

    public async Task DeleteEntreeAsync(Guid id)
    {
        // Get entree to check for photo before deleting
        var entree = await _entreeRepository.GetByIdAsync(id);
        
        // Delete the entree from database
        await _entreeRepository.DeleteAsync(id);

        // Delete photo from Supabase Storage if exists
        if (entree != null && !string.IsNullOrEmpty(entree.PhotoUrl) && _photoService != null)
        {
            try
            {
                Debug.WriteLine($"[EntreeService] Deleting photo for entree: {entree.PhotoUrl}");
                await _photoService.DeletePhotoAsync(entree.PhotoUrl);
                Debug.WriteLine($"[EntreeService] ✓ Photo deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EntreeService] ⚠ Failed to delete photo: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to delete photo for entree {EntreeId}", id);
            }
        }

        // Auto-sync deletion to Supabase if authenticated
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (!string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine($"[EntreeService] Auto-syncing deletion of entree {id} to Supabase");
                    SyncDebugLogger.WriteInfo($"Auto-syncing deletion of entree {id} to Supabase");

                    var result = await _dataService.DeleteAsync<SupabaseEntree>(id);

                    if (result.IsSuccess)
                    {
                        SyncDebugLogger.WriteSuccess($"Entree {id} deleted from Supabase");
                        Debug.WriteLine($"[EntreeService] Successfully deleted entree from Supabase");
                    }
                    else
                    {
                        SyncDebugLogger.WriteError("Auto-sync entree deletion failed", new Exception(result.Error ?? "Unknown"));
                        _logger?.LogWarning("Failed to auto-sync entree deletion to Supabase: {Error}", result.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError("Auto-sync entree deletion exception", ex);
                Debug.WriteLine($"[EntreeService] Exception auto-syncing deletion: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to auto-sync entree deletion to Supabase");
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