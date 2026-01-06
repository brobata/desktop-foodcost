using Freecost.Core.Models;

namespace Freecost.Core.Repositories;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(Guid id);
    Task<Location> AddAsync(Location location);
    Task<Location> UpdateAsync(Location location);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> SaveChangesAsync();
}
