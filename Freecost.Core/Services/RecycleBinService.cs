using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class RecycleBinService : IRecycleBinService
{
    private readonly IDeletedItemRepository _deletedItemRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;
    private readonly ILocalModificationService _localModificationService;
    private readonly IUserSessionService? _sessionService;
    private readonly SupabaseDataService? _dataService;
    private readonly ILogger<RecycleBinService>? _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RecycleBinService(
        IDeletedItemRepository deletedItemRepository,
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IEntreeRepository entreeRepository,
        ILocalModificationService localModificationService,
        IUserSessionService? sessionService = null,
        SupabaseDataService? dataService = null,
        ILogger<RecycleBinService>? logger = null)
    {
        _deletedItemRepository = deletedItemRepository;
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;
        _localModificationService = localModificationService;
        _sessionService = sessionService;
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<DeletedItem> MoveToRecycleBinAsync<T>(T entity, DeletedItemType type, Guid locationId) where T : BaseEntity
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Starting MoveToRecycleBinAsync");
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Entity Type: {typeof(T).Name}");
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Item Type: {type}");
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Location ID: {locationId}");
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Entity ID: {entity.Id}");

            // CRITICAL: Clear navigation properties before serialization to avoid circular references
            // and serialization issues, especially for online locations with User→Location references
            string itemName = type switch
            {
                DeletedItemType.Ingredient => (entity as Ingredient)?.Name ?? "Unknown",
                DeletedItemType.Recipe => (entity as Recipe)?.Name ?? "Unknown",
                DeletedItemType.Entree => (entity as Entree)?.Name ?? "Unknown",
                _ => "Unknown"
            };
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Item Name: {itemName}");

        // Clear navigation properties based on type to avoid serialization issues
        switch (type)
        {
            case DeletedItemType.Ingredient when entity is Ingredient ingredient:
                ingredient.Location = null!;
                ingredient.PriceHistory = new List<PriceHistory>();
                ingredient.RecipeIngredients = new List<RecipeIngredient>();
                ingredient.EntreeIngredients = new List<EntreeIngredient>();

                // Keep allergens and aliases for restoration, but clear their navigation properties
                if (ingredient.IngredientAllergens != null)
                {
                    foreach (var allergen in ingredient.IngredientAllergens)
                    {
                        allergen.Ingredient = null!;
                        allergen.Allergen = null!;
                    }
                }
                if (ingredient.Aliases != null)
                {
                    foreach (var alias in ingredient.Aliases)
                    {
                        alias.Ingredient = null!;
                    }
                }
                break;

            case DeletedItemType.Recipe when entity is Recipe recipe:
                recipe.Location = null!;
                recipe.EntreeRecipes = new List<EntreeRecipe>();
                recipe.Photos = new List<Photo>();

                // Keep ingredients and allergens for restoration, but clear their navigation properties
                if (recipe.RecipeIngredients != null)
                {
                    foreach (var ingredient in recipe.RecipeIngredients)
                    {
                        ingredient.Recipe = null!;
                        ingredient.Ingredient = null!;
                    }
                }
                if (recipe.RecipeAllergens != null)
                {
                    foreach (var allergen in recipe.RecipeAllergens)
                    {
                        allergen.Recipe = null!;
                        allergen.Allergen = null!;
                    }
                }
                break;

            case DeletedItemType.Entree when entity is Entree entree:
                entree.Location = null!;
                entree.Photos = new List<Photo>();

                // Keep recipes, ingredients, and allergens for restoration, but clear their navigation properties
                if (entree.EntreeRecipes != null)
                {
                    foreach (var recipe in entree.EntreeRecipes)
                    {
                        recipe.Entree = null!;
                        recipe.Recipe = null!;
                    }
                }
                if (entree.EntreeIngredients != null)
                {
                    foreach (var ingredient in entree.EntreeIngredients)
                    {
                        ingredient.Entree = null!;
                        ingredient.Ingredient = null!;
                    }
                }
                if (entree.EntreeAllergens != null)
                {
                    foreach (var allergen in entree.EntreeAllergens)
                    {
                        allergen.Entree = null!;
                        allergen.Allergen = null!;
                    }
                }
                break;
        }

            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Clearing navigation properties complete");

            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Serializing entity to JSON...");
            var serializedData = JsonSerializer.Serialize(entity, _jsonOptions);
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Serialization complete. Length: {serializedData.Length} chars");

            var deletedItem = new DeletedItem
            {
                ItemId = entity.Id,
                ItemType = type,
                ItemName = itemName,
                SerializedData = serializedData,
                LocationId = locationId,
                DeletedDate = DateTime.UtcNow
            };
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] DeletedItem created. ItemId: {deletedItem.ItemId}");

            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Adding to DeletedItemRepository...");
            var result = await _deletedItemRepository.AddAsync(deletedItem);
            System.Diagnostics.Debug.WriteLine($"  [RECYCLE BIN] Successfully added. Result Id: {result.Id}");

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("  ║ [RECYCLE BIN EXCEPTION]                           ║");
            System.Diagnostics.Debug.WriteLine("  ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"  Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"  Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("  ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task<T?> RestoreAsync<T>(Guid deletedItemId) where T : BaseEntity
    {
        var deletedItem = await _deletedItemRepository.GetByIdAsync(deletedItemId);
        if (deletedItem == null)
            return null;

        try
        {
            var entity = JsonSerializer.Deserialize<T>(deletedItem.SerializedData, _jsonOptions);
            if (entity == null)
                return null;

            // Clear the Id and reset timestamps to let the repository create new ones
            entity.Id = Guid.Empty;
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;

            // Restore to database based on type
            switch (deletedItem.ItemType)
            {
                case DeletedItemType.Ingredient when entity is Ingredient ingredient:
                    // Clear ALL navigation property references to avoid EF Core tracking conflicts
                    ingredient.Location = null!; // CRITICAL: Clear Location to avoid tracking conflict
                    ingredient.RecipeIngredients?.Clear();
                    ingredient.EntreeIngredients?.Clear();
                    ingredient.PriceHistory?.Clear();

                    // Reset allergen IDs if present
                    if (ingredient.IngredientAllergens != null)
                    {
                        foreach (var allergen in ingredient.IngredientAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.IngredientId = Guid.Empty; // Will be set by repository
                            allergen.Ingredient = null!; // Clear navigation property
                            allergen.Allergen = null!; // Clear navigation property
                        }
                    }

                    await _ingredientRepository.AddAsync(ingredient);

                    // Auto-sync restored ingredient to Supabase if authenticated
                    if (_sessionService?.IsAuthenticated == true && _dataService != null)
                    {
                        try
                        {
                            var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                            if (!string.IsNullOrEmpty(userId))
                            {
                                Debug.WriteLine($"[RecycleBinService] Auto-syncing restored ingredient '{ingredient.Name}' to Supabase");
                                SyncDebugLogger.WriteInfo($"Auto-syncing restored ingredient '{ingredient.Name}' to Supabase");

                                var supabaseIngredient = ingredient.ToSupabase();
                                var result = await _dataService.UpsertAsync(supabaseIngredient);

                                if (result.IsSuccess)
                                {
                                    SyncDebugLogger.WriteSuccess($"Restored ingredient '{ingredient.Name}' synced to Supabase");
                                    Debug.WriteLine($"[RecycleBinService] Successfully synced restored ingredient to Supabase");
                                }
                                else
                                {
                                    SyncDebugLogger.WriteError("Auto-sync restored ingredient failed", new Exception(result.Error ?? "Unknown"));
                                    _logger?.LogWarning("Failed to auto-sync restored ingredient to Supabase: {Error}", result.Error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SyncDebugLogger.WriteError("Auto-sync restore ingredient exception", ex);
                            Debug.WriteLine($"[RecycleBinService] Exception auto-syncing restored ingredient: {ex.Message}");
                            _logger?.LogWarning(ex, "Failed to auto-sync restored ingredient to Supabase");
                        }
                    }

                    break;
                case DeletedItemType.Recipe when entity is Recipe recipe:
                    // Clear Location navigation property to avoid EF Core tracking conflicts
                    recipe.Location = null!;

                    // Preserve allergens and ingredients but reset their IDs and clear navigation properties
                    if (recipe.RecipeAllergens != null)
                    {
                        foreach (var allergen in recipe.RecipeAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.RecipeId = Guid.Empty; // Will be set by EF Core
                            allergen.Recipe = null!; // Clear navigation property to avoid tracking conflicts
                            allergen.Allergen = null!; // Clear navigation property
                        }
                    }

                    if (recipe.RecipeIngredients != null)
                    {
                        foreach (var ingredient in recipe.RecipeIngredients)
                        {
                            ingredient.Id = Guid.NewGuid();
                            ingredient.RecipeId = Guid.Empty; // Will be set by EF Core
                            ingredient.Recipe = null!; // Clear navigation property to avoid tracking conflicts
                            ingredient.Ingredient = null!; // Clear navigation property
                        }
                    }

                    // Clear EntreeRecipes to avoid conflicts (these are references, not core data)
                    recipe.EntreeRecipes?.Clear();
                    // Clear photos to avoid file path conflicts
                    recipe.Photos?.Clear();

                    await _recipeRepository.CreateRecipeAsync(recipe);

                    // Auto-sync restored recipe to Supabase if authenticated
                    if (_sessionService?.IsAuthenticated == true && _dataService != null)
                    {
                        try
                        {
                            var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                            if (!string.IsNullOrEmpty(userId))
                            {
                                Debug.WriteLine($"[RecycleBinService] Auto-syncing restored recipe '{recipe.Name}' to Supabase");
                                SyncDebugLogger.WriteInfo($"Auto-syncing restored recipe '{recipe.Name}' to Supabase");

                                var supabaseRecipe = recipe.ToSupabase();
                                var result = await _dataService.UpsertAsync(supabaseRecipe);

                                if (result.IsSuccess)
                                {
                                    SyncDebugLogger.WriteSuccess($"Restored recipe '{recipe.Name}' synced to Supabase");
                                    Debug.WriteLine($"[RecycleBinService] Successfully synced restored recipe to Supabase");
                                }
                                else
                                {
                                    SyncDebugLogger.WriteError("Auto-sync restored recipe failed", new Exception(result.Error ?? "Unknown"));
                                    _logger?.LogWarning("Failed to auto-sync restored recipe to Supabase: {Error}", result.Error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SyncDebugLogger.WriteError("Auto-sync restore recipe exception", ex);
                            Debug.WriteLine($"[RecycleBinService] Exception auto-syncing restored recipe: {ex.Message}");
                            _logger?.LogWarning(ex, "Failed to auto-sync restored recipe to Supabase");
                        }
                    }

                    break;
                case DeletedItemType.Entree when entity is Entree entree:
                    // Clear Location navigation property to avoid EF Core tracking conflicts
                    entree.Location = null!;

                    // Preserve allergens, recipes, and ingredients but reset their IDs and clear navigation properties
                    if (entree.EntreeAllergens != null)
                    {
                        foreach (var allergen in entree.EntreeAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.EntreeId = Guid.Empty; // Will be set by EF Core
                            allergen.Entree = null!; // Clear navigation property to avoid tracking conflicts
                            allergen.Allergen = null!; // Clear navigation property
                        }
                    }

                    if (entree.EntreeRecipes != null)
                    {
                        foreach (var recipe in entree.EntreeRecipes)
                        {
                            recipe.Id = Guid.NewGuid();
                            recipe.EntreeId = Guid.Empty; // Will be set by EF Core
                            recipe.Entree = null!; // Clear navigation property to avoid tracking conflicts
                            recipe.Recipe = null!; // Clear navigation property
                        }
                    }

                    if (entree.EntreeIngredients != null)
                    {
                        foreach (var ingredient in entree.EntreeIngredients)
                        {
                            ingredient.Id = Guid.NewGuid();
                            ingredient.EntreeId = Guid.Empty; // Will be set by EF Core
                            ingredient.Entree = null!; // Clear navigation property to avoid tracking conflicts
                            ingredient.Ingredient = null!; // Clear navigation property
                        }
                    }

                    // Clear photos to avoid file path conflicts
                    entree.Photos?.Clear();

                    await _entreeRepository.CreateAsync(entree);

                    // Auto-sync restored entree to Supabase if authenticated
                    if (_sessionService?.IsAuthenticated == true && _dataService != null)
                    {
                        try
                        {
                            var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                            if (!string.IsNullOrEmpty(userId))
                            {
                                Debug.WriteLine($"[RecycleBinService] Auto-syncing restored entree '{entree.Name}' to Supabase");
                                SyncDebugLogger.WriteInfo($"Auto-syncing restored entree '{entree.Name}' to Supabase");

                                var supabaseEntree = entree.ToSupabase();
                                var result = await _dataService.UpsertAsync(supabaseEntree);

                                if (result.IsSuccess)
                                {
                                    SyncDebugLogger.WriteSuccess($"Restored entree '{entree.Name}' synced to Supabase");
                                    Debug.WriteLine($"[RecycleBinService] Successfully synced restored entree to Supabase");
                                }
                                else
                                {
                                    SyncDebugLogger.WriteError("Auto-sync restored entree failed", new Exception(result.Error ?? "Unknown"));
                                    _logger?.LogWarning("Failed to auto-sync restored entree to Supabase: {Error}", result.Error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SyncDebugLogger.WriteError("Auto-sync restore entree exception", ex);
                            Debug.WriteLine($"[RecycleBinService] Exception auto-syncing restored entree: {ex.Message}");
                            _logger?.LogWarning(ex, "Failed to auto-sync restored entree to Supabase");
                        }
                    }

                    break;
                default:
                    return null;
            }

            // Remove from recycle bin
            await _deletedItemRepository.DeleteAsync(deletedItemId);

            return entity;
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            System.Diagnostics.Debug.WriteLine($"Error restoring item: {ex.Message}");
            return null;
        }
    }

    public async Task PermanentlyDeleteAsync(Guid deletedItemId)
    {
        // Get the deleted item to find the original entity ID
        var deletedItem = await _deletedItemRepository.GetByIdAsync(deletedItemId);
        if (deletedItem == null)
            return;

        // Clean up LocalModification records for this entity before deleting from recycle bin
        // This prevents foreign key constraint violations
        await CleanupModificationRecordsAsync(deletedItem.ItemId, deletedItem.ItemType);

        // Now delete from recycle bin
        await _deletedItemRepository.DeleteAsync(deletedItemId);
    }

    private async Task CleanupModificationRecordsAsync(Guid entityId, DeletedItemType itemType)
    {
        // Map DeletedItemType to entity type string
        string entityType = itemType switch
        {
            DeletedItemType.Ingredient => "Ingredient",
            DeletedItemType.Recipe => "Recipe",
            DeletedItemType.Entree => "Entree",
            _ => throw new ArgumentException($"Unknown item type: {itemType}")
        };

        // Clean up all modification records for this entity
        await _localModificationService.ClearEntityModificationsAsync(entityType, entityId);
    }

    public async Task<List<DeletedItem>> GetDeletedItemsAsync(Guid locationId)
    {
        var items = await _deletedItemRepository.GetAllAsync(locationId);
        return items.ToList();
    }

    public async Task<List<DeletedItem>> GetDeletedItemsByTypeAsync(Guid locationId, DeletedItemType type)
    {
        var items = await _deletedItemRepository.GetByTypeAsync(locationId, type);
        return items.ToList();
    }

    public async Task CleanupExpiredItemsAsync()
    {
        await _deletedItemRepository.DeleteExpiredAsync();
    }

    public async Task EmptyRecycleBinAsync(Guid locationId)
    {
        var items = await _deletedItemRepository.GetAllAsync(locationId);
        foreach (var item in items)
        {
            await _deletedItemRepository.DeleteAsync(item.Id);
        }
    }
}
