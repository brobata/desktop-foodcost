using Dfc.Core.Models;

namespace Dfc.Core.Services;

public interface IIngredientService
{
    Task<List<Ingredient>> GetAllIngredientsAsync(Guid locationId);
    Task<Ingredient?> GetIngredientByIdAsync(Guid id);
    Task<Ingredient?> GetIngredientBySkuAsync(string sku, Guid locationId);
    Task<Ingredient> CreateIngredientAsync(Ingredient ingredient);
    Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
    Task DeleteIngredientAsync(Guid id);
    Task<List<Ingredient>> SearchIngredientsAsync(string searchTerm, Guid locationId);
}