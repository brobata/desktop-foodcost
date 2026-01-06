using Dfc.Core.Models;

namespace Dfc.Core.Services;

public interface ILocationService
{
    Task<List<Location>> GetAllLocationsAsync();
    Task<Location?> GetLocationByIdAsync(Guid id);
    Task<Location> CreateLocationAsync(Location location);
    Task<Location> UpdateLocationAsync(Location location);
    Task DeleteLocationAsync(Guid id);
}
