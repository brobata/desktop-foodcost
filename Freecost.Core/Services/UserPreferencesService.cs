using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly IUserPreferencesRepository _repository;

    public UserPreferencesService(IUserPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferences> GetPreferencesAsync(Guid userId)
    {
        var preferences = await _repository.GetByUserIdAsync(userId);

        // Create default preferences if none exist
        if (preferences == null)
        {
            preferences = new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(preferences);
        }

        return preferences;
    }

    public async Task UpdatePreferencesAsync(UserPreferences preferences)
    {
        await _repository.UpdateAsync(preferences);
    }

    public async Task ResetToDefaultsAsync(Guid userId)
    {
        var preferences = await GetPreferencesAsync(userId);

        preferences.Theme = "Light";
        preferences.Language = "en-US";
        preferences.PageSize = 50;
        preferences.DateFormat = "MM/dd/yyyy";
        preferences.TimeFormat = "12h";
        preferences.CurrencySymbol = "$";
        preferences.EmailNotifications = true;
        preferences.PriceAlertNotifications = true;
        preferences.ApprovalNotifications = true;
        preferences.CommentNotifications = true;
        preferences.DefaultDashboard = "Executive Summary";
        preferences.DefaultView = "Dashboard";
        preferences.DefaultRecipeSort = "Name";
        preferences.DefaultRecipeFilter = "All";
        preferences.ShowRecipeCosts = true;
        preferences.ShowNutritionalInfo = true;
        preferences.DefaultExportFormat = "Excel";
        preferences.AutoSaveEnabled = true;
        preferences.AutoSaveIntervalSeconds = 30;

        await UpdatePreferencesAsync(preferences);
    }

    public async Task UpdatePreferenceAsync(Guid userId, string key, string value)
    {
        var preferences = await GetPreferencesAsync(userId);

        // Use reflection to set the property dynamically
        var property = typeof(UserPreferences).GetProperty(key);
        if (property != null && property.CanWrite)
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(preferences, value);
            }
            else if (property.PropertyType == typeof(int))
            {
                if (int.TryParse(value, out var intValue))
                {
                    property.SetValue(preferences, intValue);
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                if (bool.TryParse(value, out var boolValue))
                {
                    property.SetValue(preferences, boolValue);
                }
            }

            await UpdatePreferencesAsync(preferences);
        }
    }
}
