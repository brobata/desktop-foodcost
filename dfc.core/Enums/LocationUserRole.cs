namespace Dfc.Core.Enums;

/// <summary>
/// Role-based access control for location users
/// Defines what permissions a user has for a specific location
/// </summary>
public enum LocationUserRole
{
    /// <summary>
    /// Read-only access to all data in the location
    /// Cannot modify anything
    /// </summary>
    Viewer = 0,

    /// <summary>
    /// Can view and edit recipes and entrees
    /// Cannot edit ingredients or manage users
    /// </summary>
    Chef = 1,

    /// <summary>
    /// Can view, add, edit, and delete ingredients, recipes, and entrees
    /// Cannot manage users or delete the location
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Full access to location data and can manage user access
    /// Cannot delete the location (only owner can)
    /// </summary>
    Admin = 3
}
