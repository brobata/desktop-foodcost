using Dfc.Core.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IUserPreferencesService
{
    /// <summary>
    /// Get user preferences by user ID
    /// </summary>
    Task<UserPreferences> GetPreferencesAsync(Guid userId);

    /// <summary>
    /// Update user preferences
    /// </summary>
    Task UpdatePreferencesAsync(UserPreferences preferences);

    /// <summary>
    /// Reset user preferences to defaults
    /// </summary>
    Task ResetToDefaultsAsync(Guid userId);

    /// <summary>
    /// Update a specific preference
    /// </summary>
    Task UpdatePreferenceAsync(Guid userId, string key, string value);
}
