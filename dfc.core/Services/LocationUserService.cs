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
/// Handles sync between local SQLite and Supabase
/// </summary>
public class LocationUserService : ILocationUserService
{
    private readonly ILocationUserRepository _localRepo;
    private readonly SupabaseDataService _supabaseService;

    public LocationUserService(
        ILocationUserRepository localRepo,
        SupabaseDataService supabaseService)
    {
        _localRepo = localRepo;
        _supabaseService = supabaseService;
    }

    /// <summary>
    /// Get all users who have access to a location
    /// </summary>
    public async Task<List<LocationUser>> GetLocationUsersAsync(Guid locationId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Getting users for location {locationId}");

            // Get from local DB
            var localUsers = await _localRepo.GetByLocationIdAsync(locationId);

            Debug.WriteLine($"[LocationUserService] Found {localUsers.Count} users in local DB");
            return localUsers;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] ❌ Error getting location users: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all locations a user has access to (not currently used, but available for future)
    /// </summary>
    public async Task<List<Location>> GetUserLocationsAsync(string userId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Getting locations for user {userId}");

            // Get location users from local DB
            var locationUsers = await _localRepo.GetByLocationIdAsync(Guid.Empty); // This would need a new method
            var userLocationUsers = locationUsers.Where(lu => lu.UserId == userId).ToList();

            // Extract locations from navigation properties
            var locations = userLocationUsers
                .Where(lu => lu.Location != null)
                .Select(lu => lu.Location!)
                .ToList();

            Debug.WriteLine($"[LocationUserService] Found {locations.Count} locations for user");
            return locations;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] ❌ Error getting user locations: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Grant a user access to a location with a specific role
    /// Syncs to both local DB and Supabase
    /// </summary>
    public async Task AddUserToLocationAsync(Guid locationId, string userId, LocationUserRole role)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Adding user {userId} to location {locationId} with role {role}");

            // Check if already exists
            var existing = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (existing != null)
            {
                Debug.WriteLine($"[LocationUserService] User already has access to this location");
                throw new InvalidOperationException("User already has access to this location");
            }

            // Create LocationUser
            var locationUser = new LocationUser
            {
                Id = Guid.NewGuid(),
                LocationId = locationId,
                UserId = userId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            // 1. Add to local DB
            await _localRepo.AddAsync(locationUser);
            Debug.WriteLine($"[LocationUserService] ✓ Added to local DB");

            // 2. Sync to Supabase
            var supabaseLocationUser = new SupabaseLocationUser
            {
                Id = locationUser.Id,
                LocationId = locationUser.LocationId,
                UserId = locationUser.UserId,
                Role = role.ToString().ToLower(), // Store as "viewer", "chef", "manager", "admin"
                CreatedAt = locationUser.CreatedAt,
                ModifiedAt = locationUser.ModifiedAt
            };

            var result = await _supabaseService.AddLocationUserAsync(supabaseLocationUser);

            if (result.IsSuccess)
            {
                Debug.WriteLine($"[LocationUserService] ✓ Synced to Supabase");
            }
            else
            {
                Debug.WriteLine($"[LocationUserService] ⚠ Supabase sync failed: {result.Error}");
                // Local DB still has it, sync will happen later
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] ❌ Error adding user to location: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Remove a user's access to a location
    /// Syncs to both local DB and Supabase
    /// </summary>
    public async Task RemoveUserFromLocationAsync(Guid locationId, string userId)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Removing user {userId} from location {locationId}");

            // Find the LocationUser
            var locationUser = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (locationUser == null)
            {
                Debug.WriteLine($"[LocationUserService] User does not have access to this location");
                throw new InvalidOperationException("User does not have access to this location");
            }

            // 1. Remove from local DB
            await _localRepo.DeleteAsync(locationUser.Id);
            Debug.WriteLine($"[LocationUserService] ✓ Removed from local DB");

            // 2. Sync to Supabase
            var result = await _supabaseService.DeleteLocationUserAsync(locationUser.Id);

            if (result.IsSuccess)
            {
                Debug.WriteLine($"[LocationUserService] ✓ Synced deletion to Supabase");
            }
            else
            {
                Debug.WriteLine($"[LocationUserService] ⚠ Supabase deletion failed: {result.Error}");
                // Local DB already deleted it, should be fine
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] ❌ Error removing user from location: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update a user's role for a location
    /// Syncs to both local DB and Supabase
    /// </summary>
    public async Task UpdateUserRoleAsync(Guid locationId, string userId, LocationUserRole newRole)
    {
        try
        {
            Debug.WriteLine($"[LocationUserService] Updating user {userId} role to {newRole} for location {locationId}");

            // Find the LocationUser
            var locationUser = await _localRepo.GetByLocationAndUserAsync(locationId, userId);
            if (locationUser == null)
            {
                Debug.WriteLine($"[LocationUserService] User does not have access to this location");
                throw new InvalidOperationException("User does not have access to this location");
            }

            // Update role
            locationUser.Role = newRole;
            locationUser.ModifiedAt = DateTime.UtcNow;

            // 1. Update in local DB
            await _localRepo.UpdateAsync(locationUser);
            Debug.WriteLine($"[LocationUserService] ✓ Updated in local DB");

            // 2. Sync to Supabase
            var supabaseLocationUser = new SupabaseLocationUser
            {
                Id = locationUser.Id,
                LocationId = locationUser.LocationId,
                UserId = locationUser.UserId,
                Role = newRole.ToString().ToLower(),
                CreatedAt = locationUser.CreatedAt,
                ModifiedAt = locationUser.ModifiedAt
            };

            var result = await _supabaseService.UpdateLocationUserAsync(supabaseLocationUser);

            if (result.IsSuccess)
            {
                Debug.WriteLine($"[LocationUserService] ✓ Synced update to Supabase");
            }
            else
            {
                Debug.WriteLine($"[LocationUserService] ⚠ Supabase update failed: {result.Error}");
                // Local DB updated, sync will happen later
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationUserService] ❌ Error updating user role: {ex.Message}");
            throw;
        }
    }
}
