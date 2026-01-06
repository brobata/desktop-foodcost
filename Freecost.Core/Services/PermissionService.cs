using Freecost.Core.Enums;
using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class PermissionService : IPermissionService
{
    private static readonly Dictionary<UserRole, PermissionSet> _rolePermissions = new()
    {
        [UserRole.Admin] = new PermissionSet
        {
            Role = UserRole.Admin,
            Permissions = Enum.GetValues<Permission>().ToList() // Admin has all permissions
        },
        [UserRole.Manager] = new PermissionSet
        {
            Role = UserRole.Manager,
            Permissions = new List<Permission>
            {
                // View permissions
                Permission.ViewIngredients,
                Permission.ViewRecipes,
                Permission.ViewEntrees,
                Permission.ViewReports,
                Permission.ViewUsers,
                Permission.ViewSettings,
                Permission.ViewAuditLog,

                // Create permissions
                Permission.CreateIngredients,
                Permission.CreateRecipes,
                Permission.CreateEntrees,
                Permission.CreateReports,

                // Edit permissions
                Permission.EditIngredients,
                Permission.EditRecipes,
                Permission.EditEntrees,
                Permission.EditSettings,

                // Delete permissions
                Permission.DeleteIngredients,
                Permission.DeleteRecipes,
                Permission.DeleteEntrees,

                // Special permissions
                Permission.ApproveChanges,
                Permission.ExportData,
                Permission.ImportData,
                Permission.ViewAuditLog
            }
        },
        [UserRole.Chef] = new PermissionSet
        {
            Role = UserRole.Chef,
            Permissions = new List<Permission>
            {
                // View permissions
                Permission.ViewIngredients,
                Permission.ViewRecipes,
                Permission.ViewEntrees,
                Permission.ViewReports,

                // Create permissions
                Permission.CreateIngredients,
                Permission.CreateRecipes,
                Permission.CreateEntrees,

                // Edit permissions
                Permission.EditIngredients,
                Permission.EditRecipes,
                Permission.EditEntrees,

                // Limited delete (can delete own items only - would need additional logic)
                Permission.DeleteIngredients,
                Permission.DeleteRecipes,
                Permission.DeleteEntrees,

                // Special permissions
                Permission.ExportData
            }
        },
        [UserRole.Viewer] = new PermissionSet
        {
            Role = UserRole.Viewer,
            Permissions = new List<Permission>
            {
                // View permissions only
                Permission.ViewIngredients,
                Permission.ViewRecipes,
                Permission.ViewEntrees,
                Permission.ViewReports,

                // Export (read-only export)
                Permission.ExportData
            }
        }
    };

    public async Task<bool> HasPermissionAsync(User user, Permission permission)
    {
        if (user == null)
        {
            return false;
        }

        var permissionSet = await GetPermissionsForRoleAsync(user.Role);
        return permissionSet.HasPermission(permission);
    }

    public async Task<bool> CanViewAsync(User user, string entityType)
    {
        var permission = entityType.ToLowerInvariant() switch
        {
            "ingredient" or "ingredients" => Permission.ViewIngredients,
            "recipe" or "recipes" => Permission.ViewRecipes,
            "entree" or "entrees" => Permission.ViewEntrees,
            "report" or "reports" => Permission.ViewReports,
            "user" or "users" => Permission.ViewUsers,
            "settings" => Permission.ViewSettings,
            "auditlog" => Permission.ViewAuditLog,
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };

        return await HasPermissionAsync(user, permission);
    }

    public async Task<bool> CanCreateAsync(User user, string entityType)
    {
        var permission = entityType.ToLowerInvariant() switch
        {
            "ingredient" or "ingredients" => Permission.CreateIngredients,
            "recipe" or "recipes" => Permission.CreateRecipes,
            "entree" or "entrees" => Permission.CreateEntrees,
            "report" or "reports" => Permission.CreateReports,
            "user" or "users" => Permission.CreateUsers,
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };

        return await HasPermissionAsync(user, permission);
    }

    public async Task<bool> CanEditAsync(User user, string entityType)
    {
        var permission = entityType.ToLowerInvariant() switch
        {
            "ingredient" or "ingredients" => Permission.EditIngredients,
            "recipe" or "recipes" => Permission.EditRecipes,
            "entree" or "entrees" => Permission.EditEntrees,
            "settings" => Permission.EditSettings,
            "user" or "users" => Permission.EditUsers,
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };

        return await HasPermissionAsync(user, permission);
    }

    public async Task<bool> CanDeleteAsync(User user, string entityType)
    {
        var permission = entityType.ToLowerInvariant() switch
        {
            "ingredient" or "ingredients" => Permission.DeleteIngredients,
            "recipe" or "recipes" => Permission.DeleteRecipes,
            "entree" or "entrees" => Permission.DeleteEntrees,
            "user" or "users" => Permission.DeleteUsers,
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };

        return await HasPermissionAsync(user, permission);
    }

    public async Task<bool> CanApproveAsync(User user)
    {
        return await HasPermissionAsync(user, Permission.ApproveChanges);
    }

    public async Task<bool> CanManageUsersAsync(User user)
    {
        return await HasPermissionAsync(user, Permission.ManageUsers);
    }

    public async Task<bool> CanManageSettingsAsync(User user)
    {
        return await HasPermissionAsync(user, Permission.ManageSettings);
    }

    public async Task<bool> CanExportDataAsync(User user)
    {
        return await HasPermissionAsync(user, Permission.ExportData);
    }

    public async Task<PermissionSet> GetPermissionsForRoleAsync(UserRole role)
    {
        return await Task.FromResult(_rolePermissions.GetValueOrDefault(role, new PermissionSet
        {
            Role = role,
            Permissions = new List<Permission>()
        }));
    }
}
