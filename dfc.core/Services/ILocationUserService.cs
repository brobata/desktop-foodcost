using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.Core.Enums;
using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing user access to locations (multi-user location sharing)
/// Local-only implementation
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
    /// </summary>
    Task AddUserToLocationAsync(Guid locationId, string userId, LocationUserRole role);

    /// <summary>
    /// Remove a user's access to a location
    /// </summary>
    Task RemoveUserFromLocationAsync(Guid locationId, string userId);

    /// <summary>
    /// Update a user's role for a location
    /// </summary>
    Task UpdateUserRoleAsync(Guid locationId, string userId, LocationUserRole newRole);
}
