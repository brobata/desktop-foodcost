using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing user-restaurant relationships (local-only mode)
/// Returns empty data as this is a local-only application
/// </summary>
public class UserRestaurantService : IUserRestaurantService
{
    private readonly ILogger<UserRestaurantService>? _logger;

    public UserRestaurantService(ILogger<UserRestaurantService>? logger = null)
    {
        _logger = logger;
    }

    public async Task<List<string>> GetUserRestaurantsAsync(string userId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Local-only mode - returning empty list");
        return new List<string>();
    }

    public async Task<List<string>> GetAllRestaurantsAsync()
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Local-only mode - returning empty list");
        return new List<string>();
    }

    public async Task<bool> AddUserToRestaurantAsync(string userId, string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Local-only mode - operation skipped");
        return false;
    }

    public async Task<bool> RemoveUserFromRestaurantAsync(string userId, string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Local-only mode - operation skipped");
        return false;
    }

    public async Task<List<string>> GetRestaurantUsersAsync(string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Local-only mode - returning empty list");
        return new List<string>();
    }
}
