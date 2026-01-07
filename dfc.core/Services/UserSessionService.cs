using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dfc.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.Core.Services;

/// <summary>
/// Manages user session state for local-only mode
/// </summary>
public class UserSessionService : IUserSessionService
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<UserSessionService>? _logger;
    private readonly string _sessionFilePath;
    private User? _currentUser;
    private bool _isInitialized;

    public event EventHandler? AuthenticationStateChanged;

    public UserSessionService(
        IAuthenticationService authService,
        ILogger<UserSessionService>? logger = null)
    {
        _authService = authService;
        _logger = logger;

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );
        Directory.CreateDirectory(appDataPath);
        _sessionFilePath = Path.Combine(appDataPath, "session.json");
    }

    public bool IsAuthenticated => _currentUser != null && _authService.IsAuthenticated;

    public User? CurrentUser => _currentUser;

    private void OnAuthenticationStateChanged()
    {
        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Initialize session - for local-only mode this just sets up the state
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        _logger?.LogInformation("[AUTH] Session initialization started (local-only mode)");

        if (_isInitialized)
        {
            _logger?.LogInformation("[AUTH] Already initialized. IsAuthenticated: {IsAuthenticated}", IsAuthenticated);
            return IsAuthenticated;
        }

        _isInitialized = true;
        await Task.CompletedTask;

        _logger?.LogInformation("[AUTH] Session initialization completed (local-only mode). Authenticated: {IsAuthenticated}", IsAuthenticated);
        return false;
    }

    /// <summary>
    /// Sign in with email and password (local-only mode)
    /// </summary>
    public async Task<SessionResult> SignInAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            var user = await _authService.SignInAsync(email, password);
            _currentUser = user;

            _logger?.LogInformation("User {Email} signed in successfully (local mode)", email);
            OnAuthenticationStateChanged();
            return SessionResult.Success(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogWarning("Sign in failed for {Email}: {Error}", email, ex.Message);
            return SessionResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during sign in for {Email}", email);
            return SessionResult.Failure($"Sign-in error: {ex.Message}");
        }
    }

    /// <summary>
    /// Sign up a new user (local-only mode)
    /// </summary>
    public async Task<SessionResult> SignUpAsync(string email, string password, string displayName, bool rememberMe = false)
    {
        try
        {
            var user = await _authService.SignUpAsync(email, password, displayName);
            _currentUser = user;

            _logger?.LogInformation("User {Email} signed up successfully (local mode)", email);
            OnAuthenticationStateChanged();
            return SessionResult.Success(user);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Sign up failed for {Email}", email);
            return SessionResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Sign out the current user
    /// </summary>
    public async Task SignOutAsync()
    {
        var email = _currentUser?.Email;
        _logger?.LogInformation("[AUTH] Sign-out started for user: {Email}", email);
        await _authService.SignOutAsync();
        _currentUser = null;
        _logger?.LogInformation("[AUTH] User {Email} signed out successfully", email);
        OnAuthenticationStateChanged();
    }

    /// <summary>
    /// Get the current ID token (not used in local-only mode)
    /// </summary>
    public string? GetIdToken()
    {
        return null;
    }

    /// <summary>
    /// Refresh the ID token if needed (not used in local-only mode)
    /// </summary>
    public async Task<bool> RefreshTokenIfNeededAsync()
    {
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Send password reset email (not supported in local-only mode)
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string email)
    {
        await _authService.SendPasswordResetEmailAsync(email);
    }
}

/// <summary>
/// Interface for user session management
/// </summary>
public interface IUserSessionService
{
    bool IsAuthenticated { get; }
    User? CurrentUser { get; }
    event EventHandler? AuthenticationStateChanged;
    Task<bool> InitializeAsync();
    Task<SessionResult> SignInAsync(string email, string password, bool rememberMe = false);
    Task<SessionResult> SignUpAsync(string email, string password, string displayName, bool rememberMe = false);
    Task SignOutAsync();
    string? GetIdToken();
    Task<bool> RefreshTokenIfNeededAsync();
    Task SendPasswordResetEmailAsync(string email);
}

/// <summary>
/// Result of a session operation
/// </summary>
public class SessionResult
{
    public bool IsSuccess { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static SessionResult Success(User user) => new()
    {
        IsSuccess = true,
        User = user
    };

    public static SessionResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
