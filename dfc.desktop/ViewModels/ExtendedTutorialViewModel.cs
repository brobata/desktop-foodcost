using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Desktop.Services;
using TutorialModule = Dfc.Desktop.Models.TutorialModule;
using TutorialStep = Dfc.Desktop.Models.TutorialStep;

namespace Dfc.Desktop.ViewModels;

public partial class ExtendedTutorialViewModel : ViewModelBase
{
    private readonly IExtendedTutorialService _tutorialService;
    private readonly ITutorialProgressTracker _progressTracker;
    private readonly Action _onComplete;
    private readonly bool _singleModuleMode;
    private readonly string? _startModuleId;

    [ObservableProperty]
    private TutorialModule _currentModule;

    [ObservableProperty]
    private TutorialStep _currentStep;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string _nextButtonText = "Next →";

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private string? _screenshotPath;

    [ObservableProperty]
    private bool _showConfetti;

    [ObservableProperty]
    private bool _isGrandConfetti;

    public bool HasScreenshot => !string.IsNullOrEmpty(ScreenshotPath);
    public bool HasKeyboardShortcut => !string.IsNullOrEmpty(CurrentStep?.KeyboardShortcut);
    public bool HasProTip => !string.IsNullOrEmpty(CurrentStep?.ProTip);
    public bool HasAnnotations => CurrentStep?.Annotations?.Any() == true;

    public ExtendedTutorialViewModel()
    {
        // Design-time constructor
        _tutorialService = null!;
        _progressTracker = null!;
        _onComplete = () => { };
        _singleModuleMode = false;
        _startModuleId = null;
        _currentModule = new TutorialModule { Name = "Getting Started", Icon = "🚀" };
        _currentStep = new TutorialStep { Title = "Welcome", Description = "Welcome to Desktop Food Cost!" };
    }

    public ExtendedTutorialViewModel(
        IExtendedTutorialService tutorialService,
        ITutorialProgressTracker progressTracker,
        Action onComplete)
    {
        _tutorialService = tutorialService;
        _progressTracker = progressTracker;
        _onComplete = onComplete;
        _singleModuleMode = false;
        _startModuleId = null;

        var modules = _tutorialService.GetAllModules();
        _currentModule = modules.First();
        _currentStep = _currentModule.Steps.First();

        LoadScreenshot();
        UpdateProgress();
        UpdateNavigationState();
    }

    public ExtendedTutorialViewModel(
        IExtendedTutorialService tutorialService,
        ITutorialProgressTracker progressTracker,
        Action onComplete,
        string startModuleId)
    {
        _tutorialService = tutorialService;
        _progressTracker = progressTracker;
        _onComplete = onComplete;
        _singleModuleMode = true; // Single module mode
        _startModuleId = startModuleId;

        // Get the specified module, or fall back to first module if not found
        var module = _tutorialService.GetModuleById(startModuleId);
        if (module == null)
        {
            var modules = _tutorialService.GetAllModules();
            _currentModule = modules.First();
            _singleModuleMode = false;
        }
        else
        {
            _currentModule = module;
        }

        _currentStep = _currentModule.Steps.First();

        LoadScreenshot();
        UpdateProgress();
        UpdateNavigationState();
    }

    [RelayCommand]
    private async Task Next()
    {
        // Mark current step as completed
        await _progressTracker.MarkStepCompletedAsync(CurrentModule.Id, CurrentStep.Id);

        // In single module mode, check if we're at the end of the current module
        if (_singleModuleMode)
        {
            var currentStepIndex = CurrentModule.Steps.FindIndex(s => s.Id == CurrentStep.Id);

            // Check if this is the last step of the current module
            if (currentStepIndex >= CurrentModule.Steps.Count - 1)
            {
                // Check if ALL 6 modules are now complete
                bool allModulesComplete = await CheckAllModulesCompleteAsync();

                if (allModulesComplete)
                {
                    // ALL MODULES COMPLETE - GRAND CONFETTI!!!
                    IsGrandConfetti = true;
                    ShowConfetti = true;

                    // Wait a moment for confetti to show
                    await Task.Delay(300);

                    var progress = await _progressTracker.LoadProgressAsync();
                    progress.HasCompletedFullTutorial = true;
                    progress.CompletedDate = DateTime.UtcNow;
                    await _progressTracker.SaveProgressAsync(progress);

                    // Wait for grand confetti to finish (3 seconds total - intense!)
                    await Task.Delay(2700);
                }
                else
                {
                    // Single module complete - no confetti, just close
                    IsGrandConfetti = false;
                }

                _onComplete();
                return;
            }

            // Move to next step within the same module
            CurrentStep = CurrentModule.Steps[currentStepIndex + 1];

            LoadScreenshot();
            UpdateProgress();
            UpdateNavigationState();
            OnPropertyChanged(nameof(HasScreenshot));
            OnPropertyChanged(nameof(HasKeyboardShortcut));
            OnPropertyChanged(nameof(HasProTip));
            OnPropertyChanged(nameof(HasAnnotations));
            return;
        }

        // Multi-module mode: navigate across all modules
        var (nextModule, nextStep) = _tutorialService.GetNextStep(CurrentModule.Id, CurrentStep.Id);

        if (nextModule == null || nextStep == null)
        {
            // Reached the end of tutorial - trigger confetti!
            ShowConfetti = true;

            // Wait a moment for confetti to show
            await Task.Delay(300);

            var progress = await _progressTracker.LoadProgressAsync();
            progress.HasCompletedFullTutorial = true;
            progress.CompletedDate = DateTime.UtcNow;
            await _progressTracker.SaveProgressAsync(progress);

            // Wait for confetti to finish (3 seconds total)
            await Task.Delay(2700);

            _onComplete();
            return;
        }

        CurrentModule = nextModule;
        CurrentStep = nextStep;

        LoadScreenshot();
        UpdateProgress();
        UpdateNavigationState();
        OnPropertyChanged(nameof(HasScreenshot));
        OnPropertyChanged(nameof(HasKeyboardShortcut));
        OnPropertyChanged(nameof(HasProTip));
        OnPropertyChanged(nameof(HasAnnotations));
    }

