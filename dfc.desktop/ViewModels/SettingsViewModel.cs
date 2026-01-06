using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Helpers;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Desktop.Services;
using Dfc.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Location = Dfc.Core.Models.Location;

namespace Dfc.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IExcelExportService _excelExportService;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IDatabaseBackupService? _backupService;
    private readonly IAutoUpdateService? _updateService;
    private readonly IUserSessionService _sessionService;
    private readonly ISyncService? _syncService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IStatusNotificationService _notificationService;
    private readonly IIngredientConversionRepository? _conversionRepository;
    private readonly SupabasePhotoService? _photoService;
    private readonly ISpoonyService? _spoonyService;
    private readonly ILogger<SettingsViewModel>? _logger;
    private Window? _ownerWindow;
    private UserPreferences? _currentPreferences;

    // Smart auto-sync fields
    private Timer? _autoSyncTimer;
    private bool _hasUnsyncedChanges;
    private DateTime _lastActivityTime = DateTime.UtcNow;
    private DateTime? _lastSyncTime;
    private const int SYNC_INTERVAL_MINUTES = 5;
    private const int IDLE_THRESHOLD_MINUTES = 10;


    [ObservableProperty]
    private int _totalConversions;

    [ObservableProperty]
    private int _usdaConversions;

    [ObservableProperty]
    private int _userDefinedConversions;

    [ObservableProperty]
    private decimal _conversionCoveragePercent;

    [ObservableProperty]
    private string _cacheSize = "0 MB";

    [ObservableProperty]
    private bool _isCacheClearing;

    [ObservableProperty]
    private bool _isSpoonyEnabled = false;

    partial void OnIsSpoonyEnabledChanged(bool value)
    {
        if (_spoonyService != null)
        {
            _spoonyService.IsSpoonyEnabled = value;
        }
    }

    // ============================================
    // LOCATION MANAGEMENT
    // ============================================

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Location> _locations = new();

    [ObservableProperty]
    private Location? _selectedLocation;

    [ObservableProperty]
    private string _currentLocationName = "Default Location";

    public SettingsViewModel(
        IServiceProvider serviceProvider,
        IExcelExportService excelExportService,
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService,
        IPriceHistoryService priceHistoryService,
        ILocalSettingsService localSettingsService,
        IStatusNotificationService notificationService,
        IDatabaseBackupService? backupService = null,
        IAutoUpdateService? updateService = null,
        ILogger<SettingsViewModel>? logger = null,
        IIngredientConversionRepository? conversionRepository = null,
        SupabasePhotoService? photoService = null)
    {
        _serviceProvider = serviceProvider;
        _excelExportService = excelExportService;
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
        _priceHistoryService = priceHistoryService;
        _localSettingsService = localSettingsService;
        _notificationService = notificationService;
        _conversionRepository = conversionRepository;
        _backupService = backupService;
        _updateService = updateService;
        _logger = logger;
        _sessionService = (IUserSessionService)serviceProvider.GetRequiredService(typeof(IUserSessionService));
        _syncService = serviceProvider.GetService(typeof(ISyncService)) as ISyncService;
        _currentLocationService = (ICurrentLocationService)serviceProvider.GetRequiredService(typeof(ICurrentLocationService));
        _photoService = photoService;
        _spoonyService = serviceProvider.GetService(typeof(ISpoonyService)) as ISpoonyService;

        // Initialize Spoony setting from service
        if (_spoonyService != null)
        {
            IsSpoonyEnabled = _spoonyService.IsSpoonyEnabled;
        }

        _ = LoadSettingsAsync();
        _ = CheckAuthenticationStatusAsync();
        _ = InitializeAutoSyncAsync();
        _ = LoadCacheSizeAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            _currentPreferences = await _localSettingsService.LoadSettingsAsync();
            IsDarkMode = _currentPreferences.Theme == "Dark";
        }
        catch (Exception)
        {
            _currentPreferences = null;
        }
    }

    public void SetOwnerWindow(Window window)
    {
        _ownerWindow = window;
    }

    [ObservableProperty]
    private bool _enableCloudSync;

    [ObservableProperty]
    private bool _enableAutoUpdate = true;

    [ObservableProperty]
    private bool _exportIngredients;

    [ObservableProperty]
    private bool _exportRecipes;

    [ObservableProperty]
    private bool _exportEntrees;

    [ObservableProperty]
    private double _greenThreshold = 30.0;

    [ObservableProperty]
    private double _yellowThreshold = 40.0;

    [ObservableProperty]
    private bool _isDarkMode;

    public string AppVersion
    {
        get
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly()
                .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "Unknown";
            
            // Extract build number (e.g., "0.8.1-build0026" -> "Build 0026")
            if (version.Contains("-build"))
            {
                var buildPart = version.Split('-')[1].Replace("build", "");
                return $"Build {buildPart}";
            }
            return version;
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        var app = Avalonia.Application.Current;
        if (app != null)
        {
            app.RequestedThemeVariant = value
                ? Avalonia.Styling.ThemeVariant.Dark
                : Avalonia.Styling.ThemeVariant.Light;
        }

        _ = SaveThemePreferenceAsync(value);
    }

    private async Task SaveThemePreferenceAsync(bool isDark)
    {
        try
        {
            if (_currentPreferences == null)
            {
                _currentPreferences = await _localSettingsService.LoadSettingsAsync();
            }

            _currentPreferences.Theme = isDark ? "Dark" : "Light";
            await _localSettingsService.SaveSettingsAsync(_currentPreferences);
        }
        catch (Exception)
        {
            // Silently fail - theme is already applied to UI
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            if (_currentPreferences == null)
            {
                _currentPreferences = await _localSettingsService.LoadSettingsAsync();
            }

            _currentPreferences.Theme = IsDarkMode ? "Dark" : "Light";
            await _localSettingsService.SaveSettingsAsync(_currentPreferences);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Settings save failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportData()
    {
        if (_ownerWindow == null) return;

        if (!ExportIngredients && !ExportRecipes && !ExportEntrees)
        {
            _notificationService.ShowWarning("Please select at least one data type to export");
            return;
        }

        try
        {
            var file = await _ownerWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export Data to Excel",
                SuggestedFileName = $"Desktop Food Cost_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExtension = "xlsx",
                ShowOverwritePrompt = true
            });

            if (file == null) return;

            var filePath = file.Path.LocalPath;
            var exportedTypes = new System.Collections.Generic.List<string>();

            if (ExportIngredients)
            {
                var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
                if (ingredients.Any())
                {
                    await _excelExportService.ExportIngredientsToExcelAsync(ingredients.ToList(), filePath);
                    exportedTypes.Add("Ingredients");
                }
            }

            if (ExportRecipes)
            {
                var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);
                if (recipes.Any())
                {
                    var recipesPath = exportedTypes.Any()
                        ? Path.Combine(Path.GetDirectoryName(filePath)!, Path.GetFileNameWithoutExtension(filePath) + "_Recipes.xlsx")
                        : filePath;

                    await _excelExportService.ExportRecipesToExcelAsync(recipes.ToList(), recipesPath);
                    exportedTypes.Add("Recipes");
                }
            }

            if (ExportEntrees)
            {
                var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationService.CurrentLocationId);
                if (entrees.Any())
                {
                    var entreesPath = exportedTypes.Any()
                        ? Path.Combine(Path.GetDirectoryName(filePath)!, Path.GetFileNameWithoutExtension(filePath) + "_Entrees.xlsx")
                        : filePath;

                    await _excelExportService.ExportEntreesToExcelAsync(entrees.ToList(), entreesPath);
                    exportedTypes.Add("Entrees");
                }
            }

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directoryPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
        catch (Exception ex)
        {
            var friendlyError = ErrorMessageHelper.GetDatabaseErrorMessage(ex, "exporting data");
            _notificationService.ShowError(friendlyError);
        }
    }

    [RelayCommand]
    private async Task ImportData()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task BackupDatabase()
    {
        if (_ownerWindow == null || _backupService == null) return;

        try
        {
            var file = await _ownerWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Backup Database (JSON Format)",
                SuggestedFileName = $"Desktop Food Cost_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                DefaultExtension = "json",
                ShowOverwritePrompt = true,
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("JSON Backup")
                    {
                        Patterns = new[] { "*.json" }
                    },
                    new Avalonia.Platform.Storage.FilePickerFileType("Compressed Backup")
                    {
                        Patterns = new[] { "*.zip" }
                    }
                }
            });

            if (file == null) return;

            var filePath = file.Path.LocalPath;
            BackupResult result;

            // Check if user chose zip format
            if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                result = await _backupService.CreateCompressedBackupAsync(filePath);
            }
            else
            {
                result = await _backupService.CreateBackupAsync(filePath);
            }

            if (result.IsSuccess)
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = directoryPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }

            }
            else
            {
                _notificationService.ShowError($"Backup failed: {result.ErrorMessage ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            var friendlyError = ErrorMessageHelper.GetDatabaseErrorMessage(ex, "backing up database");
            _notificationService.ShowError(friendlyError);
        }
    }

    [RelayCommand]
    private async Task RestoreDatabase()
    {
        if (_ownerWindow == null || _backupService == null) return;

        try
        {
            var files = await _ownerWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Restore Database from Backup",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Backup Files")
                    {
                        Patterns = new[] { "*.json", "*.zip" }
                    }
                }
            });

            if (files.Count == 0) return;

            var confirmDialog = new Window
            {
                Title = "Confirm Restore",
                Width = 450,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text = "⚠️ Warning: This will merge the backup data with your current database.\n\n" +
                       "Existing items with the same ID will be updated.\n" +
                       "New items from the backup will be added.\n\n" +
                       "Do you want to continue?",
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontWeight = Avalonia.Media.FontWeight.Bold
            });

            var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };

            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            cancelButton.Click += (s, e) => confirmDialog.Close(false);

            var confirmButton = new Button { Content = "Restore", Width = 80, Background = Avalonia.Media.Brushes.Orange, Foreground = Avalonia.Media.Brushes.White };
            confirmButton.Click += (s, e) => confirmDialog.Close(true);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(confirmButton);
            panel.Children.Add(buttonPanel);
            confirmDialog.Content = panel;

            var confirmed = await confirmDialog.ShowDialog<bool>(_ownerWindow);
            if (!confirmed) return;

            var backupPath = files[0].Path.LocalPath;
            var restoreResult = await _backupService.RestoreFromBackupAsync(backupPath, clearExistingData: false);

            if (restoreResult.IsSuccess)
            {
                _notificationService.ShowSuccess($"Restored {restoreResult.TotalItemsRestored} items from backup");
            }
            else
            {
                _notificationService.ShowError($"Restore failed: {restoreResult.ErrorMessage ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            var friendlyError = ErrorMessageHelper.GetDatabaseErrorMessage(ex, "restoring database");
            _notificationService.ShowError(friendlyError);
        }
    }


    [RelayCommand]
    private async Task OpenSyncMonitor()
    {
        if (_ownerWindow == null) return;

        try
        {
            var modificationService = _serviceProvider.GetService(typeof(ILocalModificationService)) as ILocalModificationService;
            if (modificationService == null)
            {
                _logger?.LogWarning("LocalModificationService not available");
                return;
            }

            if (_syncService == null)
            {
                _logger?.LogWarning("SyncService not available");
                _notificationService.ShowWarning("Sync service is not available. Please sign in to use sync features.");
                return;
            }

            var dialog = new Views.SyncMonitorWindow();
            var viewModel = new SyncMonitorViewModel(
                modificationService,
                _syncService,
                _currentLocationService,
                () => dialog.Close(),
                _serviceProvider.GetService(typeof(ILogger<SyncMonitorViewModel>)) as ILogger<SyncMonitorViewModel>
            );
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error opening sync monitor");
        }
    }

    [RelayCommand]
    private async Task ManageApprovals()
    {
        if (_ownerWindow == null) return;

        try
        {
            var workflowRepository = _serviceProvider.GetService(typeof(IApprovalWorkflowRepository)) as IApprovalWorkflowRepository;
            var workflowService = _serviceProvider.GetService(typeof(IApprovalWorkflowService)) as IApprovalWorkflowService;
            var permissionService = _serviceProvider.GetService(typeof(IPermissionService)) as IPermissionService;
            var sessionService = _serviceProvider.GetService(typeof(IUserSessionService)) as IUserSessionService;

            if (workflowRepository == null || workflowService == null || permissionService == null || sessionService == null) return;

            var dialog = new Views.ApprovalWorkflowsWindow();
            var viewModel = new ApprovalWorkflowsViewModel(
                workflowRepository,
                workflowService,
                permissionService,
                sessionService,
                () => dialog.Close());
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error opening approval workflows");
        }
    }

    [RelayCommand]
    private async Task ViewCostTrends()
    {
        if (_ownerWindow == null) return;

        var dialog = new CostTrendAnalysisWindow();
        var viewModel = new CostTrendAnalysisViewModel(
            _ingredientService,
            _recipeService,
            _entreeService,
            _priceHistoryService
        );

        dialog.DataContext = viewModel;
        await dialog.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task OpenBackupManager()
    {
        if (_ownerWindow == null) return;

        var backupService = _serviceProvider.GetService(typeof(IBackupService)) as IBackupService;
        if (backupService == null) return;

        var dialog = new BackupManagerWindow();
        var viewModel = new BackupManagerViewModel(backupService);
        dialog.DataContext = viewModel;
        await dialog.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        if (_ownerWindow == null || _updateService == null) return;

        try
        {
            _notificationService.ShowInfo("Checking for updates...");

            // Check for updates
            var result = await _updateService.CheckForUpdateAsync();

            // Create and show the update notification window
            var viewModel = new UpdateNotificationViewModel(_updateService, null);
            viewModel.Initialize(result);

            var updateWindow = new UpdateNotificationWindow(viewModel);
            await updateWindow.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to check for updates");
            _notificationService.ShowError($"Failed to check for updates: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ReportBug()
    {
        if (_ownerWindow == null) return;

        try
        {
            var bugReportService = _serviceProvider.GetService<IBugReportService>();
            if (bugReportService == null)
            {
                _notificationService.ShowWarning("Bug reporting service is not available. Please contact support directly.");
                return;
            }

            var bugReportWindow = new BugReportWindow(
                bugReportService,
                _notificationService,
                null
            );

            await bugReportWindow.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open bug report window");
            _notificationService.ShowError($"Failed to open bug report form: {ex.Message}");
        }
    }


    [ObservableProperty]
    private bool _isSignedIn;

    [ObservableProperty]
    private string? _userEmail;

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _syncStatus = "Not synced";

    [ObservableProperty]
    private string _lastSyncedText = "Never";

    private async Task CheckAuthenticationStatusAsync()
    {
        try
        {
            await Task.CompletedTask; // Placeholder for async
            IsSignedIn = _sessionService.IsAuthenticated;
            UserEmail = _sessionService.CurrentUser?.Email;
        }
        catch
        {
            IsSignedIn = false;
            UserEmail = null;
        }
    }

    /// <summary>
    /// Initialize auto-sync if user is already signed in
    /// </summary>
    private async Task InitializeAutoSyncAsync()
    {
        try
        {
            if (_sessionService.IsAuthenticated && _syncService != null)
            {
                // Perform initial sync on startup
                await PerformSyncAsync();

                // Start auto-sync timer
                StartAutoSyncTimer();
            }
        }
        catch (Exception)
        {
            // Silently fail - user can manually sync if needed
        }
    }

    /// <summary>
    /// Start the auto-sync timer (5-minute intervals)
    /// </summary>
    private void StartAutoSyncTimer()
    {
        if (_autoSyncTimer != null)
        {
            _autoSyncTimer.Dispose();
        }

        var interval = TimeSpan.FromMinutes(SYNC_INTERVAL_MINUTES);
        _autoSyncTimer = new Timer(
            async _ => await OnAutoSyncTimerElapsed(),
            null,
            interval,
            interval
        );

        _lastActivityTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Stop the auto-sync timer
    /// </summary>
    private void StopAutoSyncTimer()
    {
        _autoSyncTimer?.Dispose();
        _autoSyncTimer = null;
    }

    /// <summary>
    /// Called every 5 minutes by the timer
    /// </summary>
    private async Task OnAutoSyncTimerElapsed()
    {
        try
        {
            // Skip if not signed in
            if (!_sessionService.IsAuthenticated || _syncService == null)
            {
                return;
            }

            // Skip if already syncing
            if (_syncService.IsSyncing)
            {
                return;
            }

            // Skip if user is idle (no activity for 10+ minutes)
            var idleTime = DateTime.UtcNow - _lastActivityTime;
            if (idleTime.TotalMinutes >= IDLE_THRESHOLD_MINUTES)
            {
                return;
            }

            // Skip if no unsynced changes
            if (!_hasUnsyncedChanges)
            {
                return;
            }

            // Perform sync
            await PerformSyncAsync();
        }
        catch (Exception)
        {
            // Silently fail - will retry in next interval
        }
    }

    /// <summary>
    /// Perform a sync operation
    /// </summary>
    private async Task PerformSyncAsync()
    {
        if (_syncService == null || !_sessionService.IsAuthenticated)
        {
            return;
        }

        try
        {
            IsSyncing = true;
            SyncStatus = "Syncing...";

            var result = await _syncService.SyncAsync();

            if (result.IsSuccess)
            {
                _hasUnsyncedChanges = false;
                _lastSyncTime = DateTime.UtcNow;
                SyncStatus = "Synced";
                UpdateLastSyncedText();
            }
            else
            {
                SyncStatus = "Sync failed";
            }
        }
        catch (Exception)
        {
            SyncStatus = "Sync error";
        }
        finally
        {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Update the "Last synced: X ago" text
    /// </summary>
    private void UpdateLastSyncedText()
    {
        if (_lastSyncTime == null)
        {
            LastSyncedText = "Never";
            return;
        }

        var elapsed = DateTime.UtcNow - _lastSyncTime.Value;

        if (elapsed.TotalMinutes < 1)
        {
            LastSyncedText = "Just now";
        }
        else if (elapsed.TotalMinutes < 60)
        {
            var minutes = (int)elapsed.TotalMinutes;
            LastSyncedText = $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }
        else if (elapsed.TotalHours < 24)
        {
            var hours = (int)elapsed.TotalHours;
            LastSyncedText = $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }
        else
        {
            var days = (int)elapsed.TotalDays;
            LastSyncedText = $"{days} day{(days == 1 ? "" : "s")} ago";
        }
    }

    /// <summary>
    /// Mark that data has changed and needs to be synced
    /// </summary>
    public void MarkDataChanged()
    {
        _hasUnsyncedChanges = true;
        _lastActivityTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Manual sync now button
    /// </summary>
    [RelayCommand]
    private async Task SyncNow()
    {
        if (!IsSignedIn || _syncService == null)
        {
            _notificationService.ShowWarning("Please sign in to use cloud sync features");
            return;
        }

        // DIAGNOSTIC: Show Location ID before sync
        var currentLocationId = _currentLocationService.CurrentLocationId;
        var currentLocation = _currentLocationService.CurrentLocation;

        // CRITICAL: Check if LocalModificationService is tracking changes
        var modService = _serviceProvider.GetService(typeof(ILocalModificationService)) as ILocalModificationService;
        int unsyncedCount = 0;
        if (modService != null)
        {
            var unsyncedMods = await modService.GetUnsyncedModificationsAsync(currentLocationId);
            unsyncedCount = unsyncedMods.Count;

            Debug.WriteLine("═══════════════════════════════════════════════════");
            Debug.WriteLine("[SYNC DIAGNOSTIC] Pre-Sync Check");
            Debug.WriteLine($"[SYNC DIAGNOSTIC] Current Location ID: {currentLocationId}");
            Debug.WriteLine($"[SYNC DIAGNOSTIC] Current Location Name: {currentLocation?.Name ?? "Unknown"}");
            Debug.WriteLine($"[SYNC DIAGNOSTIC] Signed in as: {UserEmail}");
            Debug.WriteLine($"[SYNC DIAGNOSTIC] Unsynced Modifications: {unsyncedCount}");

            if (unsyncedCount > 0)
            {
                // Show breakdown by type
                var byType = unsyncedMods.GroupBy(m => m.EntityType).ToList();
                foreach (var group in byType)
                {
                    var creates = group.Count(m => m.ModificationType == Core.Enums.ModificationType.Create);
                    var updates = group.Count(m => m.ModificationType == Core.Enums.ModificationType.Update);
                    var deletes = group.Count(m => m.ModificationType == Core.Enums.ModificationType.Delete);
                    Debug.WriteLine($"[SYNC DIAGNOSTIC]   {group.Key}: {group.Count()} total (Create: {creates}, Update: {updates}, Delete: {deletes})");
                }
            }
            else
            {
                Debug.WriteLine($"[SYNC DIAGNOSTIC]   ⚠️ WARNING: No unsynced modifications found!");
                Debug.WriteLine($"[SYNC DIAGNOSTIC]   This means no data will be uploaded to Firebase.");
            }
            Debug.WriteLine("═══════════════════════════════════════════════════");
        }
        else
        {
            Debug.WriteLine("═══════════════════════════════════════════════════");
            Debug.WriteLine("[SYNC DIAGNOSTIC] ❌ ERROR: LocalModificationService is NULL!");
            Debug.WriteLine("[SYNC DIAGNOSTIC] Modifications are NOT being tracked!");
            Debug.WriteLine("═══════════════════════════════════════════════════");
        }

        // Log to file for troubleshooting
        await LogSyncDiagnosticsAsync(currentLocationId, currentLocation?.Name, unsyncedCount);

        await PerformSyncAsync();

        if (SyncStatus == "Synced")
        {
            _notificationService.ShowSuccess($"Sync completed successfully\nLocation ID: {currentLocationId}\nUnsynced Mods: {unsyncedCount}");
        }
        else
        {
            _notificationService.ShowError("Sync failed. Please try again later");
        }
    }

    /// <summary>
    /// Force upload ALL existing data - use when modifications weren't tracked
    /// </summary>
    [RelayCommand]
    private async Task ForceUploadAll()
    {
        Debug.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Debug.WriteLine("║ [FORCE UPLOAD] Button clicked!                                ║");
        Debug.WriteLine("╚═══════════════════════════════════════════════════════════════╝");

        if (!IsSignedIn || _syncService == null)
        {
            Debug.WriteLine("[FORCE UPLOAD] ERROR: Not signed in or sync service is null");
            _notificationService.ShowWarning("Please sign in to use force upload");
            return;
        }

        Debug.WriteLine("[FORCE UPLOAD] User is signed in, starting upload...");

        // Perform force upload
        SyncStatus = "Syncing...";
        _notificationService.ShowInfo("Starting force upload of all data...");

        var result = await _syncService.ForceUploadAllAsync();

        if (result.IsSuccess)
        {
            SyncStatus = "Synced";
            _notificationService.ShowSuccess($"Force upload completed!\n{result.ItemsUploaded} items uploaded to Firebase.\n\nOther devices can now sync to download this data.");
        }
        else
        {
            SyncStatus = "Error";
            _notificationService.ShowError($"Force upload failed: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Log sync diagnostics to file for troubleshooting
    /// </summary>
    private async Task LogSyncDiagnosticsAsync(Guid locationId, string? locationName, int unsyncedCount)
    {
        try
        {
            var logDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "OneDrive",
                "Desktop",
                "Logs"
            );

            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            var logFile = System.IO.Path.Combine(logDirectory, $"sync_diagnostics_{DateTime.Now:yyyyMMdd}.txt");

            var logEntry = $@"
═══════════════════════════════════════════════════════════════
Sync Diagnostic - {DateTime.Now:yyyy-MM-dd HH:mm:ss}
═══════════════════════════════════════════════════════════════
Device: {Environment.MachineName}
User Email: {UserEmail}
Selected Location: {locationName ?? "Unknown"}
Location ID: {locationId}
Is Signed In: {IsSignedIn}
Sync Status: {SyncStatus}
Unsynced Modifications: {unsyncedCount}
{(unsyncedCount == 0 ? "⚠️ WARNING: No unsynced modifications! Nothing will be uploaded to Firebase." : "")}
═══════════════════════════════════════════════════════════════

";

            await System.IO.File.AppendAllTextAsync(logFile, logEntry);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write sync diagnostic log: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SignIn()
    {
        if (_ownerWindow == null) return;

        try
        {
            // Step 1: Show login window
            await ShowLoginWindowAsync();

            // Step 2: If signed in successfully, proceed with post-login workflow
            if (IsSignedIn)
            {
                // Step 3: Sync restaurants from cloud
                SyncResult? syncResult;
                try
                {
                    syncResult = await SyncRestaurantsAfterLoginWithResultAsync();
                    if (syncResult == null)
                        return; // Sync failed, user was signed out
                }
                catch (Exception syncEx)
                {
                    Debug.WriteLine($"Restaurant sync exception: {syncEx.Message}");
                    _logger?.LogWarning(syncEx, "Restaurant sync failed");
                    _notificationService.ShowWarning($"Could not sync locations: {syncEx.Message}. You can still use local data.");
                    // Continue anyway - user can work with local locations
                    syncResult = SyncResult.Success();
                }

                // Step 4: Try to restore last selected location, otherwise show location selector
                Location? selectedLocation = null;
                
                // Check if we have a saved last location ID
                var lastLocationId = _currentLocationService.LoadLastLocationId();
                if (lastLocationId.HasValue)
                {
                    var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
                    if (locationService != null)
                    {
                        var locations = await locationService.GetAllLocationsAsync();
                        var lastLocation = locations.FirstOrDefault(l => 
                            l.Id == lastLocationId.Value && 
                            l.IsActive && 
                            l.UserId != null // Ensure it's an online location user still has access to
                        );
                        
                        if (lastLocation != null)
                        {
                            // Use last selected location, skip selector
                            selectedLocation = lastLocation;
                            Debug.WriteLine($"[SETTINGS] ✓ Restored last selected location, skipping selector: {lastLocation.Name}");
                            _logger?.LogInformation("Restored last selected location: {LocationName}", lastLocation.Name);
                        }
                        else
                        {
                            Debug.WriteLine("[SETTINGS] Last location no longer valid, showing selector");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("[SETTINGS] No saved location, showing selector");
                }
                
                // Only show selector if last location not found or invalid
                if (selectedLocation == null)
                {
                    try
                    {
                        selectedLocation = await ShowLocationSelectorAsync();
                        if (selectedLocation == null)
                            return; // User cancelled
                    }
                    catch (Exception selectorEx)
                    {
                        Debug.WriteLine($"Location selector error: {selectorEx.Message}");
                        _logger?.LogWarning(selectorEx, "Location selector failed");
                        _notificationService.ShowWarning($"Could not show location selector: {selectorEx.Message}. Please select a location from Settings.");
                        return;
                    }
                }

                // Step 5: Set current location and refresh UI
                try
                {
                    await SetLocationAndRefreshUIAsync(selectedLocation);
                }
                catch (Exception setLocationEx)
                {
                    Debug.WriteLine($"Set location error: {setLocationEx.Message}");
                    _logger?.LogWarning(setLocationEx, "Failed to set location");
                    _notificationService.ShowWarning($"Could not set location: {setLocationEx.Message}. Please try selecting a location again.");
                    return;
                }

                // DIAGNOSTIC: Verify location was set correctly
                Debug.WriteLine("=== DIAGNOSTIC: Location Selection ===");
                Debug.WriteLine($"Selected Location - Name: {selectedLocation.Name}, Id: {selectedLocation.Id}, UserId: {selectedLocation.UserId ?? "NULL"}");
                Debug.WriteLine($"CurrentLocationService.CurrentLocationId: {_currentLocationService.CurrentLocationId}");
                Debug.WriteLine($"Location match: {_currentLocationService.CurrentLocationId == selectedLocation.Id}");
                _logger?.LogInformation("=== DIAGNOSTIC: Location Selection ===");
                _logger?.LogInformation("Selected Location - Name: {Name}, Id: {Id}, UserId: {UserId}",
                    selectedLocation.Name, selectedLocation.Id, selectedLocation.UserId ?? "NULL");
                _logger?.LogInformation("CurrentLocationService.CurrentLocationId: {CurrentLocationId}",
                    _currentLocationService.CurrentLocationId);
                _logger?.LogInformation("Location match: {Match}",
                    _currentLocationService.CurrentLocationId == selectedLocation.Id);

                // Step 5b: If online location, perform full data sync immediately
                if (_syncService != null && selectedLocation.UserId != null)
                {
                    Debug.WriteLine("=== DIAGNOSTIC: Starting data sync ===");
                    Debug.WriteLine($"Syncing data for online location: {selectedLocation.Name} (ID: {selectedLocation.Id})");
                    _logger?.LogInformation("=== DIAGNOSTIC: Starting data sync ===");
                    _logger?.LogInformation("Syncing data for online location: {LocationName} (ID: {LocationId})",
                        selectedLocation.Name, selectedLocation.Id);

                    SyncResult dataSyncResult;
                    try
                    {
                        dataSyncResult = await _syncService.SyncAsync();
                    }
                    catch (Exception syncEx)
                    {
                        Debug.WriteLine($"Sync exception: {syncEx.Message}");
                        _logger?.LogWarning(syncEx, "Sync threw exception during sign-in");
                        dataSyncResult = SyncResult.Failure($"Sync error: {syncEx.Message}");
                    }

                    Debug.WriteLine("=== DIAGNOSTIC: Sync completed ===");
                    Debug.WriteLine($"Sync Success: {dataSyncResult.IsSuccess}, Downloaded: {dataSyncResult.ItemsDownloaded}, Uploaded: {dataSyncResult.ItemsUploaded}");
                    if (!string.IsNullOrEmpty(dataSyncResult.ErrorMessage))
                    {
                        Debug.WriteLine($"Sync ErrorMessage: {dataSyncResult.ErrorMessage}");
                    }
                    _logger?.LogInformation("=== DIAGNOSTIC: Sync completed ===");
                    _logger?.LogInformation("Sync Success: {Success}, Downloaded: {Downloaded}, Uploaded: {Uploaded}",
                        dataSyncResult.IsSuccess, dataSyncResult.ItemsDownloaded, dataSyncResult.ItemsUploaded);
                    if (!string.IsNullOrEmpty(dataSyncResult.ErrorMessage))
                    {
                        _logger?.LogWarning("Sync ErrorMessage: {ErrorMessage}", dataSyncResult.ErrorMessage);
                    }

                    // Show visible sync result to user
                    if (dataSyncResult.IsSuccess)
                    {
                        if (dataSyncResult.ItemsDownloaded == 0 && dataSyncResult.ItemsUploaded == 0)
                        {
                            _notificationService.ShowInfo($"Connected to {selectedLocation.Name}. No data found in cloud for this location. You can start adding data now.");
                        }
                        else
                        {
                            _notificationService.ShowSuccess($"Sync completed: {dataSyncResult.ItemsDownloaded} items downloaded, {dataSyncResult.ItemsUploaded} items uploaded");
                        }
                    }
                    else
                    {
                        _notificationService.ShowWarning($"Sync issue: {dataSyncResult.ErrorMessage ?? "Unknown error"}. You can continue working offline.");
                    }

                    // Refresh UI again to show synced data
                    if (_ownerWindow is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel mainViewModel)
                    {
                        await mainViewModel.RefreshCurrentViewAsync();
                        _logger?.LogInformation("Refreshed UI after location sync");
                    }
                }
                else
                {
                    Debug.WriteLine("=== DIAGNOSTIC: Sync skipped ===");
                    Debug.WriteLine($"SyncService exists: {_syncService != null}, UserId is not null: {selectedLocation.UserId != null}");
                    _logger?.LogWarning("=== DIAGNOSTIC: Sync skipped ===");
                    _logger?.LogWarning("SyncService exists: {SyncServiceExists}, UserId is not null: {UserIdNotNull}",
                        _syncService != null, selectedLocation.UserId != null);
                }

                // Step 6: Start auto-sync
                try
                {
                    await InitializeAutoSyncAfterLoginAsync();
                }
                catch (Exception autoSyncEx)
                {
                    Debug.WriteLine($"Auto-sync initialization failed (non-critical): {autoSyncEx.Message}");
                    _logger?.LogWarning(autoSyncEx, "Auto-sync initialization failed (non-critical)");
                    // Don't fail sign-in if auto-sync fails
                }
            }
        }
        catch (Exception ex)
        {
            // Check if user is actually signed in despite the error
            if (IsSignedIn)
            {
                // User is signed in, but post-login workflow failed
                Debug.WriteLine($"Post-login error (user is signed in): {ex.Message}");
                _logger?.LogWarning(ex, "Post-login workflow error (user successfully authenticated)");
                _notificationService.ShowWarning($"Signed in successfully, but encountered an issue: {ex.Message}");
            }
            else
            {
                // Actual authentication failure
                var friendlyError = ErrorMessageHelper.GetAuthenticationErrorMessage(ex);
                _notificationService.ShowError($"Sign in failed: {friendlyError}");
                _logger?.LogError(ex, "Sign-in authentication failed");
            }
        }
    }

    /// <summary>
    /// Show the login window and wait for user to sign in
    /// </summary>
    private async Task ShowLoginWindowAsync()
    {
        var loginWindow = new LoginWindow();
        var loginViewModel = new LoginViewModel(
            _sessionService,
            _localSettingsService,
            async (offline) =>
            {
                if (!offline)
                {
                    await CheckAuthenticationStatusAsync();
                }
            },
            loginWindow
        );

        loginWindow.DataContext = loginViewModel;
        await loginWindow.ShowDialog(_ownerWindow!);

        // Refresh authentication status after login window closes
        await CheckAuthenticationStatusAsync();
    }

    /// <summary>
    /// Sync restaurants from Firestore to local database after successful login
    /// Returns the sync result so caller can decide whether to show location selector
    /// Returns null if sync failed and user was signed out
    /// </summary>
    private async Task<SyncResult?> SyncRestaurantsAfterLoginWithResultAsync()
    {
        if (_syncService == null)
            return SyncResult.Success(); // No sync service, continue

        var syncResult = await _syncService.SyncRestaurantsAsync();

        if (!syncResult.IsSuccess)
        {
            // Sync failed - sign out and notify user
            await _sessionService.SignOutAsync();
            await CheckAuthenticationStatusAsync();
            _notificationService.ShowError($"Could not load your restaurants: {syncResult.ErrorMessage}. You have been signed out.");
            return null; // Indicate failure
        }

        // Check if user has no restaurants (will be indicated by 0 items downloaded and a message)
        if (syncResult.ItemsDownloaded == 0 && !string.IsNullOrEmpty(syncResult.ErrorMessage))
        {
            // User has no restaurants assigned - show info message and continue in offline mode
            _notificationService.ShowInfo($"Signed in successfully. {syncResult.ErrorMessage}");
        }

        return syncResult; // Return result so caller can check ItemsDownloaded
    }

    /// <summary>
    /// Show location selector and return the selected location
    /// </summary>
    private async Task<Location?> ShowLocationSelectorAsync()
    {
        var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
        if (locationService == null)
            return null;

        Location? selectedLocation = null;
        var locationWindow = new LocationSelectorWindow();
        var locationViewModel = new LocationSelectorViewModel(
            locationService,
            _sessionService,
            (location) =>
            {
                selectedLocation = location;
                locationWindow.SetSelectedLocation(location);
            },
            _syncService // Pass sync service to ensure locations are synced before loading
        );

        locationWindow.DataContext = locationViewModel;
        await locationViewModel.InitializeAsync();
        await locationWindow.ShowDialog(_ownerWindow!);

        if (selectedLocation == null)
        {
            // User cancelled location selection, sign them out
            await _sessionService.SignOutAsync();
            await CheckAuthenticationStatusAsync();
            _notificationService.ShowWarning("Sign in cancelled - you must select a location to continue");
        }

        return selectedLocation;
    }

    /// <summary>
    /// Set the current location and refresh the main window UI
    /// </summary>
    private async Task SetLocationAndRefreshUIAsync(Location selectedLocation)
    {
        // Store selected location for this session
        _currentLocationService.SetCurrentLocation(selectedLocation);

        // Request refresh of main window after location selection
        if (_ownerWindow is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel mainViewModel)
        {
            await mainViewModel.RefreshCurrentViewAsync();
        }
    }

    /// <summary>
    /// Perform initial sync and start auto-sync timer after successful login
    /// </summary>
    private async Task InitializeAutoSyncAfterLoginAsync()
    {
        if (_syncService == null)
            return;

        // Perform initial sync
        await PerformSyncAsync();

        // Refresh the main window to show synced data
        if (_ownerWindow is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel mainViewModel)
        {
            await mainViewModel.RefreshCurrentViewAsync();

            // Update location name in header
            var currentLoc = _currentLocationService.CurrentLocation;
            if (currentLoc != null)
            {
                mainViewModel.CurrentLocationName = currentLoc.Name;
            }

            _logger?.LogInformation("Refreshed UI after initial sync - showing synced data");
        }

        // Start auto-sync timer
        StartAutoSyncTimer();
    }

    [RelayCommand]
    private async Task LaunchTutorial()
    {
        if (_ownerWindow == null) return;

        try
        {
            var tutorialService = _serviceProvider.GetService(typeof(IExtendedTutorialService)) as IExtendedTutorialService;
            var progressTracker = _serviceProvider.GetService(typeof(ITutorialProgressTracker)) as ITutorialProgressTracker;

            if (tutorialService == null || progressTracker == null)
            {
                _notificationService.ShowError("Tutorial service is not available");
                return;
            }

            var tutorialWindow = new ExtendedTutorialWindow();
            var tutorialViewModel = new ExtendedTutorialViewModel(
                tutorialService,
                progressTracker,
                () => tutorialWindow.Close()
            );

            tutorialWindow.DataContext = tutorialViewModel;
            await tutorialWindow.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to launch tutorial: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ShowKeyboardShortcuts()
    {
        if (_ownerWindow == null) return;

        try
        {
            var shortcutsWindow = new KeyboardShortcutsHelpWindow();
            await shortcutsWindow.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to show keyboard shortcuts: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RedoOnboarding()
    {
        if (_ownerWindow == null) return;

        try
        {
            // Delete the onboarding completion file
            var onboardingPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Desktop Food Cost",
                "onboarding.json"
            );

            if (File.Exists(onboardingPath))
            {
                File.Delete(onboardingPath);
            }

            // Show onboarding wizard
            var locationRepository = _serviceProvider.GetService(typeof(ILocationRepository)) as ILocationRepository;
            if (locationRepository == null) return;

            var onboardingViewModel = new OnboardingViewModel(_localSettingsService, locationRepository);
            var onboardingWindow = new OnboardingWindow(onboardingViewModel);

            // Wait for completion
            var tcs = new TaskCompletionSource<bool>();
            onboardingViewModel.OnboardingCompleted += (s, e) =>
            {
                onboardingWindow.Close();
                tcs.TrySetResult(true); // Use TrySetResult to avoid exception if already completed
            };
            onboardingWindow.Closed += (s, e) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(false);
            };

            onboardingWindow.Show();
            await tcs.Task;

            // Update location name if changed
            if (_currentPreferences != null && !string.IsNullOrEmpty(_currentPreferences.RestaurantName))
            {
                var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
                if (locationService != null)
                {
                    var locations = await locationService.GetAllLocationsAsync();
                    var defaultLocation = locations.FirstOrDefault();

                    if (defaultLocation != null)
                    {
                        defaultLocation.Name = _currentPreferences.RestaurantName;
                        defaultLocation.ModifiedAt = DateTime.UtcNow;
                        await locationService.UpdateLocationAsync(defaultLocation);
                        _currentLocationService.SetCurrentLocation(defaultLocation);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Onboarding failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SignOut()
    {
        if (_ownerWindow == null) return;

        try
        {
            // Confirm sign out
            var confirmDialog = new Window
            {
                Title = "Confirm Sign Out",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 12 };
            panel.Children.Add(new TextBlock
            {
                Text = "Are you sure you want to sign out?",
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 14,
                Margin = new Avalonia.Thickness(0, 0, 0, 8)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "You will continue to have access to your local data, but cloud features will be disabled.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Avalonia.Thickness(0, 12, 0, 0)
            };

            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            cancelButton.Click += (s, e) => confirmDialog.Close(false);

            var signOutButton = new Button
            {
                Content = "Sign Out",
                Width = 80,
                Background = Avalonia.Media.Brushes.OrangeRed,
                Foreground = Avalonia.Media.Brushes.White
            };
            signOutButton.Click += (s, e) => confirmDialog.Close(true);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(signOutButton);
            panel.Children.Add(buttonPanel);
            confirmDialog.Content = panel;

            var confirmed = await confirmDialog.ShowDialog<bool>(_ownerWindow);
            if (!confirmed) return;

            // Perform final sync before signing out
            if (_syncService != null && _hasUnsyncedChanges)
            {
                await PerformSyncAsync();
            }

            // Stop auto-sync timer
            StopAutoSyncTimer();

            // Sign out
            await _sessionService.SignOutAsync();

            // Switch to offline location after sign-out
            System.Diagnostics.Debug.WriteLine("[SETTINGS] Switching to offline location after sign-out...");
            var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
            if (locationService != null)
            {
                var locations = await locationService.GetAllLocationsAsync();
                var offlineLocation = locations.FirstOrDefault(l => l.UserId == null);
                if (offlineLocation != null)
                {
                    _currentLocationService.SetCurrentLocation(offlineLocation);
                    System.Diagnostics.Debug.WriteLine($"[SETTINGS] ✓ Switched to offline location: {offlineLocation.Name}");
                    
                    // Request refresh of main window after location switch
                    if (_ownerWindow is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel mainViewModel)
                    {
                        await mainViewModel.RefreshCurrentViewAsync();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SETTINGS] ⚠️ Warning: No offline location found after sign-out");
                }
            }

            // Update UI
            IsSignedIn = false;
            UserEmail = null;
            EnableCloudSync = false;
            SyncStatus = "Not synced";
            LastSyncedText = "Never";
            _lastSyncTime = null;
            _hasUnsyncedChanges = false;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Sign out failed: {ex.Message}");
        }
    }

    private static async Task ShowMessageDialog(Window owner, string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
        panel.Children.Add(new TextBlock { Text = message, Margin = new Avalonia.Thickness(0, 0, 0, 20), TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        okButton.Click += (s, e) => dialog.Close();

        panel.Children.Add(okButton);
        dialog.Content = panel;
        await dialog.ShowDialog(owner);
    }
    // ============================================
    // CONVERSION MANAGEMENT COMMANDS
    // ============================================

    [RelayCommand]
    private async Task RefreshConversionStats()
    {
        if (_conversionRepository == null) return;

        try
        {
            var stats = await _conversionRepository.GetStatisticsAsync();
            TotalConversions = stats.Total;
            UsdaConversions = stats.USDA;
            UserDefinedConversions = stats.UserDefined;

            // Calculate coverage percentage
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
            if (ingredients.Count > 0)
            {
                var ingredientsWithConversions = stats.IngredientSpecific;
                ConversionCoveragePercent = (decimal)ingredientsWithConversions / ingredients.Count * 100;
            }
            else
            {
                ConversionCoveragePercent = 0;
            }

            _notificationService.ShowSuccess("Conversion statistics refreshed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh conversion stats");
            _notificationService.ShowError($"Failed to refresh statistics: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewConversions()
    {
        _notificationService.ShowInfo("Conversion manager coming soon! View conversions in the Ingredients tab for now.");
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await CheckAuthenticationStatusAsync();
        await RefreshConversionStats();
    }

    // Photo Cache Management
    private async Task LoadCacheSizeAsync()
    {
        if (_photoService == null)
        {
            CacheSize = "Not available";
            return;
        }

        try
        {
            var sizeBytes = _photoService.GetCacheSize();
            CacheSize = FormatBytes(sizeBytes);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load cache size");
            CacheSize = "Error";
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        if (_photoService == null)
        {
            _notificationService.ShowWarning("Photo service not available");
            return;
        }

        IsCacheClearing = true;
        try
        {
            _photoService.ClearLocalCache();
            await LoadCacheSizeAsync();
            _notificationService.ShowSuccess("Photo cache cleared successfully!");
            _logger?.LogInformation("User cleared photo cache");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear cache");
            _notificationService.ShowError($"Failed to clear cache: {ex.Message}");
        }
        finally
        {
            IsCacheClearing = false;
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    // ============================================
    // LOCATION MANAGEMENT COMMANDS
    // ============================================

    [RelayCommand]
    private async Task LoadLocations()
    {
        try
        {
            var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
            if (locationService == null) return;

            var allLocations = await locationService.GetAllLocationsAsync();
            Locations.Clear();
            foreach (var loc in allLocations.OrderBy(l => l.Name))
            {
                Locations.Add(loc);
            }

            // Update current location name
            var currentLoc = _currentLocationService.CurrentLocation;
            CurrentLocationName = currentLoc?.Name ?? "No location selected";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load locations");
        }
    }

    [RelayCommand]
    private async Task AddLocation()
    {
        if (_ownerWindow == null) return;

        try
        {
            var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
            if (locationService == null) return;

            var window = new AddEditLocationWindow();
            var viewModel = new AddEditLocationViewModel(
                locationService,
                () => window.Close(),
                null // New location
            );

            viewModel.LocationSaved += async (s, location) =>
            {
                await LoadLocations();
                _notificationService.ShowSuccess($"Location '{location.Name}' created successfully");
            };

            window.DataContext = viewModel;
            await window.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add location");
            _notificationService.ShowError($"Failed to add location: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditLocation(Location? location)
    {
        if (_ownerWindow == null || location == null) return;

        try
        {
            var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
            if (locationService == null) return;

            var window = new AddEditLocationWindow();
            var viewModel = new AddEditLocationViewModel(
                locationService,
                () => window.Close(),
                location
            );

            viewModel.LocationSaved += async (s, savedLocation) =>
            {
                await LoadLocations();
                _notificationService.ShowSuccess($"Location '{savedLocation.Name}' updated successfully");

                // If this was the current location, update the display
                if (_currentLocationService.CurrentLocationId == savedLocation.Id)
                {
                    _currentLocationService.SetCurrentLocation(savedLocation);
                    CurrentLocationName = savedLocation.Name;
                }
            };

            window.DataContext = viewModel;
            await window.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to edit location");
            _notificationService.ShowError($"Failed to edit location: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteLocation(Location? location)
    {
        if (_ownerWindow == null || location == null) return;

        // Don't allow deleting the current location
        if (_currentLocationService.CurrentLocationId == location.Id)
        {
            _notificationService.ShowWarning("Cannot delete the currently active location. Switch to a different location first.");
            return;
        }

        // Don't allow deleting if it's the only location
        if (Locations.Count <= 1)
        {
            _notificationService.ShowWarning("Cannot delete the only location. Create another location first.");
            return;
        }

        try
        {
            // Confirm deletion
            var confirmDialog = new Window
            {
                Title = "Confirm Delete",
                Width = 450,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text = $"Are you sure you want to delete '{location.Name}'?\n\n" +
                       "This will permanently delete all ingredients, recipes, and entrees associated with this location.",
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontWeight = Avalonia.Media.FontWeight.Bold
            });

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            cancelButton.Click += (s, e) => confirmDialog.Close(false);

            var deleteButton = new Button
            {
                Content = "Delete",
                Width = 80,
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White
            };
            deleteButton.Click += (s, e) => confirmDialog.Close(true);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(deleteButton);
            panel.Children.Add(buttonPanel);
            confirmDialog.Content = panel;

            var confirmed = await confirmDialog.ShowDialog<bool>(_ownerWindow);
            if (!confirmed) return;

            var locationService = _serviceProvider.GetService(typeof(ILocationService)) as ILocationService;
            if (locationService == null) return;

            await locationService.DeleteLocationAsync(location.Id);
            await LoadLocations();
            _notificationService.ShowSuccess($"Location '{location.Name}' deleted");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete location");
            _notificationService.ShowError($"Failed to delete location: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SwitchToLocation(Location? location)
    {
        if (location == null) return;

        try
        {
            _currentLocationService.SetCurrentLocation(location);
            CurrentLocationName = location.Name;

            // Refresh the main window to show data for the new location
            if (_ownerWindow is MainWindow mainWindow && mainWindow.DataContext is MainWindowViewModel mainViewModel)
            {
                mainViewModel.CurrentLocationName = location.Name;
                await mainViewModel.RefreshCurrentViewAsync();
            }

            _notificationService.ShowSuccess($"Switched to '{location.Name}'");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to switch location");
            _notificationService.ShowError($"Failed to switch location: {ex.Message}");
        }
    }

    // ============================================
    // IMPORT MAPPINGS MANAGEMENT
    // ============================================

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Core.Models.ImportMap> _savedImportMappings = new();

    [RelayCommand]
    private async Task LoadImportMappings()
    {
        try
        {
            var importMapRepository = _serviceProvider.GetService(typeof(IImportMapRepository)) as IImportMapRepository;
            if (importMapRepository == null) return;

            var maps = await importMapRepository.GetUserMapsAsync(_currentLocationService.CurrentLocationId);
            SavedImportMappings.Clear();
            foreach (var map in maps.OrderByDescending(m => m.LastUsedAt ?? m.CreatedAt))
            {
                SavedImportMappings.Add(map);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load import mappings");
        }
    }

    [RelayCommand]
    private async Task EditImportMapping(Core.Models.ImportMap? mapping)
    {
        if (_ownerWindow == null || mapping == null) return;

        try
        {
            var importMapService = _serviceProvider.GetService(typeof(IImportMapService)) as IImportMapService;
            var importMapRepository = _serviceProvider.GetService(typeof(IImportMapRepository)) as IImportMapRepository;
            var importBatchRepository = _serviceProvider.GetService(typeof(IImportBatchRepository)) as IImportBatchRepository;
            var ingredientRepository = _serviceProvider.GetService(typeof(IIngredientRepository)) as IIngredientRepository;

            if (importMapService == null || importMapRepository == null ||
                importBatchRepository == null || ingredientRepository == null)
            {
                _notificationService.ShowWarning("Import service not available");
                return;
            }

            // Open the import mapper window with the mapping pre-loaded
            var window = new ImportMapperWindow();
            var viewModel = new ImportMapperViewModel(
                importMapService,
                importMapRepository,
                importBatchRepository,
                ingredientRepository,
                _ingredientService,
                _currentLocationService,
                _notificationService,
                () => window.Close()
            );

            // Pre-load the mapping
            viewModel.CurrentMapping = mapping;
            viewModel.ShouldSaveMapping = true;
            viewModel.SaveMappingName = mapping.DisplayName;

            window.DataContext = viewModel;
            await window.ShowDialog(_ownerWindow);

            // Refresh the list after editing
            await LoadImportMappings();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to edit import mapping");
            _notificationService.ShowError($"Failed to edit mapping: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteImportMapping(Core.Models.ImportMap? mapping)
    {
        if (_ownerWindow == null || mapping == null) return;

        try
        {
            // Confirm deletion
            var confirmDialog = new Window
            {
                Title = "Confirm Delete",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text = $"Are you sure you want to delete the mapping '{mapping.DisplayName}'?\n\n" +
                       "This will not affect any ingredients that were imported using this mapping.",
                Margin = new Avalonia.Thickness(0, 0, 0, 20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontWeight = Avalonia.Media.FontWeight.Bold
            });

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            cancelButton.Click += (s, e) => confirmDialog.Close(false);

            var deleteButton = new Button
            {
                Content = "Delete",
                Width = 80,
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White
            };
            deleteButton.Click += (s, e) => confirmDialog.Close(true);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(deleteButton);
            panel.Children.Add(buttonPanel);
            confirmDialog.Content = panel;

            var confirmed = await confirmDialog.ShowDialog<bool>(_ownerWindow);
            if (!confirmed) return;

            var importMapRepository = _serviceProvider.GetService(typeof(IImportMapRepository)) as IImportMapRepository;
            if (importMapRepository == null) return;

            await importMapRepository.DeleteAsync(mapping.Id);
            await LoadImportMappings();
            _notificationService.ShowSuccess($"Mapping '{mapping.DisplayName}' deleted");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete import mapping");
            _notificationService.ShowError($"Failed to delete mapping: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenImportMapper()
    {
        if (_ownerWindow == null) return;

        try
        {
            var importMapService = _serviceProvider.GetService(typeof(IImportMapService)) as IImportMapService;
            var importMapRepository = _serviceProvider.GetService(typeof(IImportMapRepository)) as IImportMapRepository;
            var importBatchRepository = _serviceProvider.GetService(typeof(IImportBatchRepository)) as IImportBatchRepository;
            var ingredientRepository = _serviceProvider.GetService(typeof(IIngredientRepository)) as IIngredientRepository;

            if (importMapService == null || importMapRepository == null ||
                importBatchRepository == null || ingredientRepository == null)
            {
                _notificationService.ShowWarning("Import service not available. Please restart the application.");
                return;
            }

            var window = new ImportMapperWindow();
            var viewModel = new ImportMapperViewModel(
                importMapService,
                importMapRepository,
                importBatchRepository,
                ingredientRepository,
                _ingredientService,
                _currentLocationService,
                _notificationService,
                () => window.Close()
            );

            window.DataContext = viewModel;
            await window.ShowDialog(_ownerWindow);

            // Refresh the list after importing
            await LoadImportMappings();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open import mapper");
            _notificationService.ShowError($"Failed to open import mapper: {ex.Message}");
        }
    }

}
