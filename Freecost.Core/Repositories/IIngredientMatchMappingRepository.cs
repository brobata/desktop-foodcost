using Freecost.Core.Models;

namespace Freecost.Core.Repositories;

public interface IIngredientMatchMappingRepository
{
    Task<List<IngredientMatchMapping>> GetAllByLocationAsync(Guid locationId);
    Task<IngredientMatchMapping?> GetByIdAsync(Guid id);
    Task<IngredientMatchMapping?> GetByImportNameAsync(string importName, Guid locationId);
    Task<IngredientMatchMapping> AddAsync(IngredientMatchMapping mapping);
    Task<IngredientMatchMapping> UpdateAsync(IngredientMatchMapping mapping);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByImportNameAsync(string importName, Guid locationId);
}
