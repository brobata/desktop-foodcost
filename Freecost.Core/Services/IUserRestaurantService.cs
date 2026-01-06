using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

/// <summary>
/// Service for managing user-restaurant relationships
/// (Firebase removed - stub for compatibility)
/// </summary>
public interface IUserRestaurantService
{
    Task<List<string>> GetUserRestaurantsAsync(string supabaseAuthUid);
    Task<List<string>> GetAllRestaurantsAsync();
    Task<bool> AddUserToRestaurantAsync(string supabaseAuthUid, string restaurantId);
    Task<bool> RemoveUserFromRestaurantAsync(string supabaseAuthUid, string restaurantId);
    Task<List<string>> GetRestaurantUsersAsync(string restaurantId);
}
