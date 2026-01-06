using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface ICustomDashboardService
{
    /// <summary>
    /// Create a new custom dashboard
    /// </summary>
    Task<CustomDashboard> CreateDashboardAsync(CustomDashboard dashboard);

    /// <summary>
    /// Get all dashboards
    /// </summary>
    Task<List<CustomDashboard>> GetAllDashboardsAsync();

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    Task<CustomDashboard?> GetDashboardByIdAsync(Guid id);

    /// <summary>
    /// Get default dashboard
    /// </summary>
    Task<CustomDashboard?> GetDefaultDashboardAsync();

    /// <summary>
    /// Update dashboard
    /// </summary>
    Task UpdateDashboardAsync(CustomDashboard dashboard);

    /// <summary>
    /// Delete dashboard
    /// </summary>
    Task DeleteDashboardAsync(Guid id);

    /// <summary>
    /// Set dashboard as default
    /// </summary>
    Task SetDefaultDashboardAsync(Guid id);

    /// <summary>
    /// Get pre-defined dashboard templates
    /// </summary>
    Task<List<CustomDashboard>> GetDashboardTemplatesAsync();
}
