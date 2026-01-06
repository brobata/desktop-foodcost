using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freecost.Core.Enums;
using Freecost.Core.Models;

namespace Freecost.Core.Services;

/// <summary>
/// Service for managing user access to locations (multi-user location sharing)
/// Handles sync between local SQLite and Supabase
/// </summary>
public interface ILocationUserService
{
    /// <summary>
    /// Get all users who have access to a location
    /// </summary>
    Task<List<LocationUser>> GetLocationUsersAsync(Guid locationId);

    /// <summary>
    /// Get all locations a user has access to
    /// </summary>
    Task<List<Location>> GetUserLocationsAsync(string userId);

    /// <summary>
    /// Grant a user access to a location with a specific role
    /// Syncs to both local DB and Supabase
    /// </summary>
    Task AddUserToLocationAsync(Guid locationId, string userId, LocationUserRole role);

    /// <summary>
    /// Remove a user's access to a location
    /// Syncs to both local DB and Supabase
    /// </summary>
    Task RemoveUserFromLocationAsync(Guid locationId, string userId);

    /// <summary>
    /// Update a user's role for a location
    /// Syncs to both local DB and Supabase
    /// </summary>
    Task UpdateUserRoleAsync(Guid locationId, string userId, LocationUserRole newRole);
}
