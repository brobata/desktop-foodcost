using Dfc.Core.Models;
using Dfc.Core.Repositories;

namespace Dfc.Core.Services;

public interface IDonationReminderService
{
    Task<bool> ShouldShowDonationReminderAsync();
    Task IncrementLaunchCountAsync();
    Task<DonationStats> GetDonationStatsAsync();
}

public class DonationReminderService : IDonationReminderService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;
    private readonly IUserSessionService _userSessionService;
    private readonly ICurrentLocationService _currentLocationService;

    // Configuration
    private const int FIRST_REMINDER_LAUNCH = 1; // Show on first launch
    private const int REMINDER_INTERVAL = 10; // Show every 10 launches after first

    public DonationReminderService(
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IEntreeRepository entreeRepository,
        IUserSessionService userSessionService,
        ICurrentLocationService currentLocationService)
    {
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;
        _userSessionService = userSessionService;
        _currentLocationService = currentLocationService;
    }

    public async Task<bool> ShouldShowDonationReminderAsync()
    {
        try
        {
            // Don't show if user is logged in to Supabase (they're a supporter!)
            var isAuthenticated = _userSessionService.IsAuthenticated;
            if (isAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[DonationReminder] User is authenticated - skipping reminder");
                return false;
            }

            // Get launch count from app settings
            var launchCount = GetLaunchCount();
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Launch count: {launchCount}");

            // Show on first launch
            if (launchCount == FIRST_REMINDER_LAUNCH)
            {
                System.Diagnostics.Debug.WriteLine("[DonationReminder] First launch - showing reminder");
                return true;
            }

            // Show every 10 launches after that
            if (launchCount > FIRST_REMINDER_LAUNCH && (launchCount % REMINDER_INTERVAL) == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[DonationReminder] Launch {launchCount} - showing reminder (interval: {REMINDER_INTERVAL})");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Error checking if should show: {ex.Message}");
            return false;
        }
    }

    public Task IncrementLaunchCountAsync()
    {
        try
        {
            var count = GetLaunchCount();
            count++;
            SaveLaunchCount(count);
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Launch count incremented to: {count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Error incrementing launch count: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public async Task<DonationStats> GetDonationStatsAsync()
    {
        try
        {
            var currentLocationId = _currentLocationService.CurrentLocationId;

            var ingredients = await _ingredientRepository.GetAllAsync(currentLocationId);
            var recipes = await _recipeRepository.GetAllRecipesAsync(currentLocationId);
            var entrees = await _entreeRepository.GetAllAsync(currentLocationId);

            return new DonationStats
            {
                IngredientCount = ingredients.Count,
                RecipeCount = recipes.Count(),
                EntreeCount = entrees.Count(),
                LaunchCount = GetLaunchCount()
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Error getting stats: {ex.Message}");
            return new DonationStats();
        }
    }

    private int GetLaunchCount()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost",
            "settings.json");

        if (!File.Exists(appDataPath))
        {
            return 0;
        }

        try
        {
            var json = File.ReadAllText(appDataPath);
            var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
            return settings?.LaunchCount ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void SaveLaunchCount(int count)
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost");

        Directory.CreateDirectory(appDataFolder);

        var appDataPath = Path.Combine(appDataFolder, "settings.json");

        AppSettings settings;
        if (File.Exists(appDataPath))
        {
            try
            {
                var json = File.ReadAllText(appDataPath);
                settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                settings = new AppSettings();
            }
        }
        else
        {
            settings = new AppSettings();
        }

        settings.LaunchCount = count;

        var updatedJson = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(appDataPath, updatedJson);
    }

    private class AppSettings
    {
        public int LaunchCount { get; set; }
    }
}

public class DonationStats
{
    public int IngredientCount { get; set; }
    public int RecipeCount { get; set; }
    public int EntreeCount { get; set; }
    public int LaunchCount { get; set; }
}
