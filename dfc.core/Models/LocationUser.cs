using Dfc.Core.Enums;

namespace Dfc.Core.Models;

/// <summary>
/// Junction table entity for many-to-many relationship between users and locations
/// Enables multiple users to access the same location with different permission levels
/// </summary>
public class LocationUser : BaseEntity
{
    /// <summary>
    /// Foreign key to the location being shared
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// ID of the user being granted access
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Role/permission level for this user at this location
    /// - Viewer: Read-only access
    /// - Chef: Can edit recipes/entrees
    /// - Manager: Can CRUD ingredients/recipes/entrees
    /// - Admin: Can manage location settings and add/remove users
    /// </summary>
    public LocationUserRole Role { get; set; } = LocationUserRole.Viewer;

    // Navigation property
    public Location? Location { get; set; }
}