    [RelayCommand]
    private void Back()
    {
        // In single module mode, only navigate within the current module
        if (_singleModuleMode)
        {
            var currentStepIndex = CurrentModule.Steps.FindIndex(s => s.Id == CurrentStep.Id);

            // Can't go back if we're on the first step
            if (currentStepIndex <= 0)
                return;

            CurrentStep = CurrentModule.Steps[currentStepIndex - 1];

            LoadScreenshot();
            UpdateProgress();
            UpdateNavigationState();
            OnPropertyChanged(nameof(HasScreenshot));
            OnPropertyChanged(nameof(HasKeyboardShortcut));
            OnPropertyChanged(nameof(HasProTip));
            OnPropertyChanged(nameof(HasAnnotations));
            return;
        }

        // Multi-module mode: navigate across all modules
        var (prevModule, prevStep) = _tutorialService.GetPreviousStep(CurrentModule.Id, CurrentStep.Id);

        if (prevModule == null || prevStep == null)
            return;

        CurrentModule = prevModule;
        CurrentStep = prevStep;

        LoadScreenshot();
        UpdateProgress();
        UpdateNavigationState();
        OnPropertyChanged(nameof(HasScreenshot));
        OnPropertyChanged(nameof(HasKeyboardShortcut));
        OnPropertyChanged(nameof(HasProTip));
        OnPropertyChanged(nameof(HasAnnotations));
    }

    [RelayCommand]
    private async Task Skip()
    {
        // Save progress before exiting - user can resume later
        await SaveProgressAndExit();
    }

    [RelayCommand]
    private async Task Close()
    {
        // Save progress when closing with X button
        await SaveProgressAndExit();
    }

    private async Task SaveProgressAndExit()
    {
        try
        {
            // Save current position so user can resume later
            var progress = await _progressTracker.LoadProgressAsync();
            progress.LastModuleId = CurrentModule.Id;
            progress.LastStepId = CurrentStep.Id;
            await _progressTracker.SaveProgressAsync(progress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving tutorial progress: {ex.Message}");
            // Still exit even if save fails
        }

        _onComplete();
    }

    private void LoadScreenshot()
    {
        if (string.IsNullOrEmpty(CurrentStep.ScreenshotPath))
        {
            ScreenshotPath = null;
            return;
        }

        try
        {
            var assetPath = $"avares://Dfc.Desktop/Assets/Tutorial/Screenshots/{CurrentStep.ScreenshotPath}";

            // Check if asset exists
            var assets = AssetLoader.GetAssets(new Uri(assetPath), null);
            if (assets != null && assets.Any())
            {
                ScreenshotPath = assetPath;
            }
            else
            {
                // Asset doesn't exist yet, set to null
                ScreenshotPath = null;
            }
        }
        catch
        {
            // If there's any error loading the asset, just don't show it
            ScreenshotPath = null;
        }
    }

    private void UpdateProgress()
    {
        var currentStepIndex = CurrentModule.Steps.FindIndex(s => s.Id == CurrentStep.Id);
        int totalSteps;
        int currentStepNumber;

        // In single module mode, show progress within the current module only
        if (_singleModuleMode)
        {
            totalSteps = CurrentModule.Steps.Count;
            currentStepNumber = currentStepIndex + 1;
        }
        else
        {
            // Multi-module mode: calculate overall progress across all modules
            var modules = _tutorialService.GetAllModules();
            var currentModuleIndex = modules.FindIndex(m => m.Id == CurrentModule.Id);

            totalSteps = _tutorialService.GetTotalStepCount();
            var stepsBeforeCurrentModule = modules.Take(currentModuleIndex).Sum(m => m.Steps.Count);
            currentStepNumber = stepsBeforeCurrentModule + currentStepIndex + 1;
        }

        ProgressText = $"Step {currentStepNumber} of {totalSteps}";
        ProgressPercentage = (double)currentStepNumber / totalSteps * 100;

        // Update next button text
        if (currentStepNumber == totalSteps)
        {
            NextButtonText = "Finish 🎉";
        }
        else
        {
            NextButtonText = "Next →";
        }
    }

    private void UpdateNavigationState()
    {
        var currentStepIndex = CurrentModule.Steps.FindIndex(s => s.Id == CurrentStep.Id);

        // In single module mode, can only go back if not on first step of current module
        if (_singleModuleMode)
        {
            CanGoBack = currentStepIndex > 0;
            return;
        }

        // Multi-module mode: can go back if not on the first step of the first module
        var modules = _tutorialService.GetAllModules();
        var currentModuleIndex = modules.FindIndex(m => m.Id == CurrentModule.Id);
        CanGoBack = currentModuleIndex > 0 || currentStepIndex > 0;
    }

    private async Task<bool> CheckAllModulesCompleteAsync()
    {
        try
        {
            var allModules = _tutorialService.GetAllModules();

            foreach (var module in allModules)
            {
                foreach (var step in module.Steps)
                {
                    if (!await _progressTracker.HasCompletedStepAsync(module.Id, step.Id))
                    {
                        return false; // Found an incomplete step
                    }
                }
            }

            return true; // All steps in all modules are complete!
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking module completion: {ex.Message}");
            return false;
        }
    }
}
