using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Freecost.Core.Models;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Manages user session state, token persistence, and automatic authentication
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
            "Freecost"
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
    /// Initialize session - attempt to restore previous session if "remember me" was enabled
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        _logger?.LogInformation("[AUTH] Session initialization started");

        if (_isInitialized)
        {
            _logger?.LogInformation("[AUTH] Already initialized. IsAuthenticated: {IsAuthenticated}", IsAuthenticated);
            return IsAuthenticated;
        }

        _isInitialized = true;

        // Try to restore saved session
        var sessionData = LoadSessionData();
        if (sessionData != null)
        {
            System.Diagnostics.Debug.WriteLine($"[AUTH] SESSION FILE FOUND at: {_sessionFilePath}");
            System.Diagnostics.Debug.WriteLine($"[AUTH]   Email: {sessionData.Email}");
            System.Diagnostics.Debug.WriteLine($"[AUTH]   RememberMe: {sessionData.RememberMe}");
            System.Diagnostics.Debug.WriteLine($"[AUTH]   HasRefreshToken: {!string.IsNullOrEmpty(sessionData.RefreshToken)}");
            System.Diagnostics.Debug.WriteLine($"[AUTH]   RefreshToken Length: {sessionData.RefreshToken?.Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[AUTH]   LastSignIn: {sessionData.LastSignIn}");

            _logger?.LogInformation("[AUTH] Session file found - Email: {Email}, RememberMe: {RememberMe}, HasRefreshToken: {HasToken}, LastSignIn: {LastSignIn}",
                sessionData.Email, sessionData.RememberMe, !string.IsNullOrEmpty(sessionData.RefreshToken), sessionData.LastSignIn);

            if (sessionData.RememberMe)
            {
                _logger?.LogInformation("[AUTH] Attempting to restore previous session for {Email}", sessionData.Email);

                try
                {
                    // Use the stored refresh token to restore the session
                    if (_authService is SupabaseAuthService supabaseAuth)
                    {
                        if (string.IsNullOrEmpty(sessionData.RefreshToken))
                        {
                            _logger?.LogWarning("[AUTH] Session data contains no refresh token for {Email} - cannot restore session", sessionData.Email);
                            ClearSessionData();
                            return false;
                        }

                        _logger?.LogInformation("[AUTH] Calling RestoreSessionAsync with refresh token (length: {TokenLength})", sessionData.RefreshToken.Length);
                        // Pass both access and refresh tokens for proper restoration
                        _currentUser = await supabaseAuth.RestoreSessionAsync(sessionData.RefreshToken, sessionData.AccessToken);

                        if (_currentUser != null)
                        {
                            _logger?.LogInformation("[AUTH] ✓ Session restored successfully for {Email}, User ID: {UserId}", _currentUser.Email, _currentUser.SupabaseAuthUid);
                            OnAuthenticationStateChanged();
                            return true;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AUTH] SESSION RESTORATION FAILED - returned null user");
                            System.Diagnostics.Debug.WriteLine($"[AUTH]   Email attempted: {sessionData.Email}");
                            System.Diagnostics.Debug.WriteLine($"[AUTH]   Check Supabase logs for auth errors");

                            _logger?.LogWarning("[AUTH] ✗ Session restoration returned null user for {Email} - clearing session data", sessionData.Email);
                            ClearSessionData();
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("[AUTH] Authentication service is not SupabaseAuthService (type: {Type}) - session restoration not supported", _authService?.GetType().Name ?? "null");
                        ClearSessionData();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "[AUTH] Exception during session restoration for {Email}: {Message}", sessionData.Email, ex.Message);
                    ClearSessionData();
                }
            }
            else
            {
                _logger?.LogInformation("[AUTH] RememberMe is false - skipping session restoration");
            }
        }
        else
        {
            _logger?.LogInformation("[AUTH] No session file found or session data is null");
        }

        _logger?.LogInformation("[AUTH] Session initialization completed. Authenticated: {IsAuthenticated}", IsAuthenticated);
        return false;
    }

    /// <summary>
    /// Sign in with email and password
    /// </summary>
    public async Task<SessionResult> SignInAsync(string email, string password, bool rememberMe = false)
    {
        SafeLogToFile($"SignInAsync called - Email: {email}, RememberMe: {rememberMe}");

        try
        {
            var user = await _authService.SignInAsync(email, password);
            _currentUser = user;
            SafeLogToFile($"SignIn successful - User: {user.Email}");

            // Save session if remember me is enabled
            if (rememberMe)
            {
                SafeLogToFile("RememberMe=true, calling SaveSessionData...");
                SaveSessionData(email, rememberMe);
                SafeLogToFile("SaveSessionData completed");
            }
            else
            {
                ClearSessionData();
            }

            _logger?.LogInformation("User {Email} signed in successfully", email);
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

            // Provide more detailed error message for troubleshooting
            var detailedError = $"Sign-in error: {ex.Message}";
            if (ex.InnerException != null)
            {
                detailedError += $"\nDetails: {ex.InnerException.Message}";
            }

            _logger?.LogError("Full error details for user: {DetailedError}", detailedError);
            return SessionResult.Failure(detailedError);
        }
    }

    /// <summary>
    /// Sign up a new user
    /// </summary>
    public async Task<SessionResult> SignUpAsync(string email, string password, string displayName, bool rememberMe = false)
    {
        try
        {
            var user = await _authService.SignUpAsync(email, password, displayName);
            _currentUser = user;

            // Save session if remember me is enabled
            if (rememberMe)
            {
                SaveSessionData(email, rememberMe);
            }

            _logger?.LogInformation("User {Email} signed up successfully", email);
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
        ClearSessionData();
        _logger?.LogInformation("[AUTH] User {Email} signed out successfully, session data cleared", email);
        OnAuthenticationStateChanged();
    }

    /// <summary>
    /// Get the current ID token for making authenticated requests
    /// </summary>
    public string? GetIdToken()
    {
        if (_authService is SupabaseAuthService supabaseAuth)
        {
            return supabaseAuth.GetIdToken();
        }
        return null;
    }

    /// <summary>
    /// Refresh the ID token if needed
    /// </summary>
    public async Task<bool> RefreshTokenIfNeededAsync()
    {
        try
        {
            if (_authService is SupabaseAuthService supabaseAuth)
            {
                await supabaseAuth.RefreshIdTokenAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Token refresh failed");
            return false;
        }
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string email)
    {
        await _authService.SendPasswordResetEmailAsync(email);
    }

    /// <summary>
    /// Safe file logging helper - ensures directory exists and handles errors gracefully
    /// </summary>
    private void SafeLogToFile(string message)
    {
        try
        {
            var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");
            Directory.CreateDirectory(logFolder); // Ensures directory exists
            var logFile = Path.Combine(logFolder, $"auth_{DateTime.Now:yyyyMMdd}.txt");
            File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
        }
        catch
        {
            // Silently ignore logging errors to prevent UI freezes
            // Errors are still logged via ILogger if available
        }
    }

    #region Session Persistence

    private void SaveSessionData(string email, bool rememberMe)
    {
        SafeLogToFile($"SaveSessionData called - Email: {email}");

        try
        {
            _logger?.LogInformation("[AUTH] Saving session data for email: {Email}, RememberMe: {RememberMe}", email, rememberMe);
            SafeLogToFile("Inside SaveSessionData try block");

            string? accessToken = null;
            string? refreshToken = null;
            if (_authService is SupabaseAuthService supabaseAuth)
            {
                accessToken = supabaseAuth.GetAccessToken();
                refreshToken = supabaseAuth.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger?.LogWarning("[AUTH] ⚠️ Refresh token is null or empty from SupabaseAuthService.GetRefreshToken() - session will not be restorable!");
                }
                else
                {
                    _logger?.LogInformation("[AUTH] Refresh token retrieved successfully (length: {TokenLength} characters)", refreshToken.Length);
                }
            }
            else
            {
                _logger?.LogWarning("[AUTH] Authentication service is not SupabaseAuthService (type: {Type}) - cannot get refresh token", _authService?.GetType().Name ?? "null");
            }

            var sessionData = new SessionData
            {
                Email = email,
                RememberMe = rememberMe,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                LastSignIn = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_sessionFilePath, json);
            SafeLogToFile($"Session file written to: {_sessionFilePath}");
            SafeLogToFile($"File exists: {File.Exists(_sessionFilePath)}");
            SafeLogToFile($"File size: {new FileInfo(_sessionFilePath).Length} bytes");
            _logger?.LogInformation("[AUTH] ✓ Session data written to: {FilePath}", _sessionFilePath);
        }
        catch (Exception ex)
        {
            SafeLogToFile($"ERROR in SaveSessionData: {ex.Message}");
            SafeLogToFile($"Stack trace: {ex.StackTrace}");
            _logger?.LogError(ex, "[AUTH] ✗ Failed to save session data: {Message}", ex.Message);
        }
    }

    private SessionData? LoadSessionData()
    {
        try
        {
            if (!File.Exists(_sessionFilePath))
                return null;

            var json = File.ReadAllText(_sessionFilePath);
            var sessionData = JsonSerializer.Deserialize<SessionData>(json);

            // Check if session is too old (30 days)
            if (sessionData != null && (DateTime.UtcNow - sessionData.LastSignIn).TotalDays > 30)
            {
                _logger?.LogInformation("Session data expired for {Email}", sessionData.Email);
                ClearSessionData();
                return null;
            }

            return sessionData;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load session data");
            return null;
        }
    }

    private void ClearSessionData()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
            {
                File.Delete(_sessionFilePath);
                _logger?.LogInformation("[AUTH] Session file deleted: {FilePath}", _sessionFilePath);
            }
            else
            {
                _logger?.LogDebug("[AUTH] No session file to clear");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[AUTH] ✗ Failed to clear session data: {Message}", ex.Message);
        }
    }

    #endregion

    private class SessionData
    {
        public string Email { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? AccessToken { get; set; }  // Added for proper session restoration
        public string? RefreshToken { get; set; }
        public DateTime LastSignIn { get; set; }
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
