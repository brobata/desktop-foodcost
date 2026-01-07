using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Repositories;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing user access to locations (multi-user location sharing)
/// Local-only implementation
/// </summary>
public class LocationUserService : ILocationUserService
{
    private readonly ILocationUserRepository _localRepo;

    public LocationUserService(ILocationUserRepository localRepo)
    {
        _localRepo = localRepo;
    }

    /// <summary>
    /// Get all users who have access to a location
    /// </summary>
    public async Task<List<LocationUser>> GetLocationUsersAsync(Guid locationId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Getting users for location {locationId}");

            var localUsers = await _localRepo.GetByLocationIdAsync(locationId);

            Debug.WriteLine($"[LocationUserService] Found {localUsers.Count} users in local DB");
            return localUsers;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] Error getting location users: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all locations a user has access to
    /// </summary>
    public async Task<List<Location>> GetUserLocationsAsync(string userId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Getting locations for user {userId}");

            var locationUsers = await _localRepo.GetByLocationIdAsync(Guid.Empty);
            var userLocationUsers = locationUsers.Where(lu => lu.UserId == userId).ToList();

            var locations = userLocationUsers
                .Where(lu => lu.Location != null)
                .Select(lu => lu.Location!)
                .ToList();

            Debug.WriteLine($"[LocationUserService] Found {locations.Count} locations for user");
            return locations;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] Error getting user locations: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Grant a user access to a location with a specific role
    /// </summary>
    public async Task AddUserToLocationAsync(Guid locationId, string userId, LocationUserRole role)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Adding user {userId} to location {locationId} with role {role}");

            var existing = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (existing != null)
            {
                Debug.WriteLine($"[LocationUserService] User already has access to this location");
                throw new InvalidOperationException("User already has access to this location");
            }

            var locationUser = new LocationUser
            {
                Id = Guid.NewGuid(),
                LocationId = locationId,
                UserId = userId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _localRepo.AddAsync(locationUser);
            Debug.WriteLine($"[LocationUserService] Added to local DB");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] Error adding user to location: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Remove a user's access to a location
    /// </summary>
    public async Task RemoveUserFromLocationAsync(Guid locationId, string userId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Removing user {userId} from location {locationId}");

            var locationUser = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (locationUser == null)
            {
                Debug.WriteLine($"[LocationUserService] User does not have access to this location");
                throw new InvalidOperationException("User does not have access to this location");
            }

            await _localRepo.DeleteAsync(locationUser.Id);
            Debug.WriteLine($"[LocationUserService] Removed from local DB");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] Error removing user from location: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update a user's role for a location
    /// </summary>
    public async Task UpdateUserRoleAsync(Guid locationId, string userId, LocationUserRole newRole)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Updating user {userId} role to {newRole} for location {locationId}");

            var locationUser = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (locationUser == null)
            {
                Debug.WriteLine($"[LocationUserService] User does not have access to this location");
                throw new InvalidOperationException("User does not have access to this location");
            }

            locationUser.Role = newRole;
            locationUser.ModifiedAt = DateTime.UtcNow;

            await _localRepo.UpdateAsync(locationUser);
            Debug.WriteLine($"[LocationUserService] Updated in local DB");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] Error updating user role: {ex.Message}");
            throw;
        }
    }
}
