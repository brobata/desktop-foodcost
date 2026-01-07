using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing user-restaurant relationships (local-only mode)
/// </summary>
public interface IUserRestaurantService
{
    Task<List<string>> GetUserRestaurantsAsync(string userId);
    Task<List<string>> GetAllRestaurantsAsync();
    Task<bool> AddUserToRestaurantAsync(string userId, string restaurantId);
    Task<bool> RemoveUserFromRestaurantAsync(string userId, string restaurantId);
    Task<List<string>> GetRestaurantUsersAsync(string restaurantId);
}
