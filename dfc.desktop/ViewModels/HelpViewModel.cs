using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Desktop.Services;
using Dfc.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
    private readonly IExtendedTutorialService _tutorialService;
    private readonly ITutorialProgressTracker _progressTracker;

    // Module completion status observables
    [ObservableProperty]
    private string _gettingStartedStatus = "🔴"; // Red = not started

    [ObservableProperty]
    private string _ingredientsStatus = "🔴";

    [ObservableProperty]
    private string _recipesStatus = "🔴";

    [ObservableProperty]
    private string _entreesStatus = "🔴";

    [ObservableProperty]
    private string _dashboardStatus = "🔴";

    [ObservableProperty]
    private string _advancedStatus = "🔴";

    public HelpViewModel(IExtendedTutorialService tutorialService, ITutorialProgressTracker progressTracker)
    {
        _tutorialService = tutorialService;
        _progressTracker = progressTracker;

        // Load module statuses
        _ = LoadModuleStatusesAsync();
    }

    private async Task LoadModuleStatusesAsync()
    {
        try
        {
            var moduleIds = new Dictionary<string, Action<string>>
            {
                { "getting-started", status => GettingStartedStatus = status },
                { "ingredients", status => IngredientsStatus = status },
                { "recipes", status => RecipesStatus = status },
                { "entrees", status => EntreesStatus = status },
                { "dashboard", status => DashboardStatus = status },
                { "advanced", status => AdvancedStatus = status }
            };

            foreach (var (moduleId, setStatus) in moduleIds)
            {
                var module = _tutorialService.GetModuleById(moduleId);
                if (module == null) continue;

                int completedSteps = 0;
                foreach (var step in module.Steps)
                {
                    if (await _progressTracker.HasCompletedStepAsync(moduleId, step.Id))
                    {
                        completedSteps++;
                    }
                }

                if (completedSteps == 0)
                {
                    setStatus("🔴"); // Red - not started
                }
                else if (completedSteps < module.Steps.Count)
                {
                    setStatus("🟡"); // Yellow - in progress
                }
                else
                {
                    setStatus("🟢"); // Green - completed
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading module statuses: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task StartTutorial(string? moduleId = null)
    {
        try
        {
            // Get main window
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot launch tutorial: Main window not found");
                return;
            }

            // Create the tutorial window first
            var tutorialWindow = new ExtendedTutorialWindow();

            // Create the tutorial window with ViewModel - pass window close action
            ExtendedTutorialViewModel tutorialViewModel;
            if (string.IsNullOrEmpty(moduleId))
            {
                // Start from beginning
                tutorialViewModel = new ExtendedTutorialViewModel(
                    _tutorialService,
                    _progressTracker,
                    () => tutorialWindow.Close()
                );
            }
            else
            {
                // Start from specific module
                tutorialViewModel = new ExtendedTutorialViewModel(
                    _tutorialService,
                    _progressTracker,
                    () => tutorialWindow.Close(),
                    moduleId
                );
            }

            tutorialWindow.DataContext = tutorialViewModel;

            // Show as dialog
            await tutorialWindow.ShowDialog(mainWindow);

            // Reload module statuses after tutorial closes
            await LoadModuleStatusesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching tutorial: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ShowKeyboardShortcuts()
    {
        try
        {
            var window = new KeyboardShortcutsHelpWindow();
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                await window.ShowDialog(mainWindow);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing keyboard shortcuts: {ex.Message}");
        }
    }
}
