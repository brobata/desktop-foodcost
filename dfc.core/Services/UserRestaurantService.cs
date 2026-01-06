using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing user-restaurant relationships
/// (Firebase removed - stub returns empty data)
/// </summary>
public class UserRestaurantService : IUserRestaurantService
{
    private readonly ILogger<UserRestaurantService>? _logger;

    public UserRestaurantService(ILogger<UserRestaurantService>? logger = null)
    {
        _logger = logger;
    }

    public async Task<List<string>> GetUserRestaurantsAsync(string supabaseAuthUid)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Firebase removed - returning empty list");
        return new List<string>();
    }

    public async Task<List<string>> GetAllRestaurantsAsync()
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Firebase removed - returning empty list");
        return new List<string>();
    }

    public async Task<bool> AddUserToRestaurantAsync(string supabaseAuthUid, string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Firebase removed - operation skipped");
        return false;
    }

    public async Task<bool> RemoveUserFromRestaurantAsync(string supabaseAuthUid, string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Firebase removed - operation skipped");
        return false;
    }

    public async Task<List<string>> GetRestaurantUsersAsync(string restaurantId)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[UserRestaurantService] Firebase removed - returning empty list");
        return new List<string>();
    }
}
