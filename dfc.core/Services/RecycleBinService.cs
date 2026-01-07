using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class RecycleBinService : IRecycleBinService
{
    private readonly IDeletedItemRepository _deletedItemRepository;
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;
    private readonly ILocalModificationService _localModificationService;
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
        ILogger<RecycleBinService>? logger = null)
    {
        _deletedItemRepository = deletedItemRepository;
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;
        _localModificationService = localModificationService;
        _logger = logger;
    }

    public async Task<DeletedItem> MoveToRecycleBinAsync<T>(T entity, DeletedItemType type, Guid locationId) where T : BaseEntity
    {
        try
        {
            Debug.WriteLine($"  [RECYCLE BIN] Starting MoveToRecycleBinAsync");
            Debug.WriteLine($"  [RECYCLE BIN] Entity Type: {typeof(T).Name}");
            Debug.WriteLine($"  [RECYCLE BIN] Item Type: {type}");
            Debug.WriteLine($"  [RECYCLE BIN] Location ID: {locationId}");
            Debug.WriteLine($"  [RECYCLE BIN] Entity ID: {entity.Id}");

            string itemName = type switch
            {
                DeletedItemType.Ingredient => (entity as Ingredient)?.Name ?? "Unknown",
                DeletedItemType.Recipe => (entity as Recipe)?.Name ?? "Unknown",
                DeletedItemType.Entree => (entity as Entree)?.Name ?? "Unknown",
                _ => "Unknown"
            };
            Debug.WriteLine($"  [RECYCLE BIN] Item Name: {itemName}");

            // Clear navigation properties based on type to avoid serialization issues
            switch (type)
            {
                case DeletedItemType.Ingredient when entity is Ingredient ingredient:
                    ingredient.Location = null!;
                    ingredient.PriceHistory = new List<PriceHistory>();
                    ingredient.RecipeIngredients = new List<RecipeIngredient>();
                    ingredient.EntreeIngredients = new List<EntreeIngredient>();

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

            Debug.WriteLine($"  [RECYCLE BIN] Clearing navigation properties complete");

            Debug.WriteLine($"  [RECYCLE BIN] Serializing entity to JSON...");
            var serializedData = JsonSerializer.Serialize(entity, _jsonOptions);
            Debug.WriteLine($"  [RECYCLE BIN] Serialization complete. Length: {serializedData.Length} chars");

            var deletedItem = new DeletedItem
            {
                ItemId = entity.Id,
                ItemType = type,
                ItemName = itemName,
                SerializedData = serializedData,
                LocationId = locationId,
                DeletedDate = DateTime.UtcNow
            };
            Debug.WriteLine($"  [RECYCLE BIN] DeletedItem created. ItemId: {deletedItem.ItemId}");

            Debug.WriteLine($"  [RECYCLE BIN] Adding to DeletedItemRepository...");
            var result = await _deletedItemRepository.AddAsync(deletedItem);
            Debug.WriteLine($"  [RECYCLE BIN] Successfully added. Result Id: {result.Id}");

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
            Debug.WriteLine("  ║ [RECYCLE BIN EXCEPTION]                           ║");
            Debug.WriteLine("  ╠═══════════════════════════════════════════════════╣");
            Debug.WriteLine($"  Exception Type: {ex.GetType().Name}");
            Debug.WriteLine($"  Message: {ex.Message}");
            Debug.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                Debug.WriteLine($"  Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            Debug.WriteLine("  ╚═══════════════════════════════════════════════════╝");
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
                    ingredient.Location = null!;
                    ingredient.RecipeIngredients?.Clear();
                    ingredient.EntreeIngredients?.Clear();
                    ingredient.PriceHistory?.Clear();

                    if (ingredient.IngredientAllergens != null)
                    {
                        foreach (var allergen in ingredient.IngredientAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.IngredientId = Guid.Empty;
                            allergen.Ingredient = null!;
                            allergen.Allergen = null!;
                        }
                    }

                    await _ingredientRepository.AddAsync(ingredient);
                    Debug.WriteLine($"[RecycleBinService] Restored ingredient '{ingredient.Name}'");
                    break;

                case DeletedItemType.Recipe when entity is Recipe recipe:
                    recipe.Location = null!;

                    if (recipe.RecipeAllergens != null)
                    {
                        foreach (var allergen in recipe.RecipeAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.RecipeId = Guid.Empty;
                            allergen.Recipe = null!;
                            allergen.Allergen = null!;
                        }
                    }

                    if (recipe.RecipeIngredients != null)
                    {
                        foreach (var ingredient in recipe.RecipeIngredients)
                        {
                            ingredient.Id = Guid.NewGuid();
                            ingredient.RecipeId = Guid.Empty;
                            ingredient.Recipe = null!;
                            ingredient.Ingredient = null!;
                        }
                    }

                    recipe.EntreeRecipes?.Clear();
                    recipe.Photos?.Clear();

                    await _recipeRepository.CreateRecipeAsync(recipe);
                    Debug.WriteLine($"[RecycleBinService] Restored recipe '{recipe.Name}'");
                    break;

                case DeletedItemType.Entree when entity is Entree entree:
                    entree.Location = null!;

                    if (entree.EntreeAllergens != null)
                    {
                        foreach (var allergen in entree.EntreeAllergens)
                        {
                            allergen.Id = Guid.NewGuid();
                            allergen.EntreeId = Guid.Empty;
                            allergen.Entree = null!;
                            allergen.Allergen = null!;
                        }
                    }

                    if (entree.EntreeRecipes != null)
                    {
                        foreach (var recipe in entree.EntreeRecipes)
                        {
                            recipe.Id = Guid.NewGuid();
                            recipe.EntreeId = Guid.Empty;
                            recipe.Entree = null!;
                            recipe.Recipe = null!;
                        }
                    }

                    if (entree.EntreeIngredients != null)
                    {
                        foreach (var ingredient in entree.EntreeIngredients)
                        {
                            ingredient.Id = Guid.NewGuid();
                            ingredient.EntreeId = Guid.Empty;
                            ingredient.Entree = null!;
                            ingredient.Ingredient = null!;
                        }
                    }

                    entree.Photos?.Clear();

                    await _entreeRepository.CreateAsync(entree);
                    Debug.WriteLine($"[RecycleBinService] Restored entree '{entree.Name}'");
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
            Debug.WriteLine($"Error restoring item: {ex.Message}");
            return null;
        }
    }

    public async Task PermanentlyDeleteAsync(Guid deletedItemId)
    {
        var deletedItem = await _deletedItemRepository.GetByIdAsync(deletedItemId);
        if (deletedItem == null)
            return;

        await CleanupModificationRecordsAsync(deletedItem.ItemId, deletedItem.ItemType);
        await _deletedItemRepository.DeleteAsync(deletedItemId);
    }

    private async Task CleanupModificationRecordsAsync(Guid entityId, DeletedItemType itemType)
    {
        string entityType = itemType switch
        {
            DeletedItemType.Ingredient => "Ingredient",
            DeletedItemType.Recipe => "Recipe",
            DeletedItemType.Entree => "Entree",
            _ => throw new ArgumentException($"Unknown item type: {itemType}")
        };

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
