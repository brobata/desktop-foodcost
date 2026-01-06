using Freecost.Core.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freecost.Desktop.Services;

public interface ILocalSettingsService
{
    Task<UserPreferences> LoadSettingsAsync();
    Task SaveSettingsAsync(UserPreferences preferences);
    Task ResetToDefaultsAsync();
    Task<string?> GetRememberedEmailAsync();
    Task SaveRememberedEmailAsync(string? email);
    Task<string?> GetRememberedPasswordAsync();
    Task SaveRememberedPasswordAsync(string? password);
}

public class LocalSettingsService : ILocalSettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalSettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var freecostFolder = Path.Combine(appDataPath, "Freecost");

        // Create directory if it doesn't exist
        if (!Directory.Exists(freecostFolder))
        {
            Directory.CreateDirectory(freecostFolder);
        }

        _settingsPath = Path.Combine(freecostFolder, "settings.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<UserPreferences> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                // Return default preferences if file doesn't exist
                return CreateDefaultPreferences();
            }

            var json = await File.ReadAllTextAsync(_settingsPath);
            var preferences = JsonSerializer.Deserialize<UserPreferences>(json, _jsonOptions);

            return preferences ?? CreateDefaultPreferences();
        }
        catch (Exception)
        {
            // If any error occurs during loading, return defaults
            return CreateDefaultPreferences();
        }
    }

    public async Task SaveSettingsAsync(UserPreferences preferences)
    {
        try
        {
            var json = JsonSerializer.Serialize(preferences, _jsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
        }
        catch (Exception)
        {
            // Silently fail - could log this in production
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaults = CreateDefaultPreferences();
        await SaveSettingsAsync(defaults);
    }

    public async Task<string?> GetRememberedEmailAsync()
    {
        try
        {
            var credentialsPath = Path.Combine(Path.GetDirectoryName(_settingsPath)!, "credentials.json");
            if (!File.Exists(credentialsPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(credentialsPath);
            var credentials = JsonSerializer.Deserialize<RememberedCredentials>(json, _jsonOptions);
            return credentials?.Email;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveRememberedEmailAsync(string? email)
    {
        try
        {
            var credentialsPath = Path.Combine(Path.GetDirectoryName(_settingsPath)!, "credentials.json");

            // Load existing credentials or create new
            RememberedCredentials? credentials = null;
            if (File.Exists(credentialsPath))
            {
                var json = await File.ReadAllTextAsync(credentialsPath);
                credentials = JsonSerializer.Deserialize<RememberedCredentials>(json, _jsonOptions);
            }

            if (credentials == null)
            {
                credentials = new RememberedCredentials();
            }

            credentials.Email = email;

            // If both email and password are null, delete the file
            if (string.IsNullOrEmpty(credentials.Email) && string.IsNullOrEmpty(credentials.Password))
            {
                if (File.Exists(credentialsPath))
                {
                    File.Delete(credentialsPath);
                }
                return;
            }

            var updatedJson = JsonSerializer.Serialize(credentials, _jsonOptions);
            await File.WriteAllTextAsync(credentialsPath, updatedJson);
        }
        catch
        {
            // Silently fail
        }
    }

    public async Task<string?> GetRememberedPasswordAsync()
    {
        try
        {
            var credentialsPath = Path.Combine(Path.GetDirectoryName(_settingsPath)!, "credentials.json");
            if (!File.Exists(credentialsPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(credentialsPath);
            var credentials = JsonSerializer.Deserialize<RememberedCredentials>(json, _jsonOptions);
            return credentials?.Password;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveRememberedPasswordAsync(string? password)
    {
        try
        {
            var credentialsPath = Path.Combine(Path.GetDirectoryName(_settingsPath)!, "credentials.json");

            // Load existing credentials or create new
            RememberedCredentials? credentials = null;
            if (File.Exists(credentialsPath))
            {
                var json = await File.ReadAllTextAsync(credentialsPath);
                credentials = JsonSerializer.Deserialize<RememberedCredentials>(json, _jsonOptions);
            }

            if (credentials == null)
            {
                credentials = new RememberedCredentials();
            }

            credentials.Password = password;

            // If both email and password are null, delete the file
            if (string.IsNullOrEmpty(credentials.Email) && string.IsNullOrEmpty(credentials.Password))
            {
                if (File.Exists(credentialsPath))
                {
                    File.Delete(credentialsPath);
                }
                return;
            }

            var updatedJson = JsonSerializer.Serialize(credentials, _jsonOptions);
            await File.WriteAllTextAsync(credentialsPath, updatedJson);
        }
        catch
        {
            // Silently fail
        }
    }

    private UserPreferences CreateDefaultPreferences()
    {
        return new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // Single-user default
            CreatedAt = DateTime.UtcNow,
            Theme = "Light",
            Language = "en-US",
            PageSize = 50,
            DateFormat = "MM/dd/yyyy",
            TimeFormat = "12h",
            CurrencySymbol = "$",
            EmailNotifications = true,
            PriceAlertNotifications = true,
            ApprovalNotifications = true,
            CommentNotifications = true,
            DefaultDashboard = "Executive Summary",
            DefaultView = "Dashboard",
            DefaultRecipeSort = "Name",
            DefaultRecipeFilter = "All",
            ShowRecipeCosts = true,
            ShowNutritionalInfo = true,
            DefaultExportFormat = "Excel",
            AutoSaveEnabled = true,
            AutoSaveIntervalSeconds = 30
        };
    }

    private class RememberedCredentials
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
