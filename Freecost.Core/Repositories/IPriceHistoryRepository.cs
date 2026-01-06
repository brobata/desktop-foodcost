using Freecost.Core.Models;
using Freecost.Core.Enums;

namespace Freecost.Core.Repositories;

public interface IPriceHistoryRepository
{
    Task<List<PriceHistory>> GetByIngredientIdAsync(Guid ingredientId);
    Task<PriceHistory?> GetLatestForIngredientAsync(Guid ingredientId);
    Task<List<PriceHistory>> GetAllUnaggregatedAsync();
    Task AddAsync(PriceHistory priceHistory);
    Task AddRangeAsync(List<PriceHistory> priceHistories);
    Task UpdateAsync(PriceHistory priceHistory);
    Task DeleteAsync(Guid id);
    Task DeleteRangeAsync(List<PriceHistory> priceHistories);
    Task<int> SaveChangesAsync();
}
