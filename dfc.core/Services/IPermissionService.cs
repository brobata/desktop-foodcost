using Dfc.Core.Enums;
using Dfc.Core.Models;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IPermissionService
{
    /// <summary>
    /// Check if user has permission to perform an action
    /// </summary>
    Task<bool> HasPermissionAsync(User user, Permission permission);

    /// <summary>
    /// Check if user can view an entity
    /// </summary>
    Task<bool> CanViewAsync(User user, string entityType);

    /// <summary>
    /// Check if user can create an entity
    /// </summary>
    Task<bool> CanCreateAsync(User user, string entityType);

    /// <summary>
    /// Check if user can edit an entity
    /// </summary>
    Task<bool> CanEditAsync(User user, string entityType);

    /// <summary>
    /// Check if user can delete an entity
    /// </summary>
    Task<bool> CanDeleteAsync(User user, string entityType);

    /// <summary>
    /// Check if user can approve changes
    /// </summary>
    Task<bool> CanApproveAsync(User user);

    /// <summary>
    /// Check if user can manage users
    /// </summary>
    Task<bool> CanManageUsersAsync(User user);

    /// <summary>
    /// Check if user can manage settings
    /// </summary>
    Task<bool> CanManageSettingsAsync(User user);

    /// <summary>
    /// Check if user can export data
    /// </summary>
    Task<bool> CanExportDataAsync(User user);

    /// <summary>
    /// Get all permissions for a user role
    /// </summary>
    Task<PermissionSet> GetPermissionsForRoleAsync(UserRole role);
}

public enum Permission
{
    // View permissions
    ViewIngredients,
    ViewRecipes,
    ViewEntrees,
    ViewReports,
    ViewUsers,
    ViewSettings,
    ViewAuditLog,

    // Create permissions
    CreateIngredients,
    CreateRecipes,
    CreateEntrees,
    CreateReports,
    CreateUsers,

    // Edit permissions
    EditIngredients,
    EditRecipes,
    EditEntrees,
    EditSettings,
    EditUsers,

    // Delete permissions
    DeleteIngredients,
    DeleteRecipes,
    DeleteEntrees,
    DeleteUsers,

    // Special permissions
    ApproveChanges,
    ManageUsers,
    ManageSettings,
    ExportData,
    ImportData,
    ManagePermissions
}

public class PermissionSet
{
    public UserRole Role { get; set; }
    public List<Permission> Permissions { get; set; } = new();

    public bool HasPermission(Permission permission)
    {
        return Permissions.Contains(permission);
    }
}
