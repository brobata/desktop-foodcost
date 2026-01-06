using Dfc.Core.Models;

namespace Dfc.Core.Repositories;

public interface ILocationUserRepository
{
    Task<List<LocationUser>> GetByLocationIdAsync(Guid locationId);
    Task<LocationUser?> GetByIdAsync(Guid id);
    Task<LocationUser?> GetByLocationAndUserAsync(Guid locationId, string userId);
    Task<LocationUser> AddAsync(LocationUser locationUser);
    Task UpdateAsync(LocationUser locationUser);
    Task DeleteAsync(Guid id);
    Task DeleteByLocationIdAsync(Guid locationId);
}
