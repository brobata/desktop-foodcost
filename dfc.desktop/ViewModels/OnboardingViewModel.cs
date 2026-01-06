using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly ILocalSettingsService _settingsService;
    private readonly ILocationRepository _locationRepository;

    [ObservableProperty]
    private int _currentStep = 0;

    [ObservableProperty]
    private string _firstName = "";

    [ObservableProperty]
    private string _lastName = "";

    [ObservableProperty]
    private string _restaurantName = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _emailError = "";

    [ObservableProperty]
    private bool _canGoNext = true;

    [ObservableProperty]
    private bool _canGoPrevious = false;

    [ObservableProperty]
    private bool _isLastStep = false;

    [ObservableProperty]
    private string _nextButtonText = "Next";

    [ObservableProperty]
    private bool _wantsTutorial = true; // Default to true - most users benefit from tutorial

    public int TotalSteps => 4; // Welcome, User Info, Ready, Tutorial Choice

    public bool TutorialOptedIn { get; private set; }

    public OnboardingViewModel(ILocalSettingsService settingsService, ILocationRepository locationRepository)
    {
        _settingsService = settingsService;
        _locationRepository = locationRepository;

        // Load existing settings to pre-populate fields
        _ = LoadExistingSettingsAsync();

        UpdateNavigationState();
    }

    private async Task LoadExistingSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();

            // Pre-populate fields if they exist
            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                // Try to split the full name into first and last name
                var nameParts = settings.UserName.Trim().Split(' ', 2);
                if (nameParts.Length == 2)
                {
                    FirstName = nameParts[0];
                    LastName = nameParts[1];
                }
                else
                {
                    // If no space, put entire name in FirstName
                    FirstName = settings.UserName;
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.RestaurantName))
            {
                RestaurantName = settings.RestaurantName;
            }

            if (!string.IsNullOrWhiteSpace(settings.Email))
            {
                Email = settings.Email;
            }

            UpdateNavigationState();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading existing settings: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Next()
    {
        if (CurrentStep < TotalSteps - 1)
        {
            CurrentStep++;
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private async Task Finish()
    {
        await SaveUserInfoAsync();
        await SendUserInfoToServerAsync(); // Optional: send to your server for email list

        // Save tutorial preference
        TutorialOptedIn = WantsTutorial;

        // Mark onboarding as complete
        await MarkOnboardingCompleteAsync();

        // Notify that onboarding is complete (caller will close window)
        OnboardingCompleted?.Invoke(this, EventArgs.Empty);
    }


    private void UpdateNavigationState()
    {
        CanGoPrevious = CurrentStep > 0;
        IsLastStep = CurrentStep == TotalSteps - 1;
        NextButtonText = IsLastStep ? "Let's Go!" : "Next";
        CanGoNext = ValidateCurrentStep();
    }

    private bool ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            1 => !string.IsNullOrWhiteSpace(FirstName) &&
                 !string.IsNullOrWhiteSpace(LastName) &&
                 !string.IsNullOrWhiteSpace(RestaurantName) &&
                 !string.IsNullOrWhiteSpace(Email) &&
                 IsValidEmail(Email), // All fields required + valid email
            _ => true
        };
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Basic email validation: must have @ and at least one dot after @
            var trimmedEmail = email.Trim();
            if (!trimmedEmail.Contains('@'))
                return false;

            var parts = trimmedEmail.Split('@');
            if (parts.Length != 2)
                return false;

            var domain = parts[1];
            if (!domain.Contains('.'))
                return false;

            // Very basic check for x@x.x format
            return trimmedEmail.Length >= 5; // minimum: a@b.c
        }
        catch
        {
            return false;
        }
    }

    partial void OnFirstNameChanged(string value)
    {
        UpdateNavigationState();
    }

    partial void OnLastNameChanged(string value)
    {
        UpdateNavigationState();
    }

    partial void OnRestaurantNameChanged(string value)
    {
        UpdateNavigationState();
    }

    partial void OnEmailChanged(string value)
    {
        // Validate email format and show error message
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (!IsValidEmail(value))
            {
                EmailError = "Please enter a valid email address (e.g., chef@restaurant.com)";
            }
            else
            {
                EmailError = "";
            }
        }
        else
        {
            EmailError = "";
        }

        UpdateNavigationState();
    }

    private async Task SaveUserInfoAsync()
    {
        try
        {
            var settings = await _settingsService.LoadSettingsAsync();

            // Save user info to local settings
            var fullName = $"{FirstName} {LastName}".Trim();
            settings.UserName = fullName;
            settings.RestaurantName = RestaurantName;
            settings.Email = Email;
            settings.AgreeToNewsletter = true; // Always opt-in

            await _settingsService.SaveSettingsAsync(settings);

            System.Diagnostics.Debug.WriteLine($"Saved user info: {fullName} from {RestaurantName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving user info: {ex.Message}");
        }
    }

    private async Task SendUserInfoToServerAsync()
    {
        await Task.CompletedTask;
        // Firebase removed - onboarding data not saved to cloud
        System.Diagnostics.Debug.WriteLine("Onboarding data not saved to cloud (Firebase removed)");
    }

    private async Task MarkOnboardingCompleteAsync()
    {
        try
        {
            var settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Desktop Food Cost",
                "onboarding.json"
            );

            var onboardingData = new { Completed = true, CompletedAt = DateTime.UtcNow };
            var json = System.Text.Json.JsonSerializer.Serialize(onboardingData);
            await System.IO.File.WriteAllTextAsync(settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error marking onboarding complete: {ex.Message}");
        }
    }

    public event EventHandler? OnboardingCompleted;
}
