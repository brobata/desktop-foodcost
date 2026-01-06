using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Services;
using Dfc.Desktop.Services;

namespace Dfc.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserSessionService _sessionService;
    private readonly ILocalSettingsService _settingsService;
    private readonly Action<bool> _onLoginComplete;
    private readonly Window? _window;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = true;

    [ObservableProperty]
    private bool _rememberPassword = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isOfflineMode = false;

    public string Title => "Sign In";
    public string SubmitButtonText => "Sign In";

    public LoginViewModel(
        IUserSessionService sessionService,
        ILocalSettingsService settingsService,
        Action<bool> onLoginComplete,
        Window? window = null)
    {
        _sessionService = sessionService;
        _settingsService = settingsService;
        _onLoginComplete = onLoginComplete;
        _window = window;

        // Load remembered email
        _ = LoadRememberedEmailAsync();
    }

    private async Task LoadRememberedEmailAsync()
    {
        try
        {
            var rememberedEmail = await _settingsService.GetRememberedEmailAsync();
            if (!string.IsNullOrEmpty(rememberedEmail))
            {
                Email = rememberedEmail;
                RememberMe = true;
            }

            var rememberedPassword = await _settingsService.GetRememberedPasswordAsync();
            if (!string.IsNullOrEmpty(rememberedPassword))
            {
                Password = rememberedPassword;
                RememberPassword = true;
            }
        }
        catch
        {
            // Silently fail
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        ErrorMessage = null;

        // Validate inputs
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email address";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your password";
            return;
        }

        IsLoading = true;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[LOGIN] User clicked Sign In. Email: {Email}, RememberMe: {RememberMe}, RememberPassword: {RememberPassword}");
            
            var result = await _sessionService.SignInAsync(Email.Trim(), Password, RememberMe);

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Sign-in successful");
                
                // Save email if Remember Me is checked
                if (RememberMe)
                {
                    System.Diagnostics.Debug.WriteLine("[LOGIN] Saving remembered email");
                    await _settingsService.SaveRememberedEmailAsync(Email.Trim());
                }
                else
                {
                    await _settingsService.SaveRememberedEmailAsync(null);
                }

                // Save password if Remember Password is checked
                if (RememberPassword)
                {
                    System.Diagnostics.Debug.WriteLine("[LOGIN] Saving remembered password");
                    await _settingsService.SaveRememberedPasswordAsync(Password);
                }
                else
                {
                    await _settingsService.SaveRememberedPasswordAsync(null);
                }

                System.Diagnostics.Debug.WriteLine("[LOGIN] ✓ Sign-in completed successfully, closing login window");
                
                // Login successful
                _onLoginComplete(true);
                _window?.Close();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Authentication failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            // Provide detailed error information for debugging
            var errorDetails = $"Sign-in error: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorDetails += $"\n\nAdditional details: {ex.InnerException.Message}";
            }

            // Log the full exception stack trace for debugging
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Sign-in exception: {ex}");

            // Write to log file for troubleshooting
            var logFile = LogLoginError(ex, errorDetails);

            // Show error to user with log location
            errorDetails += $"\n\nError logged to:\n{logFile}";
            ErrorMessage = errorDetails;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string LogLoginError(Exception ex, string errorDetails)
    {
        try
        {
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "OneDrive",
                "Desktop",
                "Logs"
            );
            System.IO.Directory.CreateDirectory(logPath);

            var logFile = System.IO.Path.Combine(logPath, $"login_errors_{DateTime.Now:yyyyMMdd}.txt");
            var logMessage = $@"
=== LOGIN ERROR ===
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Email: {Email}
Error: {errorDetails}

Exception Type: {ex.GetType().FullName}
Exception Message: {ex.Message}
Stack Trace:
{ex.StackTrace}
";

            if (ex.InnerException != null)
            {
                logMessage += $@"
Inner Exception Type: {ex.InnerException.GetType().FullName}
Inner Exception Message: {ex.InnerException.Message}
Inner Stack Trace:
{ex.InnerException.StackTrace}
";
            }

            logMessage += "\n=====================================\n";

            System.IO.File.AppendAllText(logFile, logMessage);
            return logFile;
        }
        catch
        {
            // If logging fails, return a generic path
            return "%USERPROFILE%\\OneDrive\\Desktop\\Logs\\login_errors.txt";
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email address to reset your password";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _sessionService.SendPasswordResetEmailAsync(Email.Trim());
            ErrorMessage = "Password reset email sent! Check your inbox.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to send reset email: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

}
