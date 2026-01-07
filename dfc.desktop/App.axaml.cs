// Location: Dfc.Desktop/App.axaml.cs
// Action: REPLACE entire file

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Layout;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Data;
using Dfc.Data.LocalDatabase;
using Dfc.Data.Repositories;
using Dfc.Data.Services;
using Dfc.Desktop.Services;
using Dfc.Desktop.ViewModels;
using Dfc.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Dfc.Core.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Dfc.Desktop;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    private bool _shouldLaunchTutorial = false;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Set EPPlus license for non-commercial use (EPPlus 8+)
        OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("Desktop Food Cost");
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Initialize database AFTER service provider is built
        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DfcDbContext>();

            // logFile variable removed - using SafeFileLogger
            

            SafeFileLogger.Log("startup", "=== DATABASE INITIALIZATION START ===");

            // Migration Safety: Validate and backup before migration
            var migrationSafety = Services?.GetService<Dfc.Data.Services.IMigrationSafetyService>();
            if (migrationSafety != null)
            {
                System.Diagnostics.Debug.WriteLine("Running migration safety checks...");
                SafeFileLogger.Log("startup", "Running migration safety checks...");
                var safetyResult = await migrationSafety.ValidateAndBackupAsync(dbContext);

                if (!safetyResult.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Migration safety check failed: {safetyResult.ErrorMessage}");
                    SafeFileLogger.Log("startup", $"MIGRATION ABORTED: {safetyResult.ErrorMessage}");
                    throw new InvalidOperationException($"Migration aborted: {safetyResult.ErrorMessage}");
                }

                if (!string.IsNullOrEmpty(safetyResult.BackupPath))
                {
                    SafeFileLogger.Log("startup", $"Pre-migration backup created: {safetyResult.BackupPath}");
                }
            }

            System.Diagnostics.Debug.WriteLine("Running database migrations...");
            SafeFileLogger.Log("startup", "Running database migrations...");
            dbContext.Database.Migrate();
            System.Diagnostics.Debug.WriteLine("Database migrations completed successfully");
            SafeFileLogger.Log("startup", "Database migrations completed");

            // Count data AFTER migration, BEFORE seeding
            var ingredientCountAfterMigration = dbContext.Ingredients.Count();
            var recipeCountAfterMigration = dbContext.Recipes.Count();
            var entreeCountAfterMigration = dbContext.Entrees.Count();
            SafeFileLogger.Log("startup", $"AFTER migration - Ingredients: {ingredientCountAfterMigration}, Recipes: {recipeCountAfterMigration}, Entrees: {entreeCountAfterMigration}");

            DatabaseSeeder.SeedDatabase(dbContext);
            System.Diagnostics.Debug.WriteLine("Database seeding completed successfully");
            SafeFileLogger.Log("startup", "Database seeding completed");

            // Count data AFTER seeding
            var ingredientCountAfterSeed = dbContext.Ingredients.Count();
            var recipeCountAfterSeed = dbContext.Recipes.Count();
            var entreeCountAfterSeed = dbContext.Entrees.Count();
            SafeFileLogger.Log("startup", $"AFTER seeding - Ingredients: {ingredientCountAfterSeed}, Recipes: {recipeCountAfterSeed}, Entrees: {entreeCountAfterSeed}");

            // List all locations and their data counts
            SafeFileLogger.Log("startup", "=== LOCATION BREAKDOWN ===");
            var allLocations = dbContext.Locations.ToList();
            foreach (var loc in allLocations)
            {
                var locIngredients = dbContext.Ingredients.Count(i => i.LocationId == loc.Id);
                var locRecipes = dbContext.Recipes.Count(r => r.LocationId == loc.Id);
                var locEntrees = dbContext.Entrees.Count(e => e.LocationId == loc.Id);
                SafeFileLogger.Log("startup", $"Location: '{loc.Name}' (ID: {loc.Id}, UserId: {loc.UserId?.ToString() ?? "NULL"}) - Ingredients: {locIngredients}, Recipes: {locRecipes}, Entrees: {locEntrees}");
            }
            SafeFileLogger.Log("startup", "=== DATABASE INITIALIZATION COMPLETE ===\n");
        }
        catch (Exception ex)
        {
            // logFile variable removed - using SafeFileLogger
            SafeFileLogger.Log("startup", "*** DATABASE INITIALIZATION ERROR ***");
            SafeFileLogger.Log("startup", $"Exception: {ex.Message}");
            SafeFileLogger.Log("startup", $"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                SafeFileLogger.Log("startup", $"Inner exception: {ex.InnerException.Message}");
            }
            SafeFileLogger.Log("startup", "*** CONTINUING WITH PARTIAL INITIALIZATION ***\n");

            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            // App can still continue - database operations will fail individually
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // IMPORTANT: Set shutdown mode to OnMainWindowClose so closing onboarding window doesn't shut down app
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;

            // Check if onboarding has been completed
            bool needsOnboarding = await CheckOnboardingStatusAsync();

            if (needsOnboarding)
            {
                try
                {
                    // Show onboarding wizard
                    var settingsService = Services.GetRequiredService<ILocalSettingsService>();
                    var locationRepository = Services.GetRequiredService<ILocationRepository>();
                    var onboardingViewModel = new OnboardingViewModel(settingsService, locationRepository);
                    var onboardingWindow = new OnboardingWindow(onboardingViewModel);

                    // Show as regular window, not dialog (no parent window yet)
                    onboardingWindow.Show();

                    // Wait for onboarding to complete
                    var tcs = new TaskCompletionSource<bool>();
                    onboardingViewModel.OnboardingCompleted += (s, e) =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("Onboarding completed event fired");

                            // IMPORTANT: Close window on UI thread to avoid InvalidOperationException
                            // Use Post instead of InvokeAsync to avoid async void issues
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                try
                                {
                                    onboardingWindow.Close();
                                    tcs.TrySetResult(true);
                                }
                                catch (Exception closeEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error closing onboarding window: {closeEx.Message}");
                                    tcs.TrySetResult(true); // Still complete even if close fails
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in OnboardingCompleted handler: {ex.Message}");
                            tcs.TrySetException(ex);
                        }
                    };
                    onboardingWindow.Closed += (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine("Onboarding window closed event fired");
                        if (!tcs.Task.IsCompleted)
                            tcs.TrySetResult(false);
                    };

                    await tcs.Task;
                    System.Diagnostics.Debug.WriteLine("Onboarding task completed");

                    // Update location name with restaurant name from onboarding
                    await UpdateLocationFromOnboarding();
                    System.Diagnostics.Debug.WriteLine("Location updated from onboarding");

                    // Launch tutorial if user opted in
                    if (onboardingViewModel.TutorialOptedIn)
                    {
                        System.Diagnostics.Debug.WriteLine("User opted in to tutorial - will launch after main window");
                        // Note: We'll launch the tutorial after the main window is shown
                        // Store this flag for later
                        _shouldLaunchTutorial = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during onboarding: {ex}");
                    // Continue anyway - don't block app startup
                }
            }

            // LOCAL-ONLY MODE: Skip cloud session initialization
            SafeFileLogger.Log("startup", "=== LOCAL-ONLY MODE - SKIPPING CLOUD AUTH ===");
            System.Diagnostics.Debug.WriteLine("[APP STARTUP] Running in local-only mode - no cloud authentication");

            // No cloud auth needed - we're local only
            bool isAuthenticated = false;

            // LOCAL-ONLY MODE: Skip environment variable validation
            // Cloud sync is disabled, so Supabase credentials are not required
            System.Diagnostics.Debug.WriteLine("[APP STARTUP] Running in local-only mode - skipping cloud credential validation");

            // Ensure default location exists (only if onboarding was skipped or not run)
            System.Diagnostics.Debug.WriteLine("Ensuring default location...");
            await EnsureDefaultLocationAsync();
            System.Diagnostics.Debug.WriteLine("Default location ensured");

            // Show main window directly (offline-first)
            System.Diagnostics.Debug.WriteLine("Creating MainWindowViewModel...");
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            System.Diagnostics.Debug.WriteLine("MainWindowViewModel created");

            System.Diagnostics.Debug.WriteLine("Creating MainWindow...");
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            System.Diagnostics.Debug.WriteLine("MainWindow created");

            System.Diagnostics.Debug.WriteLine("Showing MainWindow...");
            desktop.MainWindow.Show();
            desktop.MainWindow.Activate();
            System.Diagnostics.Debug.WriteLine("MainWindow shown and activated");

            // IMPORTANT: Handle location selection after auto-login
            // This must happen AFTER MainWindow is created and shown
            if (isAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[App.Startup] User is authenticated - checking for location selection...");
                _ = HandleLocationSelectionAfterAutoLoginAsync();
            }

            // Launch tutorial if user opted in during onboarding
            if (_shouldLaunchTutorial)
            {
                System.Diagnostics.Debug.WriteLine("Launching tutorial...");
                _ = LaunchTutorialAsync(desktop.MainWindow);
            }

            // Check for updates in background (don't await)
            _ = CheckForUpdatesAsync(desktop.MainWindow);

            // Increment launch count and check if we should show donation reminder
            _ = CheckDonationReminderAsync(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task<bool> CheckOnboardingStatusAsync()
    {
        try
        {
            var onboardingPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Desktop Food Cost",
                "onboarding.json"
            );

            if (!System.IO.File.Exists(onboardingPath))
            {
                return true; // Needs onboarding
            }

            var json = await System.IO.File.ReadAllTextAsync(onboardingPath);
            var onboardingData = System.Text.Json.JsonSerializer.Deserialize<OnboardingStatus>(json);

            return onboardingData?.Completed != true;
        }
        catch
        {
            // If any error, show onboarding to be safe
            return true;
        }
    }

    private class OnboardingStatus
    {
        public bool Completed { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    private async Task UpdateLocationFromOnboarding()
    {
        try
        {
            var settingsService = Services?.GetService(typeof(ILocalSettingsService)) as ILocalSettingsService;
            var locationRepository = Services?.GetService(typeof(ILocationRepository)) as ILocationRepository;
            var currentLocationService = Services?.GetService(typeof(ICurrentLocationService)) as ICurrentLocationService;

            if (settingsService == null || locationRepository == null || currentLocationService == null)
                return;

            // Load settings to get restaurant name
            var settings = await settingsService.LoadSettingsAsync();
            var restaurantName = settings.RestaurantName;

            if (string.IsNullOrWhiteSpace(restaurantName))
                return;

            // IMPORTANT: Every user has exactly ONE offline location
            // We identify it by having a null UserId (not synced from server)
            var allLocations = await locationRepository.GetAllAsync();
            var offlineLocations = allLocations.Where(l => l.UserId == null).ToList();

            // If multiple offline locations exist (shouldn't happen but could due to seeding),
            // prefer the "Default Location" to update (this is the seeded one)
            Dfc.Core.Models.Location? offlineLocation = null;
            if (offlineLocations.Count > 1)
            {
                // Prefer updating the "Default Location" from seeding
                offlineLocation = offlineLocations.FirstOrDefault(l => l.Name == "Default Location")
                    ?? offlineLocations.FirstOrDefault(l => l.Name == "My Restaurant")
                    ?? offlineLocations.OrderBy(l => l.CreatedAt).First();

                System.Diagnostics.Debug.WriteLine($"Multiple offline locations found during onboarding ({offlineLocations.Count}), updating: {offlineLocation.Name}");

                // Delete the duplicate offline locations (keep the one we're updating)
                foreach (var duplicate in offlineLocations.Where(l => l.Id != offlineLocation.Id))
                {
                    try
                    {
                        await locationRepository.DeleteAsync(duplicate.Id);
                        System.Diagnostics.Debug.WriteLine($"Deleted duplicate offline location during onboarding: {duplicate.Name} (ID: {duplicate.Id})");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not delete duplicate offline location {duplicate.Name}: {ex.Message}");
                    }
                }
            }
            else
            {
                offlineLocation = offlineLocations.FirstOrDefault();
            }

            if (offlineLocation != null)
            {
                // Update the existing offline location with the new restaurant name
                offlineLocation.Name = restaurantName;
                offlineLocation.ModifiedAt = DateTime.UtcNow;
                await locationRepository.UpdateAsync(offlineLocation);
                System.Diagnostics.Debug.WriteLine($"Updated offline location name to: {restaurantName}");
            }
            else
            {
                // No offline location exists - create it
                // This should only happen on first run
                var newOfflineLocation = new Dfc.Core.Models.Location
                {
                    Id = Guid.NewGuid(),
                    UserId = null, // NULL = offline-only location
                    Name = restaurantName,
                    Address = "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                await locationRepository.AddAsync(newOfflineLocation);
                System.Diagnostics.Debug.WriteLine($"Created new offline location with name: {restaurantName}");
                offlineLocation = newOfflineLocation;
            }

            // Set the offline location as current
            currentLocationService.SetCurrentLocation(offlineLocation);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating location from onboarding: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private async Task HandleLocationSelectionAfterAutoLoginAsync()
    {
        try
        {
            var locationRepository = Services?.GetService(typeof(ILocationRepository)) as ILocationRepository;
            var locationService = Services?.GetService(typeof(ILocationService)) as ILocationService;
            var sessionService = Services?.GetService(typeof(IUserSessionService)) as IUserSessionService;
            var currentLocationService = Services?.GetService(typeof(ICurrentLocationService)) as ICurrentLocationService;

            if (locationRepository == null || locationService == null || sessionService == null || currentLocationService == null)
            {
                System.Diagnostics.Debug.WriteLine("[App.AutoLogin] Missing required services for location selection");
                return;
            }

            // Only handle location selection if user is authenticated
            if (!sessionService.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[App.AutoLogin] User not authenticated - skipping location selection");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[App.AutoLogin] User authenticated - checking locations...");

            // Get all locations (both online and offline)
            var allLocations = await locationRepository.GetAllAsync();

            // Filter to only online locations (belong to this user)
            var onlineLocations = allLocations.Where(l => l.UserId != null && l.IsActive).ToList();

            System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Found {onlineLocations.Count} online location(s)");

            if (onlineLocations.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[App.AutoLogin] No online locations found - will use default location logic");
                return;
            }
            else if (onlineLocations.Count == 1)
            {
                // Auto-select the single location
                var singleLocation = onlineLocations[0];
                currentLocationService.SetCurrentLocation(singleLocation);
                System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Auto-selected single location: {singleLocation.Name}");
            }
            else
            {
                // User has multiple locations - show selector dialog
                System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] User has {onlineLocations.Count} locations - showing selector");

                // We need to wait for the main window to be created first
                // So we'll defer this to after the window is shown
                // Set a flag or store this state
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Wait a moment for main window to be ready
                    await Task.Delay(500);

                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[App.AutoLogin] Showing location selector dialog...");

                        Core.Models.Location? selectedLocation = null;
                        var locationWindow = new Views.LocationSelectorWindow();
                        var locationViewModel = new ViewModels.LocationSelectorViewModel(
                            locationService,
                            sessionService,
                            (location) =>
                            {
                                selectedLocation = location;
                                locationWindow.SetSelectedLocation(location);
                            }
                        );

                        locationWindow.DataContext = locationViewModel;
                        await locationViewModel.InitializeAsync();
                        await locationWindow.ShowDialog(desktop.MainWindow);

                        if (selectedLocation != null)
                        {
                            currentLocationService.SetCurrentLocation(selectedLocation);
                            System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] User selected location: {selectedLocation.Name}");

                            // Trigger sync for the selected location
                            var syncService = Services?.GetService(typeof(ISyncService)) as ISyncService;
                            if (syncService != null)
                            {
                                System.Diagnostics.Debug.WriteLine("[App.AutoLogin] Syncing selected location...");
                                var syncResult = await syncService.SyncAsync();
                                if (syncResult.IsSuccess)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Location sync completed: {syncResult.ItemsDownloaded}↓ {syncResult.ItemsUploaded}↑");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Location sync failed: {syncResult.ErrorMessage}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[App.AutoLogin] User closed location selector without choosing - using first location");
                            currentLocationService.SetCurrentLocation(onlineLocations[0]);
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Error handling location selection: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[App.AutoLogin] Stack trace: {ex.StackTrace}");
        }
    }

    private async Task EnsureDefaultLocationAsync()
    {
        try
        {
            var locationRepository = Services?.GetService(typeof(ILocationRepository)) as ILocationRepository;
            var currentLocationService = Services?.GetService(typeof(ICurrentLocationService)) as ICurrentLocationService;
            var sessionService = Services?.GetService(typeof(IUserSessionService)) as IUserSessionService;

            if (locationRepository == null || currentLocationService == null || sessionService == null)
                return;

            // Get all locations
            var allLocations = await locationRepository.GetAllAsync();

            // Every user MUST have exactly ONE offline location (UserId = null)
            // If there are multiple offline locations (shouldn't happen but could due to seeding),
            // keep the best one and delete the duplicates
            var offlineLocations = allLocations.Where(l => l.UserId == null).ToList();

            Dfc.Core.Models.Location? offlineLocation = null;
            if (offlineLocations.Count > 1)
            {
                // Multiple offline locations exist - prefer customized ones
                offlineLocation = offlineLocations
                    .Where(l => l.Name != "Default Location" && l.Name != "My Restaurant")
                    .OrderByDescending(l => l.ModifiedAt)
                    .FirstOrDefault();

                // If all have default names, pick the most recently modified
                if (offlineLocation == null)
                {
                    offlineLocation = offlineLocations.OrderByDescending(l => l.ModifiedAt).First();
                }

                System.Diagnostics.Debug.WriteLine($"Multiple offline locations found ({offlineLocations.Count}), keeping: {offlineLocation.Name}");

                // Delete the duplicate offline locations
                foreach (var duplicate in offlineLocations.Where(l => l.Id != offlineLocation.Id))
                {
                    try
                    {
                        await locationRepository.DeleteAsync(duplicate.Id);
                        System.Diagnostics.Debug.WriteLine($"Deleted duplicate offline location: {duplicate.Name} (ID: {duplicate.Id})");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not delete duplicate offline location {duplicate.Name}: {ex.Message}");
                    }
                }
            }
            else
            {
                offlineLocation = offlineLocations.FirstOrDefault();
            }

            if (offlineLocation == null)
            {
                // Create the offline location (this should only happen if onboarding was skipped)
                offlineLocation = new Dfc.Core.Models.Location
                {
                    Id = Guid.NewGuid(),
                    UserId = null, // NULL = offline-only location
                    Name = "My Restaurant",
                    Address = "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await locationRepository.AddAsync(offlineLocation);
                System.Diagnostics.Debug.WriteLine("Created default offline location");
            }

            // IMPORTANT: Respect authentication state when setting current location
            // If user is NOT currently authenticated, ALWAYS use the offline location
            // This ensures offline data is visible even if online locations exist in the database from previous sessions
            if (!sessionService.IsAuthenticated)
            {
                // User is offline (not signed in) - restore last offline location or use default
                var lastLocationId = currentLocationService.LoadLastLocationId();
                if (lastLocationId.HasValue)
                {
                    var lastLocation = allLocations.FirstOrDefault(l => l.Id == lastLocationId.Value);
                    if (lastLocation != null && lastLocation.IsActive && lastLocation.UserId == null)
                    {
                        // Restore offline location
                        currentLocationService.SetCurrentLocation(lastLocation);
                        System.Diagnostics.Debug.WriteLine($"[APP STARTUP] Restored last offline location: {lastLocation.Name}");
                        return;
                    }
                }
                
                // Default to offline location
                currentLocationService.SetCurrentLocation(offlineLocation);
                System.Diagnostics.Debug.WriteLine($"[APP STARTUP] Set default offline location: {offlineLocation.Name}");
            }
            else
            {
                // User is authenticated - default to offline/local location
                // User can switch to online locations via location selector
                currentLocationService.SetCurrentLocation(offlineLocation);
                System.Diagnostics.Debug.WriteLine($"[APP STARTUP] User authenticated - set to default local location: {offlineLocation.Name}");
                System.Diagnostics.Debug.WriteLine($"[APP STARTUP] Available locations: {allLocations.Count}");
                foreach (var loc in allLocations)
                {
                    System.Diagnostics.Debug.WriteLine($"[APP STARTUP]   - {loc.Name} (ID: {loc.Id}, UserId: {loc.UserId})");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error ensuring default location: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private async Task LaunchTutorialAsync(Window mainWindow)
    {
        try
        {
            // Wait a moment for the main window to fully render
            await Task.Delay(500);

            var tutorialService = Services?.GetService(typeof(IExtendedTutorialService)) as IExtendedTutorialService;
            var progressTracker = Services?.GetService(typeof(ITutorialProgressTracker)) as ITutorialProgressTracker;

            if (tutorialService == null || progressTracker == null)
            {
                System.Diagnostics.Debug.WriteLine("Tutorial services not available");
                return;
            }

            // Check if user has already completed or started the Quick Start module
            var progress = await progressTracker.LoadProgressAsync();

            // Check if Quick Start module is already completed
            var quickStartModule = tutorialService.GetModuleById("getting-started");
            if (quickStartModule != null)
            {
                bool isQuickStartCompleted = true;
                foreach (var step in quickStartModule.Steps)
                {
                    if (!await progressTracker.HasCompletedStepAsync("getting-started", step.Id))
                    {
                        isQuickStartCompleted = false;
                        break;
                    }
                }

                // Don't launch if Quick Start is already completed
                if (isQuickStartCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("Quick Start already completed - skipping tutorial launch");
                    return;
                }
            }

            // Mark that user opted in to the extended tutorial
            progress.OptedInToExtendedTutorial = true;
            progress.FirstAccessedDate = DateTime.UtcNow;
            await progressTracker.SaveProgressAsync(progress);

            // Launch tutorial window on UI thread - Start with "Quick Start" module only
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var tutorialWindow = new ExtendedTutorialWindow();
                var tutorialViewModel = new ExtendedTutorialViewModel(
                    tutorialService,
                    progressTracker,
                    () => tutorialWindow.Close(),
                    "getting-started" // Start with Quick Start module (5 steps)
                );

                tutorialWindow.DataContext = tutorialViewModel;
                await tutorialWindow.ShowDialog(mainWindow);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching tutorial: {ex.Message}");
            // Don't block app startup if tutorial fails
        }
    }

    private async Task CheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            // Wait a bit after startup to not slow down initial load
            await Task.Delay(3000);

            System.Diagnostics.Debug.WriteLine("========== STARTING AUTO-UPDATE CHECK ==========");

            var updateService = Services?.GetService(typeof(IAutoUpdateService)) as IAutoUpdateService;
            if (updateService == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: AutoUpdateService not found in DI container!");
                return;
            }

            System.Diagnostics.Debug.WriteLine("AutoUpdateService found, checking for updates...");

            var logger = Services?.GetService(typeof(ILogger<UpdateNotificationViewModel>)) as ILogger<UpdateNotificationViewModel>;
            var result = await updateService.CheckForUpdateAsync();

            System.Diagnostics.Debug.WriteLine($"Update check result: IsUpdateAvailable={result.IsUpdateAvailable}, Message={result.Message}");

            if (result.IsUpdateAvailable && result.DownloadUrl != null)
            {
                System.Diagnostics.Debug.WriteLine($"Update available! Version {result.LatestVersion}, Download URL: {result.DownloadUrl}");

                // Show professional update notification window
                try
                {
                    System.Diagnostics.Debug.WriteLine("Creating UpdateNotificationViewModel...");
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine("Inside UI thread...");
                        var viewModel = new UpdateNotificationViewModel(updateService, logger);
                        System.Diagnostics.Debug.WriteLine("ViewModel created, initializing...");
                        viewModel.Initialize(result);
                        System.Diagnostics.Debug.WriteLine("ViewModel initialized, creating window...");

                        var updateWindow = new Views.UpdateNotificationWindow(viewModel);
                        System.Diagnostics.Debug.WriteLine("Window created, showing dialog...");
                        await updateWindow.ShowDialog(mainWindow);
                        System.Diagnostics.Debug.WriteLine("Dialog closed");
                    });
                }
                catch (Exception windowEx)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR showing update window: {windowEx.GetType().Name}");
                    System.Diagnostics.Debug.WriteLine($"Message: {windowEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {windowEx.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No update available or no download URL. Result: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the app
            System.Diagnostics.Debug.WriteLine($"ERROR in CheckForUpdatesAsync: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("========== AUTO-UPDATE CHECK COMPLETE ==========");
        }
    }

    private async Task CheckDonationReminderAsync(Window mainWindow)
    {
        try
        {
            // Wait a bit after startup to not interfere with other startup tasks
            await Task.Delay(5000); // 5 seconds

            System.Diagnostics.Debug.WriteLine("========== CHECKING DONATION REMINDER ==========");

            var donationService = Services?.GetService(typeof(IDonationReminderService)) as IDonationReminderService;
            if (donationService == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: DonationReminderService not found in DI container!");
                return;
            }

            System.Diagnostics.Debug.WriteLine("DonationReminderService found, incrementing launch count...");

            // Increment launch count
            await donationService.IncrementLaunchCountAsync();

            // Check if we should show reminder
            var shouldShow = await donationService.ShouldShowDonationReminderAsync();
            System.Diagnostics.Debug.WriteLine($"Should show donation reminder: {shouldShow}");

            if (!shouldShow)
            {
                System.Diagnostics.Debug.WriteLine("Donation reminder not needed this launch");
                return;
            }

            // Get stats and show window
            System.Diagnostics.Debug.WriteLine("Getting donation stats...");
            var stats = await donationService.GetDonationStatsAsync();
            System.Diagnostics.Debug.WriteLine($"Stats: {stats.IngredientCount} ingredients, {stats.RecipeCount} recipes, {stats.EntreeCount} plates");

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                System.Diagnostics.Debug.WriteLine("Creating DonationReminderWindow...");
                var viewModel = new ViewModels.DonationReminderViewModel(stats);
                var window = new Views.DonationReminderWindow
                {
                    DataContext = viewModel
                };
                viewModel.CloseRequested += (s, e) => window.Close();
                System.Diagnostics.Debug.WriteLine("Showing donation reminder window...");
                window.Show();
            });
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the app
            System.Diagnostics.Debug.WriteLine($"ERROR in CheckDonationReminderAsync: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            System.Diagnostics.Debug.WriteLine("========== DONATION REMINDER CHECK COMPLETE ==========");
        }
    }

    private bool ValidateEnvironmentVariables()
    {
        try
        {
            var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");
            var usdaKey = Environment.GetEnvironmentVariable("USDA_API_KEY");

            System.Diagnostics.Debug.WriteLine("[ENV CHECK] SUPABASE_URL: " + (string.IsNullOrEmpty(supabaseUrl) ? "NOT FOUND" : "FOUND"));
            System.Diagnostics.Debug.WriteLine("[ENV CHECK] SUPABASE_ANON_KEY: " + (string.IsNullOrEmpty(supabaseKey) ? "NOT FOUND" : "FOUND"));
            System.Diagnostics.Debug.WriteLine("[ENV CHECK] USDA_API_KEY: " + (string.IsNullOrEmpty(usdaKey) ? "NOT FOUND" : "FOUND"));

            // All three required environment variables must be present
            bool allPresent = !string.IsNullOrEmpty(supabaseUrl) &&
                            !string.IsNullOrEmpty(supabaseKey) &&
                            !string.IsNullOrEmpty(usdaKey);

            if (!allPresent)
            {
                // Check if they exist in registry (installer set them but system hasn't loaded them yet)
                if (CheckRegistryForEnvironmentVariables())
                {
                    System.Diagnostics.Debug.WriteLine("[ENV CHECK] Variables found in registry but not in environment - RESTART REQUIRED");
                    return false; // Need restart
                }
            }

            return allPresent;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ENV CHECK] Error validating environment variables: {ex.Message}");
            return true; // Don't block startup on validation errors
        }
    }

    private bool CheckRegistryForEnvironmentVariables()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Environment");
            if (key == null) return false;

            var supabaseUrl = key.GetValue("SUPABASE_URL") as string;
            var supabaseKey = key.GetValue("SUPABASE_ANON_KEY") as string;
            var usdaKey = key.GetValue("USDA_API_KEY") as string;

            bool allInRegistry = !string.IsNullOrEmpty(supabaseUrl) &&
                                !string.IsNullOrEmpty(supabaseKey) &&
                                !string.IsNullOrEmpty(usdaKey);

            System.Diagnostics.Debug.WriteLine($"[REGISTRY CHECK] Variables in registry: {allInRegistry}");
            return allInRegistry;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[REGISTRY CHECK] Error checking registry: {ex.Message}");
            return false;
        }
    }

    private async Task ShowEnvironmentVariableRestartPrompt(Window? parentWindow)
    {
        try
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var messageBox = new Window
                {
                    Title = "Restart Required - Desktop Food Cost",
                    Width = 500,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    CanResize = false,
                    Content = new StackPanel
                    {
                        Margin = new Thickness(20),
                        Spacing = 15,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "⚠️ Restart Required",
                                FontSize = 18,
                                FontWeight = FontWeight.Bold
                            },
                            new TextBlock
                            {
                                Text = "Desktop Food Cost has been installed successfully, but your computer needs to restart for the application to work properly.",
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 14
                            },
                            new TextBlock
                            {
                                Text = "The required environment variables (Supabase credentials and USDA API) have been configured but are not yet loaded by the system.",
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 12,
                                Foreground = Brushes.Gray
                            },
                            new Button
                            {
                                Content = "OK - I'll Restart Now",
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                Padding = new Thickness(30, 10),
                                Margin = new Thickness(0, 10, 0, 0)
                            }
                        }
                    }
                };

                var button = (messageBox.Content as StackPanel)?.Children[3] as Button;
                if (button != null)
                {
                    button.Click += (s, e) => messageBox.Close();
                }

                await messageBox.ShowDialog(parentWindow ?? new Window());

                // Exit the application
                Environment.Exit(0);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ENV PROMPT] Error showing restart prompt: {ex.Message}");
            // Exit anyway
            Environment.Exit(0);
        }
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Logging (optional - can be configured later)
        services.AddLogging();

        // HttpClient for external API calls (nutritional data, etc.)
        services.AddHttpClient();

        // Configuration for services that need it (reads from environment variables)
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);

        // DIAGNOSTIC: Log USDA API Key configuration status at startup
        try
        {
            var usdaConfigKey = configuration["USDA:ApiKey"];
            var usdaEnvKey = Environment.GetEnvironmentVariable("USDA_API_KEY");
            var usdaEnvKeyDoubleUnderscore = Environment.GetEnvironmentVariable("USDA__ApiKey");

            System.Diagnostics.Debug.WriteLine("========== USDA API KEY DIAGNOSTIC ==========");
            System.Diagnostics.Debug.WriteLine($"Config[USDA:ApiKey]: {(!string.IsNullOrEmpty(usdaConfigKey) ? "FOUND (length: " + usdaConfigKey.Length + ")" : "NOT FOUND")}");
            System.Diagnostics.Debug.WriteLine($"Environment.GetEnvironmentVariable(USDA_API_KEY): {(!string.IsNullOrEmpty(usdaEnvKey) ? "FOUND (length: " + usdaEnvKey.Length + ")" : "NOT FOUND")}");
            System.Diagnostics.Debug.WriteLine($"Environment.GetEnvironmentVariable(USDA__ApiKey): {(!string.IsNullOrEmpty(usdaEnvKeyDoubleUnderscore) ? "FOUND (length: " + usdaEnvKeyDoubleUnderscore.Length + ")" : "NOT FOUND")}");

            // Show first 10 chars for verification (if exists)
            if (!string.IsNullOrEmpty(usdaEnvKey))
            {
                System.Diagnostics.Debug.WriteLine($"USDA_API_KEY starts with: {usdaEnvKey.Substring(0, Math.Min(10, usdaEnvKey.Length))}...");
            }
            System.Diagnostics.Debug.WriteLine("=============================================");
        }
        catch (Exception diagEx)
        {
            System.Diagnostics.Debug.WriteLine($"Error in USDA API key diagnostic: {diagEx.Message}");
        }

        // Supabase credentials are loaded from .env file automatically

        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost",
            "freecost.db"
        );
        var directory = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            ForeignKeys = true
        }.ToString();

        services.AddDbContext<DfcDbContext>(options =>
            options.UseSqlite(connectionString)
                   .AddInterceptors(new SqliteForeignKeyInterceptor()));

        // Repositories
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IRecipeVersionRepository, RecipeVersionRepository>();
        services.AddScoped<IEntreeRepository, EntreeRepository>();
        services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILocationUserRepository, LocationUserRepository>();
        services.AddScoped<IWasteRecordRepository, WasteRecordRepository>();
        services.AddScoped<IDeletedItemRepository, DeletedItemRepository>();
        services.AddScoped<IDraftItemRepository, DraftItemRepository>();
        services.AddScoped<IIngredientMatchMappingRepository, IngredientMatchMappingRepository>();
        services.AddScoped<IIngredientConversionRepository, IngredientConversionRepository>();
        services.AddScoped<IImportMapRepository, ImportMapRepository>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();

        // v1.5.0 - Multi-User & Collaboration Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddScoped<ISharedRecipeRepository, SharedRecipeRepository>();
        services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
        services.AddScoped<IRecipeCommentRepository, RecipeCommentRepository>();
        services.AddScoped<IEntreeCommentRepository, EntreeCommentRepository>();
        services.AddScoped<IChangeHistoryRepository, ChangeHistoryRepository>();
        services.AddScoped<ITeamNotificationRepository, TeamNotificationRepository>();
        services.AddScoped<ITeamActivityFeedRepository, TeamActivityFeedRepository>();

        // Services
        services.AddScoped<IIngredientService, IngredientService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IRecipeVersionService, RecipeVersionService>();
        services.AddScoped<IEntreeService, EntreeService>();
        services.AddScoped<IPriceHistoryService, PriceHistoryService>();
        // Local-only mode: LocationService without cloud sync
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ILocationUserService, LocationUserService>();

        // Delta Sync Optimization
        services.AddScoped<ILocalModificationService, LocalModificationService>();

        // Photo Service (Singleton - local storage only)
        services.AddSingleton<IPhotoService, LocalPhotoService>();

        // Recipe Card PDF Service (Singleton - creates directories once)
        services.AddSingleton<IRecipeCardService, RecipeCardService>();

        // Entree Card PDF Service (Singleton - creates directories once)
        services.AddSingleton<IEntreeCardService, EntreeCardService>();

        // Excel Import Service (Singleton)
        services.AddSingleton<IExcelImportService, ExcelImportService>();

        // Excel Export Service (Singleton)
        services.AddSingleton<IExcelExportService, ExcelExportService>();

        // CSV Export Service (Singleton)
        services.AddSingleton<ICsvExportService, CsvExportService>();

        // Recipe URL Import Service (Singleton) - Imports recipes from cooking websites
        services.AddSingleton<IRecipeUrlImportService, RecipeUrlImportService>();

        // Import Map Service (Singleton) - Auto-detects vendor formats
        services.AddSingleton<IImportMapService, ImportMapService>();

        // Ingredient Matching Service (Scoped) - Fuzzy string matching with persistent mappings
        services.AddScoped<IIngredientMatchingService, IngredientMatchingService>();

        // Ingredient Match Mapping Service (Scoped) - Manages persistent ingredient match mappings
        services.AddScoped<IIngredientMatchMappingService, IngredientMatchMappingService>();

        // Global Config Service (Singleton) - Manages ALL global configuration from Firebase
        // Includes: ingredient/recipe mappings, vendor import maps, unit conversions, allergen keywords
        services.AddSingleton<IGlobalConfigService, GlobalConfigService>();

        // Nutritional Data Service (Singleton) - Queries USDA FoodData Central API for nutritional information
        services.AddSingleton<INutritionalDataService, NutritionalDataService>();

        // Recipe Card Import Service (Singleton) - Parses multi-tab Excel recipe cards
        services.AddSingleton<IRecipeCardImportService, RecipeCardImportService>();

        // Entree Card Import Service (Singleton) - Parses multi-tab Excel entree cards
        services.AddSingleton<IEntreeCardImportService, EntreeCardImportService>();

        // Allergen Detection Service (Singleton - keyword mapping is static)
        services.AddSingleton<IAllergenDetectionService, AllergenDetectionService>();

        // ========================================
        // SUPABASE SERVICES - DISABLED (Local-only mode)
        // ========================================
        // All Supabase services are disabled in local-only mode
        // Uncomment these if re-enabling cloud sync

        // services.AddSingleton<SupabaseDataService>();
        // services.AddSingleton<SupabasePhotoService>();
        // services.AddSingleton<Supabase.Client>(sp => SupabaseClientProvider.GetClientAsync().GetAwaiter().GetResult());

        // ========================================
        // AUTHENTICATION SERVICES
        // ========================================

        // Local-only mode: Use mock authentication (no cloud auth)
        services.AddSingleton<IAuthenticationService, MockAuthenticationService>();


        services.AddSingleton<IUserSessionService, UserSessionService>();

        // User Restaurant Service (Singleton - manages user-restaurant relationships in Firestore)
        services.AddSingleton<IUserRestaurantService, UserRestaurantService>();

        // Current Location Service (Singleton - tracks selected location for session)
        services.AddSingleton<ICurrentLocationService, CurrentLocationService>();

        // ========================================
        // SYNC SERVICE - LOCAL ONLY
        // ========================================

        // Local-only mode: No cloud sync
        services.AddSingleton<ISyncService, LocalOnlySyncService>();

        // Update Service (Singleton - checks for app updates)

        // Database Backup Service (Singleton - manages backups for entire session)
        services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();

        // Migration Safety Service
        services.AddSingleton<Dfc.Data.Services.IMigrationSafetyService, Dfc.Data.Services.MigrationSafetyService>();

        // Backup Service (Singleton - manages full database + photo backups)
        services.AddSingleton<IBackupService, BackupService>();

        // Auto Update Service (Singleton - checks for updates)
        // Auto Update Service (Singleton - checks for updates)
        services.AddSingleton<IAutoUpdateService>(sp =>
        {
            string? githubToken = null;
            try
            {
                var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    var jsonContent = File.ReadAllText(appSettingsPath);
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonContent);
                    if (jsonDoc.RootElement.TryGetProperty("GitHub", out var githubSection))
                    {
                        if (githubSection.TryGetProperty("Token", out var tokenElement))
                        {
                            githubToken = tokenElement.GetString();
                            if (githubToken == "PASTE_YOUR_TOKEN_HERE")
                            {
                                githubToken = null; // Ignore placeholder
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read GitHub token from appsettings.json: {ex.Message}");
            }
            
            return new AutoUpdateService(sp.GetService<ILogger<AutoUpdateService>>(), githubToken);
        });

        // Bug Report Service (Singleton - saves bug reports locally)
        services.AddSingleton<IBugReportService>(sp =>
        {
            return new LocalBugReportService(
                sp.GetService<ILogger<LocalBugReportService>>()
            );
        });

        // Donation Reminder Service (Singleton - tracks app launches and shows donation popup)
        services.AddSingleton<IDonationReminderService>(sp => new DonationReminderService(
            sp.GetRequiredService<IIngredientRepository>(),
            sp.GetRequiredService<IRecipeRepository>(),
            sp.GetRequiredService<IEntreeRepository>(),
            sp.GetRequiredService<ICurrentLocationService>()
        ));

        // Spoony Helper Service (Singleton - Black Lab helper like Clippy)
        services.AddSingleton<ISpoonyService, SpoonyService>();


        // Universal Conversion System (Phase 2)
        services.AddSingleton<ConversionDatabaseService>(sp =>
        {
            var service = new ConversionDatabaseService(sp.GetService<ILogger<ConversionDatabaseService>>());
            service.InitializeAsync().GetAwaiter().GetResult();
            return service;
        });
        services.AddSingleton<UnitConversionService>();
        services.AddScoped<IUniversalConversionService, UniversalConversionService>();

        // Cost Calculator
        services.AddScoped<IRecipeCostCalculator, RecipeCostCalculator>();

        // Validation Service
        services.AddSingleton<IValidationService, ValidationService>();

        // Validation Feedback Service (Singleton - provides enhanced error messages)
        services.AddSingleton<IValidationFeedbackService, ValidationFeedbackService>();

        // Nutritional Goals Service (Singleton - persists goals to file)
        services.AddSingleton<INutritionalGoalsService, NutritionalGoalsService>();

        // Recipe Cost Trend Service (Scoped - analyzes historical cost data)
        services.AddScoped<IRecipeCostTrendService, RecipeCostTrendService>();

        // Price Alert Service (Scoped - monitors ingredient price changes)
        services.AddScoped<IPriceAlertService, PriceAlertService>();

        // Menu Engineering Service (Scoped - analyzes menu profitability)
        services.AddScoped<IMenuEngineeringService, MenuEngineeringService>();

        // Vendor Comparison Service (Scoped - analyzes vendor pricing)
        services.AddScoped<IVendorComparisonService, VendorComparisonService>();

        // Recipe Profitability Service (Scoped - analyzes recipe profit margins)
        services.AddScoped<IRecipeProfitabilityService, RecipeProfitabilityService>();

        // Vendor Price History Service (Scoped - tracks vendor pricing over time)
        services.AddScoped<IVendorPriceHistoryService, VendorPriceHistoryService>();

        // Cost Variance Service (Scoped - analyzes cost differences)
        services.AddScoped<ICostVarianceService, CostVarianceService>();

        // Seasonal Trend Service (Scoped - analyzes seasonal price patterns)
        services.AddScoped<ISeasonalTrendService, SeasonalTrendService>();

        // Inventory Turnover Service (Scoped - tracks ingredient usage rates)
        services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();

        // Waste Tracking Service (Scoped - tracks and reports ingredient waste)
        services.AddScoped<IWasteTrackingService, WasteTrackingService>();

        // Custom Report Service (Scoped - generates custom reports)
        services.AddScoped<ICustomReportService, CustomReportService>();

        // Undo/Redo Service (Singleton - tracks command history for entire session)
        services.AddSingleton<IUndoRedoService, UndoRedoService>();

        // Recycle Bin Service (Scoped - manages deleted items)
        services.AddScoped<IRecycleBinService, RecycleBinService>();

        // Photo Gallery Service (Scoped - manages multi-photo galleries)
        services.AddScoped<IPhotoGalleryService, Dfc.Data.Services.PhotoGalleryService>();

        // Print Preview Service (Scoped - generates print-ready PDFs)
        services.AddScoped<IPrintPreviewService, PrintPreviewService>();

        // Auto-Save Service (Scoped - manages draft items)
        services.AddScoped<IAutoSaveService, AutoSaveService>();

        // Cache Service (Singleton - memory cache for performance)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        // Performance Service (Singleton - pagination and optimization helpers)
        services.AddSingleton<IPerformanceService, PerformanceService>();

        // Diet Compliance Service (Scoped - checks dietary requirements)
        services.AddScoped<IDietComplianceService, DietComplianceService>();

        // Scheduled Report Service (Singleton - manages scheduled reports)
        services.AddSingleton<IScheduledReportService, ScheduledReportService>();

        // Custom Dashboard Service (Singleton - manages custom dashboards)
        services.AddSingleton<ICustomDashboardService, CustomDashboardService>();

        // Report Template Service (Singleton - manages report templates)
        services.AddSingleton<IReportTemplateService, ReportTemplateService>();

        // Local Settings Service (Singleton - JSON file persistence for single-user app)
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();

        // Status Notification Service (Singleton - global status notifications)
        services.AddSingleton<IStatusNotificationService, StatusNotificationService>();

        // Network Connectivity Service (Singleton - monitors network state)
        services.AddSingleton<INetworkConnectivityService, NetworkConnectivityService>();

        // v1.5.0 - Multi-User & Collaboration Services
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<IRecipeSharingService, RecipeSharingService>();
        services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IChangeTrackingService, ChangeTrackingService>();
        services.AddScoped<ITeamCollaborationService, TeamCollaborationService>();

        // Concurrency Handler (Singleton - stateless service)
        services.AddSingleton<IConcurrencyHandler, ConcurrencyHandler>();

        // Category Color Service (Singleton - generates consistent colors)
        services.AddSingleton<ICategoryColorService, CategoryColorService>();

        // Batch Operations Service (Scoped - performs batch operations on items)
        services.AddScoped<IBatchOperationsService, BatchOperationsService>();

        // Tutorial Services (Singleton - tutorial content and progress tracking)
        services.AddSingleton<IExtendedTutorialService, ExtendedTutorialService>();
        services.AddSingleton<ITutorialProgressTracker, TutorialProgressTracker>();

        // ViewModels
        services.AddTransient<SettingsViewModel>(sp => new SettingsViewModel(
            sp,
            sp.GetRequiredService<IExcelExportService>(),
            sp.GetRequiredService<IIngredientService>(),
            sp.GetRequiredService<IRecipeService>(),
            sp.GetRequiredService<IEntreeService>(),
            sp.GetRequiredService<IPriceHistoryService>(),
            sp.GetRequiredService<ILocalSettingsService>(),
            sp.GetRequiredService<IStatusNotificationService>(),
            sp.GetRequiredService<IDatabaseBackupService>(),
            sp.GetRequiredService<IAutoUpdateService>(),
            null,
            sp.GetRequiredService<IIngredientConversionRepository>()
        ));

        services.AddTransient<AdminViewModel>(sp => new AdminViewModel(
            sp,
            sp.GetRequiredService<IUserSessionService>(),
            sp.GetService<IBugReportService>(),
            sp.GetService<ILogger<AdminViewModel>>()
        ));

        services.AddTransient<HelpViewModel>(sp => new HelpViewModel(
            sp.GetRequiredService<IExtendedTutorialService>(),
            sp.GetRequiredService<ITutorialProgressTracker>()
        ));

        services.AddTransient<ReportsViewModel>(sp => new ReportsViewModel(
            sp.GetRequiredService<ICustomReportService>(),
            sp.GetRequiredService<ICurrentLocationService>(),
            sp.GetService<ILogger<ReportsViewModel>>()
        ));

        services.AddTransient<AboutViewModel>(sp => new AboutViewModel(
            sp.GetRequiredService<IAutoUpdateService>(),
            sp.GetService<ILogger<AboutViewModel>>()
        ));

        // Windows
        services.AddTransient<AddEditEntreeWindow>();

        // MainWindowViewModel with factory functions
        services.AddSingleton<MainWindowViewModel>(sp =>
        {
            MainWindowViewModel? mainWindowViewModel = null;
            mainWindowViewModel = new MainWindowViewModel(
                (window) => new IngredientsViewModel(
                    sp.GetRequiredService<IIngredientService>(),
                    sp.GetRequiredService<IImportMapService>(),
                    sp.GetRequiredService<IPriceHistoryService>(),
                    sp.GetRequiredService<IExcelExportService>(),
                    sp.GetRequiredService<IValidationService>(),
                    sp.GetRequiredService<IRecycleBinService>(),
                    sp.GetRequiredService<ICurrentLocationService>(),
                    sp.GetRequiredService<IStatusNotificationService>(),
                    sp.GetRequiredService<ICategoryColorService>(),
                    sp.GetRequiredService<IBatchOperationsService>(),
                    window,
                    async () => await mainWindowViewModel!.LoadDeletedItemsCountAsync(),
                    sp.GetRequiredService<INutritionalDataService>(),
                    sp.GetRequiredService<IIngredientConversionRepository>(),
                    sp.GetRequiredService<IImportMapRepository>(),
                    sp.GetRequiredService<IImportBatchRepository>(),
                    sp.GetRequiredService<IIngredientRepository>()
                ),
                (window) => new RecipesViewModel(
                    sp.GetRequiredService<IRecipeService>(),
                    sp,
                    sp.GetRequiredService<IRecipeCostCalculator>(),
                    sp.GetRequiredService<ICurrentLocationService>(),
                    sp.GetRequiredService<IStatusNotificationService>(),
                    window,
                    async () => await mainWindowViewModel!.LoadDeletedItemsCountAsync()
                ),
                () => new EntreesViewModel(
                    sp.GetRequiredService<IEntreeService>(),
                    sp,
                    sp.GetRequiredService<ICurrentLocationService>(),
                    sp.GetRequiredService<IStatusNotificationService>(),
                    async () => await mainWindowViewModel!.LoadDeletedItemsCountAsync()
                ),
                () => sp.GetRequiredService<SettingsViewModel>(),
                () => sp.GetRequiredService<AdminViewModel>(),
                () => new DashboardViewModel(
                    sp.GetRequiredService<IIngredientService>(),
                    sp.GetRequiredService<IRecipeService>(),
                    sp.GetRequiredService<IEntreeService>(),
                    sp.GetRequiredService<ICurrentLocationService>()
                ),
                () => new MenuPlanViewModel(
                    sp.GetRequiredService<IEntreeService>()
                ),
                sp.GetRequiredService<IUndoRedoService>(),
                sp.GetRequiredService<IRecycleBinService>(),
                sp.GetRequiredService<ITeamNotificationRepository>(),
                sp.GetRequiredService<IIngredientService>(),
                sp.GetRequiredService<IRecipeService>(),
                sp.GetRequiredService<IEntreeService>(),
                sp.GetRequiredService<IPriceHistoryService>(),
                sp.GetRequiredService<ICurrentLocationService>(),
                sp.GetRequiredService<IUserSessionService>(),
                sp.GetRequiredService<ISyncService>(),
                sp.GetService<ILogger<MainWindowViewModel>>()
            );
            return mainWindowViewModel;
        });
    }
}
