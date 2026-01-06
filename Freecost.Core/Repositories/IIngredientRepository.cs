using Freecost.Core.Models;

namespace Freecost.Core.Repositories;

public interface IIngredientRepository
{
    Task<List<Ingredient>> GetAllAsync(Guid locationId);
    Task<Ingredient?> GetByIdAsync(Guid id);
    Task<Ingredient?> GetBySkuAsync(string sku, Guid locationId);
    Task<Ingredient> AddAsync(Ingredient ingredient);
    Task<Ingredient> UpdateAsync(Ingredient ingredient);
    Task DeleteAsync(Guid id);
    Task<List<Ingredient>> SearchAsync(string searchTerm, Guid locationId);
    Task<bool> ExistsAsync(Guid id);
    Task<Ingredient?> GetByNameAsync(string name, Guid locationId);
}