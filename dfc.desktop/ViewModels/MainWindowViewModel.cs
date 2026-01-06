using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Dfc.Core.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Dfc.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Func<Window, IngredientsViewModel> _createIngredientsViewModel;
    private readonly Func<Window, RecipesViewModel> _createRecipesViewModel;
    private readonly Func<EntreesViewModel> _createEntreesViewModel;
    private readonly Func<SettingsViewModel> _createSettingsViewModel;
    private readonly Func<AdminViewModel> _createAdminViewModel;
    private readonly Func<DashboardViewModel> _createDashboardViewModel;
    private readonly Func<MenuPlanViewModel> _createMenuPlanViewModel;
    private readonly IUndoRedoService _undoRedoService;
    private readonly IRecycleBinService _recycleBinService;
    private readonly ITeamNotificationRepository _notificationRepository;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly IUserSessionService? _sessionService;
    private readonly ISyncService? _syncService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly ILogger<MainWindowViewModel>? _logger;
    private Window? _mainWindow;
    private Timer? _autoSyncTimer;
    private readonly Guid _currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _currentLocationName = "Local Mode";

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private string _undoTooltip = "Undo";

    [ObservableProperty]
    private string _redoTooltip = "Redo";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDeletedItems))]
    private int _deletedItemsCount;

    public bool HasDeletedItems => DeletedItemsCount > 0;

    [ObservableProperty]
    private int _unreadNotificationsCount;

    [ObservableProperty]
    private ObservableCollection<RecentItemModel> _recentItems = new();

    [ObservableProperty]
    private string _currentBreadcrumb = "Dashboard";

    // User Profile Properties
    [ObservableProperty]
    private string _userEmail = "Offline Mode";

    [ObservableProperty]
    private string _userDisplayName = "Guest";

    [ObservableProperty]
    private string _syncStatusIcon = "⚠️";

    [ObservableProperty]
    private string _syncStatusText = "Offline";

    [ObservableProperty]
    private string _syncStatusColor = "#FF9800";

    [ObservableProperty]
    private bool _isOfflineMode = true;

    [ObservableProperty]
    private bool _isAdmin = false;

    [ObservableProperty]
    private ObservableCollection<Location> _availableLocations = new();

    public string OfflineModeToggleText => IsOfflineMode ? "📶 Go Online" : "✈️ Go Offline";

    public MainWindowViewModel(
        Func<Window, IngredientsViewModel> createIngredientsViewModel,
        Func<Window, RecipesViewModel> createRecipesViewModel,
        Func<EntreesViewModel> createEntreesViewModel,
        Func<SettingsViewModel> createSettingsViewModel,
        Func<AdminViewModel> createAdminViewModel,
        Func<DashboardViewModel> createDashboardViewModel,
        Func<MenuPlanViewModel> createMenuPlanViewModel,
        IUndoRedoService undoRedoService,
        IRecycleBinService recycleBinService,
        ITeamNotificationRepository notificationRepository,
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService,
        IPriceHistoryService priceHistoryService,
        ICurrentLocationService currentLocationService,
        IUserSessionService? sessionService = null,
        ISyncService? syncService = null,
        ILogger<MainWindowViewModel>? logger = null)
    {
        _createIngredientsViewModel = createIngredientsViewModel;
        _createRecipesViewModel = createRecipesViewModel;
        _createEntreesViewModel = createEntreesViewModel;
        _createSettingsViewModel = createSettingsViewModel;
        _createAdminViewModel = createAdminViewModel;
        _createDashboardViewModel = createDashboardViewModel;
        _createMenuPlanViewModel = createMenuPlanViewModel;
        _undoRedoService = undoRedoService;
        _recycleBinService = recycleBinService;
        _notificationRepository = notificationRepository;
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _priceHistoryService = priceHistoryService;
        _entreeService = entreeService;
        _currentLocationService = currentLocationService;
        _sessionService = sessionService;
        _syncService = syncService;
        _logger = logger;

        // Subscribe to sync events
        if (_syncService != null)
        {
            _syncService.SyncProgressChanged += OnSyncProgressChanged;
            _syncService.SyncCompleted += OnSyncCompleted;
        }

        // Subscribe to authentication state changes
        if (_sessionService != null)
        {
            _sessionService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        // Subscribe to location changes
        _currentLocationService.CurrentLocationChanged += OnCurrentLocationChanged;

        // Initialize undo/redo state
        UpdateUndoRedoState();

        // Initialize user session state
        UpdateUserSessionState();

        // Start background sync timer (every 5 minutes)
        StartAutoSyncTimer();
    }

    private void OnAuthenticationStateChanged(object? sender, EventArgs e)
    {
        // Update UI when authentication state changes
        UpdateUserSessionState();

        // Restart auto-sync timer based on new state
        StopAutoSyncTimer();
        StartAutoSyncTimer();
    }

    private void OnCurrentLocationChanged(object? sender, EventArgs e)
    {
        // Update location name in UI when location changes
        var currentLocation = _currentLocationService.CurrentLocation;
        if (currentLocation != null)
        {
            CurrentLocationName = currentLocation.Name;
            _logger?.LogInformation("[UI] Location name updated to: {LocationName}", currentLocation.Name);
        }
        else
        {
            CurrentLocationName = "Local Mode";
            _logger?.LogInformation("[UI] Location cleared - showing Local Mode");
        }

        // Reload the current view with new location data
        System.Diagnostics.Debug.WriteLine($"[MainWindow] Location changed - reloading current view");
        if (CurrentView is DashboardViewModel dashboard)
        {
            _ = dashboard.LoadDashboardAsync();
        }
        else if (CurrentView is IngredientsViewModel ingredients)
        {
            _ = ingredients.LoadIngredientsAsync();
        }
        else if (CurrentView is RecipesViewModel recipes)
        {
            _ = recipes.LoadRecipesAsync();
        }
        else if (CurrentView is EntreesViewModel entrees)
        {
            _ = entrees.LoadEntreesAsync();
        }
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
        // Start with Dashboard view after window is set
        CurrentView = _createDashboardViewModel();
        // Load initial counts
        _ = LoadDeletedItemsCountAsync();
        _ = LoadUnreadNotificationsCountAsync();
        _ = LoadRecentItemsAsync();
        _ = LoadAvailableLocationsAsync();
    }

    private void UpdateUndoRedoState()
    {
        CanUndo = _undoRedoService.CanUndo;
        CanRedo = _undoRedoService.CanRedo;

        var undoDesc = _undoRedoService.GetUndoDescription();
        UndoTooltip = undoDesc != null ? $"Undo: {undoDesc}" : "Undo";

        var redoDesc = _undoRedoService.GetRedoDescription();
        RedoTooltip = redoDesc != null ? $"Redo: {redoDesc}" : "Redo";
    }

    public async Task LoadDeletedItemsCountAsync()
    {
        try
        {
            var deletedItems = await _recycleBinService.GetDeletedItemsAsync(_currentLocationService.CurrentLocationId);
            DeletedItemsCount = deletedItems.Count;
        }
        catch
        {
            DeletedItemsCount = 0;
        }
    }

    private async Task LoadUnreadNotificationsCountAsync()
    {
        try
        {
            var notifications = await _notificationRepository.GetByUserAsync(_currentUserId);
            UnreadNotificationsCount = notifications.Count(n => !n.IsRead);
        }
        catch
        {
            UnreadNotificationsCount = 0;
        }
    }

    public async Task RefreshCurrentViewAsync()
    {
        if (CurrentView is IngredientsViewModel ingredientsViewModel)
        {
            await ingredientsViewModel.LoadIngredientsAsync();
        }
        else if (CurrentView is RecipesViewModel recipesViewModel)
        {
            await recipesViewModel.LoadRecipesAsync();
        }
        else if (CurrentView is EntreesViewModel entreesViewModel)
        {
            await entreesViewModel.LoadEntreesAsync();
        }
        else if (CurrentView is DashboardViewModel dashboardViewModel)
        {
            await dashboardViewModel.LoadDashboardAsync();
        }
    }

    private async Task LoadRecentItemsAsync()
    {
        try
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);

            RecentItems.Clear();

            // Last 3 modified ingredients
            var recentIngredients = ingredients
                .OrderByDescending(i => i.ModifiedAt)
                .Take(3)
                .Select(i => new RecentItemModel
                {
                    Name = i.Name,
                    Type = "Ingredient",
                    Icon = "🥬",
                    Date = i.ModifiedAt.ToString("MMM d")
                });

            // Last 2 modified recipes
            var recentRecipes = recipes
                .OrderByDescending(r => r.ModifiedAt)
                .Take(2)
                .Select(r => new RecentItemModel
                {
                    Name = r.Name,
                    Type = "Recipe",
                    Icon = "📋",
                    Date = r.ModifiedAt.ToString("MMM d")
                });

            // Combine and sort by date
            foreach (var item in recentIngredients.Concat(recentRecipes).OrderByDescending(i => i.Date).Take(5))
            {
                RecentItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading recent items");
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = _createDashboardViewModel();
        CurrentBreadcrumb = "🏠 Dashboard";
    }

    [RelayCommand]
    private void NavigateToIngredients()
    {
        if (_mainWindow != null)
            CurrentView = _createIngredientsViewModel(_mainWindow);
        CurrentBreadcrumb = "🏠 Dashboard > 🥕 Ingredients";
    }

    [RelayCommand]
    private void NavigateToRecipes()
    {
        if (_mainWindow != null)
            CurrentView = _createRecipesViewModel(_mainWindow);
        CurrentBreadcrumb = "🏠 Dashboard > 📖 Recipes";
    }

    [RelayCommand]
    private void NavigateToEntrees()
    {
        CurrentView = _createEntreesViewModel();
        CurrentBreadcrumb = "🏠 Dashboard > 🍽️ Entrees";
    }

    [RelayCommand]
    private void NavigateToMenuPlan()
    {
        CurrentView = _createMenuPlanViewModel();
        CurrentBreadcrumb = "🏠 Dashboard > 📅 Menu Planning";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        var settingsViewModel = _createSettingsViewModel();
        if (_mainWindow != null)
        {
            settingsViewModel.SetOwnerWindow(_mainWindow);
        }
        CurrentView = settingsViewModel;
        CurrentBreadcrumb = "🏠 Dashboard > ⚙️ Settings";
    }

    [RelayCommand]
    private void NavigateToAdmin()
    {
        var adminViewModel = _createAdminViewModel();
        if (_mainWindow != null)
        {
            adminViewModel.SetOwnerWindow(_mainWindow);
        }
        CurrentView = adminViewModel;
        CurrentBreadcrumb = "🏠 Dashboard > 👑 Admin";
    }

    [RelayCommand]
    private void NavigateToHelp()
    {
        CurrentView = App.Services?.GetService(typeof(HelpViewModel)) as HelpViewModel ?? new HelpViewModel(null!, null!);
        CurrentBreadcrumb = "🏠 Dashboard > ❓ Help & Tutorial";
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentView = App.Services?.GetService(typeof(ReportsViewModel)) as ReportsViewModel;
        CurrentBreadcrumb = "🏠 Dashboard > 📊 Reports";
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
        CurrentView = App.Services?.GetService(typeof(AboutViewModel)) as AboutViewModel ?? new AboutViewModel();
        CurrentBreadcrumb = "🏠 Dashboard > ℹ️ About";
    }

    [RelayCommand]
    private void NavigateToMultiLocation()
    {
        // Just navigate to admin page - user can click the button there
        NavigateToAdmin();
    }


    [RelayCommand]
    private async Task Undo()
    {
        await _undoRedoService.UndoAsync();
        UpdateUndoRedoState();
    }

    [RelayCommand]
    private async Task Redo()
    {
        await _undoRedoService.RedoAsync();
        UpdateUndoRedoState();
    }

    [RelayCommand]
    private async Task OpenRecycleBin()
    {
        if (_mainWindow == null) return;

        var dialog = new Views.RecycleBinWindow();
        var viewModel = new RecycleBinViewModel(_recycleBinService, _currentLocationService, null, async () =>
        {
            await LoadDeletedItemsCountAsync();
            await RefreshCurrentViewAsync();
        });
        dialog.DataContext = viewModel;

        // Load deleted items
        await viewModel.LoadDeletedItemsAsync();

        await dialog.ShowDialog(_mainWindow);

        // Refresh count and current view after dialog closes
        await LoadDeletedItemsCountAsync();
        await RefreshCurrentViewAsync();
    }

    [RelayCommand]
    private void OpenRecentItem(RecentItemModel? item)
    {
        if (item == null || _mainWindow == null) return;

        try
        {
            if (item.Type == "Ingredient")
            {
                // Navigate to Ingredients view and select the item
                NavigateToIngredients();
                // TODO: Could add SelectIngredientById method to IngredientsViewModel
            }
            else if (item.Type == "Recipe")
            {
                // Navigate to Recipes view
                NavigateToRecipes();
                // TODO: Could add SelectRecipeById method to RecipesViewModel
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening recent item: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenNotifications()
    {
        if (_mainWindow == null) return;

        var dialog = new Views.NotificationsWindow();
        var viewModel = new NotificationsViewModel(_notificationRepository, async () => await LoadUnreadNotificationsCountAsync());
        dialog.DataContext = viewModel;

        // Load notifications
        await viewModel.LoadNotificationsAsync();

        await dialog.ShowDialog(_mainWindow);

        // Refresh count after dialog closes
        await LoadUnreadNotificationsCountAsync();
    }

    [RelayCommand]
    private void ViewCostTrends()
    {
        var viewModel = new CostTrendAnalysisViewModel(
            _ingredientService,
            _recipeService,
            _entreeService,
            _priceHistoryService
        );

        CurrentView = viewModel;
        CurrentBreadcrumb = "🏠 Dashboard > 📊 Analytics & Reports";
    }

    #region User Profile & Sync

    private void UpdateUserSessionState()
    {
        if (_sessionService != null && _sessionService.IsAuthenticated)
        {
            var user = _sessionService.CurrentUser;
            if (user != null)
            {
                UserEmail = user.Email ?? "Unknown";
                UserDisplayName = user.Email?.Split('@')[0] ?? "User";
                IsOfflineMode = false;
                IsAdmin = user.Role == Core.Enums.UserRole.Admin;
                UpdateSyncStatus();
            }
        }
        else
        {
            UserEmail = "Offline Mode";
            UserDisplayName = "Guest";
            IsOfflineMode = true;
            IsAdmin = false;
            SyncStatusIcon = "⚠️";
            SyncStatusText = "Offline";
            SyncStatusColor = "#FF9800";
            // Reset location name to default when offline
            CurrentLocationName = "Local Mode";
        }
        OnPropertyChanged(nameof(OfflineModeToggleText));
    }

    private void UpdateSyncStatus()
    {
        if (IsOfflineMode)
        {
            SyncStatusIcon = "✈️";
            SyncStatusText = "Offline Mode";
            SyncStatusColor = "#FF9800";
        }
        else
        {
            SyncStatusIcon = "✓";
            SyncStatusText = "Synced";
            SyncStatusColor = "#4CAF50";
        }
    }

    [RelayCommand]
    private async Task SyncNow()
    {
        if (_sessionService == null || !_sessionService.IsAuthenticated || _syncService == null)
        {
            return;
        }

        if (_syncService.IsSyncing)
        {
            return; // Already syncing
        }

        try
        {
            SyncStatusIcon = "🔄";
            SyncStatusText = "Syncing...";
            SyncStatusColor = "#2196F3";

            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                SyncStatusIcon = "✓";
                SyncStatusText = $"Synced ({result.ItemsDownloaded}↓ {result.ItemsUploaded}↑)";
                SyncStatusColor = "#4CAF50";

                // Refresh current view to show synced data
                await RefreshCurrentViewAsync();
            }
            else
            {
                SyncStatusIcon = "⚠️";
                SyncStatusText = "Sync Failed";
                SyncStatusColor = "#EF5350";
                _logger?.LogError("Sync failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            SyncStatusIcon = "⚠️";
            SyncStatusText = "Sync Failed";
            SyncStatusColor = "#EF5350";
            System.Diagnostics.Debug.WriteLine($"Sync error: {ex.Message}");
        }
    }

    private void OnSyncProgressChanged(object? sender, SyncProgressEventArgs e)
    {
        SyncStatusText = e.Message;
        // Could update a progress bar here if we add one
    }

    private void OnSyncCompleted(object? sender, SyncCompletedEventArgs e)
    {
        if (e.Result.IsSuccess)
        {
            _logger?.LogInformation("Sync completed successfully");
        }
        else
        {
            _logger?.LogError("Sync failed: {Error}", e.Result.ErrorMessage);
        }
    }

    [RelayCommand]
    private async Task SignIn()
    {
        if (_mainWindow == null) return;

        try
        {
            // Create settings view model to access IServiceProvider and sign-in logic
            var settingsViewModel = _createSettingsViewModel();
            settingsViewModel.SetOwnerWindow(_mainWindow);

            // Execute the full sign-in workflow (includes location selection)
            await settingsViewModel.SignInCommand.ExecuteAsync(null);

            // Refresh UI after sign-in completes
            UpdateUserSessionState();

            // Using Supabase/local config
            _logger?.LogInformation("Using Supabase/local configuration");

            await RefreshCurrentViewAsync();

            // Update current location name in UI
            if (_currentLocationService.CurrentLocation != null)
            {
                CurrentLocationName = _currentLocationService.CurrentLocation.Name;
                _logger?.LogInformation("Sign-in completed. Current location: {LocationName}", CurrentLocationName);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during sign-in from profile flyout");
        }
    }

    [RelayCommand]
    private async Task SwitchLocation()
    {
        if (_mainWindow == null || _sessionService == null || !_sessionService.IsAuthenticated) return;

        try
        {
            // Use _createSettingsViewModel to get access to IServiceProvider
            var settingsViewModel = _createSettingsViewModel();
            var locationService = settingsViewModel.GetType().GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(settingsViewModel) as IServiceProvider;

            var locationServiceInstance = locationService?.GetService(typeof(ILocationService)) as ILocationService;
            if (locationServiceInstance == null) return;

            Core.Models.Location? selectedLocation = null;
            var locationWindow = new Views.LocationSelectorWindow();
            var locationViewModel = new LocationSelectorViewModel(
                locationServiceInstance,
                _sessionService,
                (location) =>
                {
                    selectedLocation = location;
                    locationWindow.SetSelectedLocation(location);
                }
            );

            locationWindow.DataContext = locationViewModel;
            await locationViewModel.InitializeAsync();
            await locationWindow.ShowDialog(_mainWindow);

            if (selectedLocation != null)
            {
                // Update current location
                _currentLocationService.SetCurrentLocation(selectedLocation);
                CurrentLocationName = selectedLocation.Name;

                // If switching to an online location, sync data first
                if (_syncService != null && selectedLocation.UserId != null)
                {
                    _logger?.LogInformation("Syncing data for online location: {LocationName}", selectedLocation.Name);

                    SyncStatusIcon = "🔄";
                    SyncStatusText = "Syncing...";
                    SyncStatusColor = "#2196F3";

                    var result = await _syncService.SyncAsync();

                    if (result.IsSuccess)
                    {
                        SyncStatusIcon = "✓";
                        SyncStatusText = "Synced";
                        SyncStatusColor = "#4CAF50";
                        _logger?.LogInformation("Location sync completed: {Downloaded}↓ {Uploaded}↑",
                            result.ItemsDownloaded, result.ItemsUploaded);
                    }
                    else
                    {
                        SyncStatusIcon = "⚠️";
                        SyncStatusText = "Sync Failed";
                        SyncStatusColor = "#EF5350";
                        _logger?.LogError("Location sync failed: {Error}", result.ErrorMessage);
                    }
                }

                // Refresh current view to show location's data
                await RefreshCurrentViewAsync();

                _logger?.LogInformation("Switched to location: {LocationName}", selectedLocation.Name);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error switching location");
        }
    }

    [RelayCommand]
    private async Task QuickSwitchLocation(Location? location)
    {
        if (location == null) return;

        try
        {
            _currentLocationService.SetCurrentLocation(location);
            CurrentLocationName = location.Name;
            await RefreshCurrentViewAsync();
            _logger?.LogInformation("Switched to location: {LocationName}", location.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error quick-switching location");
        }
    }

    [RelayCommand]
    private void OpenLocationSettings()
    {
        // Navigate to settings view (which has location management)
        NavigateToSettings();
    }

    public async Task LoadAvailableLocationsAsync()
    {
        try
        {
            // Get ILocationService from DI - we need to find it through the service provider
            // For now, use the ILocationRepository directly
            using var scope = App.Services?.CreateScope();
            if (scope == null) return;

            var locationService = scope.ServiceProvider.GetService<ILocationService>();
            if (locationService == null) return;

            var locations = await locationService.GetAllLocationsAsync();
            AvailableLocations.Clear();
            foreach (var loc in locations.Where(l => l.IsActive).OrderBy(l => l.Name))
            {
                AvailableLocations.Add(loc);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load available locations");
        }
    }

    [RelayCommand]
    private async Task SignOut()
    {
        if (_sessionService == null || _mainWindow == null)
            return;

        try
        {
            // logFile variable removed - using SafeFileLogger
            SafeFileLogger.Log("signout", "===== SIGN OUT STARTED =====");
            SafeFileLogger.Log("signout", "Current location before signout: {CurrentLocationName}");

            StopAutoSyncTimer();
            await _sessionService.SignOutAsync();
            SafeFileLogger.Log("signout", "Session SignOut completed");

            // Clear current location after sign-out (will trigger offline mode)
            _logger?.LogInformation("Clearing current location after sign-out...");
            SafeFileLogger.Log("signout", "Calling SetCurrentLocation(null)");
            _currentLocationService.SetCurrentLocation(null);
            SafeFileLogger.Log("signout", "Location cleared");
            await RefreshCurrentViewAsync();
            SafeFileLogger.Log("signout", "View refreshed");

            // Update UI state to offline mode
            UpdateUserSessionState();
            SafeFileLogger.Log("signout", "UI state updated");
            SafeFileLogger.Log("signout", "Current location after signout: {CurrentLocationName}");
            SafeFileLogger.Log("signout", "===== SIGN OUT COMPLETED =====");

            _logger?.LogInformation("User signed out successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during sign out");
        }
    }

    private void StartAutoSyncTimer()
    {
        if (_sessionService == null || !_sessionService.IsAuthenticated || _syncService == null || IsOfflineMode)
        {
            return;
        }

        // Sync every 5 minutes
        _autoSyncTimer = new Timer(5 * 60 * 1000); // 5 minutes in milliseconds
        _autoSyncTimer.Elapsed += async (sender, e) => await AutoSyncTimerElapsed();
        _autoSyncTimer.AutoReset = true;
        _autoSyncTimer.Start();
        _logger?.LogInformation("Auto-sync timer started (5 minute interval)");
    }

    private void StopAutoSyncTimer()
    {
        if (_autoSyncTimer != null)
        {
            _autoSyncTimer.Stop();
            _autoSyncTimer.Dispose();
            _autoSyncTimer = null;
            _logger?.LogInformation("Auto-sync timer stopped");
        }
    }

    private async Task AutoSyncTimerElapsed()
    {
        if (_syncService == null || _syncService.IsSyncing || IsOfflineMode)
        {
            return;
        }

        try
        {
            _logger?.LogInformation("Running automatic background sync...");
            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                _logger?.LogInformation("Background sync completed: {Downloaded}↓ {Uploaded}↑",
                    result.ItemsDownloaded, result.ItemsUploaded);

                // Silently refresh current view
                await RefreshCurrentViewAsync();
            }
            else
            {
                _logger?.LogWarning("Background sync failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Background sync error");
        }
    }

    #endregion
}