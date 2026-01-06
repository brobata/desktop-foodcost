using Desktop Food CostUser = Dfc.Core.Models.User;
using SupabaseUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;

namespace Dfc.Core.Services;

/// <summary>
/// Supabase Authentication Service
/// Replaces Firebase Auth with Supabase Auth (Gotrue)
/// </summary>
public class SupabaseAuthService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SupabaseAuthService>? _logger;

    private Desktop Food CostUser? _currentUser;
    private SupabaseClient? _supabaseClient;

    public SupabaseAuthService(
        IUserRepository userRepository,
        ILogger<SupabaseAuthService>? logger = null)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public bool IsAuthenticated => _currentUser != null && _supabaseClient?.Auth.CurrentUser != null;

    /// <summary>
    /// Sign in with email and password using Supabase Auth
    /// </summary>
    public async Task<Desktop Food CostUser> SignInAsync(string email, string password)
    {
        Debug.WriteLine($"[Auth] SignInAsync called for email: {email}");

        try
        {
            // Get Supabase client (will throw if credentials not configured)
            _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine("[Auth] Supabase client initialized");

            // Sign in with Supabase Auth
            Debug.WriteLine("[Auth] Sending authentication request to Supabase...");
            var session = await _supabaseClient.Auth.SignIn(email, password);

            if (session?.User == null)
            {
                Debug.WriteLine("[Auth] ERROR: Sign in returned null session or user");
                throw new UnauthorizedAccessException("Authentication failed - no session returned");
            }

            Debug.WriteLine($"[Auth] Authentication successful, Supabase UID: {session.User.Id}");

            // CRITICAL FIX: Refresh the session to ensure auth headers are propagated to Postgrest client
            // This is a workaround for supabase-csharp 0.16.2 not automatically setting Authorization headers
            Debug.WriteLine("[Auth] Refreshing session to propagate auth headers to Postgrest client...");
            await _supabaseClient.Auth.RefreshSession();
            Debug.WriteLine("[Auth] ✓ Session refreshed - auth headers should now be set");

            // Check if refresh token was received
            var refreshToken = session.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                Debug.WriteLine("[AUTH] ⚠️ WARNING: No refresh token received from Supabase after sign-in!");
                _logger?.LogWarning("[AUTH] No refresh token received from Supabase - session may not be restorable");
            }
            else
            {
                Debug.WriteLine($"[AUTH] ✓ Refresh token received (length: {refreshToken.Length})");
                _logger?.LogInformation("[AUTH] Refresh token received from Supabase (length: {TokenLength})", refreshToken.Length);
            }

            // Get or create user in local database AND Supabase users table
            var supabaseUid = session.User.Id;
            Debug.WriteLine($"[Auth] Looking up user in local database by Supabase UID: {supabaseUid}");

            var user = await _userRepository.GetUserBySupabaseAuthUidAsync(supabaseUid);
            if (user == null)
            {
                Debug.WriteLine("[Auth] User not found in local database, creating new user...");

                // Create new user in local database
                // First user created gets Admin role, subsequent users get Viewer role
                var existingUsers = await _userRepository.GetAllUsersAsync();
                var isFirstUser = existingUsers.Count == 0;
                Debug.WriteLine($"[Auth] Existing users count: {existingUsers.Count}, isFirstUser: {isFirstUser}");

                user = new Desktop Food CostUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    SupabaseAuthUid = supabaseUid, // Using same field for Supabase UID
                    Role = isFirstUser ? Dfc.Core.Enums.UserRole.Admin : Dfc.Core.Enums.UserRole.Viewer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                await _userRepository.CreateAsync(user);
                Debug.WriteLine($"[Auth] ✓ Created new user {email} with role {user.Role} in local database");
                _logger?.LogInformation("Created new user {Email} with role {Role}", email, user.Role);

                // Create user record in Supabase users table
                Debug.WriteLine($"[Auth] Creating Supabase user record in users table");
                try
                {
                    var supabaseUser = new SupabaseUserModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        Status = (user.IsActive ? "active" : "inactive"),
                        CreatedAt = user.CreatedAt,
                        ModifiedAt = user.ModifiedAt
                    };

                    await _supabaseClient.From<SupabaseUserModel>().Insert(supabaseUser);
                    Debug.WriteLine($"[Auth] ✓ Created Supabase user record for {email}");
                    _logger?.LogInformation("Created Supabase user record for {Email}", email);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Auth] ⚠ Failed to create Supabase user record: {ex.Message}");
                    _logger?.LogWarning(ex, "Failed to create Supabase user record for {Email}", email);
                    // Don't fail login if Supabase table creation fails - user is still in local database
                }
            }
            else
            {
                Debug.WriteLine($"[Auth] Found existing user in local database: {user.Email}, Role: {user.Role}");
            }

            if (!user.IsActive)
            {
                Debug.WriteLine("[Auth] ERROR: Desktop Food CostUser account is disabled");
                throw new UnauthorizedAccessException("User account is disabled");
            }

            _currentUser = user;
            Debug.WriteLine($"[Auth] ✓ Sign in complete for {email}");
            _logger?.LogInformation("User {Email} signed in successfully", email);

            return user;
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            Debug.WriteLine($"[Auth] ❌ Sign in error: {ex.Message}");
            _logger?.LogError(ex, "Error during sign in for {Email}", email);

            // Provide helpful error messages
            if (ex.Message.Contains("Invalid login credentials") || ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            else if (ex.Message.Contains("Supabase credentials not configured"))
            {
                throw new InvalidOperationException(
                    "Online authentication is not available. The app is running in offline-only mode.\n\n" +
                    "To enable online features:\n" +
                    "1. Set the SUPABASE_URL and SUPABASE_ANON_KEY environment variables\n" +
                    "2. Restart the application");
            }
            else if (ex.Message.Contains("Network") || ex.Message.Contains("connection"))
            {
                throw new InvalidOperationException("Unable to connect to authentication server. Please check your internet connection.", ex);
            }

            throw new InvalidOperationException("An unexpected error occurred during sign in.", ex);
        }
    }

    /// <summary>
    /// Sign up a new user with email and password
    /// </summary>
    public async Task<Desktop Food CostUser> SignUpAsync(string email, string password, string displayName)
    {
        Debug.WriteLine($"[Auth] SignUpAsync called for email: {email}");

        try
        {
            // Get Supabase client (will throw if credentials not configured)
            _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine("[Auth] Supabase client initialized");

            // Sign up with Supabase Auth
            Debug.WriteLine("[Auth] Sending sign up request to Supabase...");
            var session = await _supabaseClient.Auth.SignUp(email, password);

            if (session?.User == null)
            {
                Debug.WriteLine("[Auth] ERROR: Sign up returned null session or user");
                throw new InvalidOperationException("Registration failed - no session returned");
            }

            Debug.WriteLine($"[Auth] Registration successful, Supabase UID: {session.User.Id}");

            // Create user in local database AND Supabase users table
            var supabaseUid = session.User.Id;

            // First user created gets Admin role, subsequent users get Viewer role
            var existingUsers = await _userRepository.GetAllUsersAsync();
            var isFirstUser = existingUsers.Count == 0;
            Debug.WriteLine($"[Auth] Existing users count: {existingUsers.Count}, isFirstUser: {isFirstUser}");

            var user = new Desktop Food CostUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                SupabaseAuthUid = supabaseUid, // Using same field for Supabase UID
                Role = isFirstUser ? Dfc.Core.Enums.UserRole.Admin : Dfc.Core.Enums.UserRole.Viewer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            await _userRepository.CreateAsync(user);
            Debug.WriteLine($"[Auth] ✓ Created new user {email} with role {user.Role} in local database");
            _logger?.LogInformation("Created new user {Email} with role {Role} in local database", email, user.Role);

            // Create user record in Supabase users table
            Debug.WriteLine($"[Auth] Creating Supabase user record in users table");
            try
            {
                var supabaseUser = new SupabaseUserModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Status = (user.IsActive ? "active" : "inactive"),
                    CreatedAt = user.CreatedAt,
                    ModifiedAt = user.ModifiedAt
                };

                await _supabaseClient.From<SupabaseUserModel>().Insert(supabaseUser);
                Debug.WriteLine($"[Auth] ✓ Created Supabase user record for {email}");
                _logger?.LogInformation("Created Supabase user record for {Email}", email);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Auth] ⚠ Failed to create Supabase user record: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to create Supabase user record for {Email}", email);
                // Don't fail signup if Supabase table creation fails - user is still in local database
            }

            _currentUser = user;
            Debug.WriteLine($"[Auth] ✓ Sign up complete for {email}");
            _logger?.LogInformation("User {Email} signed up successfully", email);

            return user;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            Debug.WriteLine($"[Auth] ❌ Sign up error: {ex.Message}");
            _logger?.LogError(ex, "Error during sign up for {Email}", email);

            // Provide helpful error messages
            if (ex.Message.Contains("User already registered") || ex.Message.Contains("already exists"))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }
            else if (ex.Message.Contains("Supabase credentials not configured"))
            {
                throw new InvalidOperationException(
                    "Online authentication is not available. The app is running in offline-only mode.\n\n" +
                    "To enable online features:\n" +
                    "1. Set the SUPABASE_URL and SUPABASE_ANON_KEY environment variables\n" +
                    "2. Restart the application");
            }
            else if (ex.Message.Contains("Network") || ex.Message.Contains("connection"))
            {
                throw new InvalidOperationException("Unable to connect to authentication server. Please check your internet connection.", ex);
            }

            throw new InvalidOperationException("An unexpected error occurred during registration.", ex);
        }
    }

    /// <summary>
    /// Sign out the current user
    /// </summary>
    public async Task SignOutAsync()
    {
        if (_currentUser != null)
        {
            _logger?.LogInformation("User {Email} signed out", _currentUser.Email);
            Debug.WriteLine($"[Auth] Signing out user {_currentUser.Email}");
        }

        // Sign out from Supabase Auth
        if (_supabaseClient != null)
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
                Debug.WriteLine("[Auth] ✓ Supabase sign out successful");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Auth] ⚠ Supabase sign out error: {ex.Message}");
                _logger?.LogWarning(ex, "Error signing out from Supabase");
            }
        }

        _currentUser = null;
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    public async Task<Desktop Food CostUser?> GetCurrentUserAsync()
    {
        // If we already have a current user, return it
        if (_currentUser != null)
        {
            return _currentUser;
        }

        // Try to restore session from Supabase
        try
        {
            _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            var supabaseUser = _supabaseClient.Auth.CurrentUser;

            if (supabaseUser != null)
            {
                Debug.WriteLine($"[Auth] Found existing Supabase session for user: {supabaseUser.Email}");

                // Load user from local database
                var user = await _userRepository.GetUserBySupabaseAuthUidAsync(supabaseUser.Id);
                if (user != null && user.IsActive)
                {
                    _currentUser = user;
                    Debug.WriteLine($"[Auth] ✓ Restored session for {user.Email}");
                    return user;
                }
                else
                {
                    Debug.WriteLine($"[Auth] ⚠ User not found in local database or inactive");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Auth] Error restoring session: {ex.Message}");
            _logger?.LogWarning(ex, "Error restoring session");
        }

        return null;
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string email)
    {
        try
        {
            _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            await _supabaseClient.Auth.ResetPasswordForEmail(email);

            Debug.WriteLine($"[Auth] ✓ Password reset email sent to {email}");
            _logger?.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Auth] Error sending password reset email: {ex.Message}");
            _logger?.LogError(ex, "Error sending password reset email to {Email}", email);
            // Don't throw - don't reveal if email exists or not
        }
    }

    /// <summary>
    /// Get the current session's access token
    /// </summary>
    public string? GetIdToken()
    {
        return _supabaseClient?.Auth.CurrentSession?.AccessToken;
    }

    /// <summary>
    /// Get the current session's access token
    /// </summary>
    public string? GetAccessToken()
    {
        return _supabaseClient?.Auth.CurrentSession?.AccessToken;
    }

    /// <summary>
    /// Get the current session's refresh token
    /// </summary>
    public string? GetRefreshToken()
    {
        var token = _supabaseClient?.Auth.CurrentSession?.RefreshToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.WriteLine("[AUTH] GetRefreshToken: No refresh token available (client or session is null)");
            _logger?.LogWarning("[AUTH] GetRefreshToken returned null or empty - client: {HasClient}, session: {HasSession}",
                _supabaseClient != null, _supabaseClient?.Auth.CurrentSession != null);
        }
        else
        {
            Debug.WriteLine($"[AUTH] GetRefreshToken: Retrieved token (length: {token.Length} characters)");
            _logger?.LogInformation("[AUTH] GetRefreshToken retrieved token (length: {TokenLength})", token.Length);
        }
        return token;
    }

    /// <summary>
    /// Refresh the current session token
    /// </summary>
    public async Task RefreshIdTokenAsync()
    {
        try
        {
            if (_supabaseClient == null)
            {
                _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            }

            await _supabaseClient.Auth.RefreshSession();
            Debug.WriteLine("[Auth] ✓ Token refreshed successfully");

            // Reload user from database
            if (_currentUser != null)
            {
                var user = await _userRepository.GetByIdAsync(_currentUser.Id);
                _currentUser = user;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Auth] ❌ Token refresh failed: {ex.Message}");
            _logger?.LogError(ex, "Error refreshing token");
            throw new UnauthorizedAccessException("Session expired. Please sign in again.", ex);
        }
    }

    /// <summary>
    /// Restore session from stored refresh token
    /// </summary>
    public async Task<Desktop Food CostUser?> RestoreSessionAsync(string refreshToken, string? accessToken = null)
    {
        Debug.WriteLine("[AUTH] RestoreSessionAsync called with stored refresh token");
        _logger?.LogInformation("[AUTH] RestoreSessionAsync called (token length: {TokenLength})", refreshToken?.Length ?? 0);

        try
        {
            // Get Supabase client
            _supabaseClient = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine("[AUTH] Supabase client initialized");
            _logger?.LogInformation("[AUTH] Supabase client initialized successfully");

            // Try to restore the session using SetSession with the refresh token
            // Note: We use an empty access token since it will be refreshed by the SDK
            Debug.WriteLine("[AUTH] Calling Supabase Auth.SetSession with refresh token...");
            _logger?.LogInformation("[AUTH] Calling Supabase Auth.SetSession to restore session");
            // Try to restore using just the refresh token
            // Supabase will generate a new access token from the refresh token
            // Use SetSession with both tokens
            var accessTokenToUse = accessToken ?? "";
            var session = await _supabaseClient.Auth.SetSession(accessTokenToUse, refreshToken);

            if (session?.User == null)
            {
                Debug.WriteLine("[AUTH] ✗ Session restoration returned null user");
                _logger?.LogWarning("[AUTH] Supabase Auth.SetSession returned null session or user - token may be expired or invalid");
                return null;
            }

            Debug.WriteLine($"[AUTH] ✓ Supabase session restored successfully, Supabase UID: {session.User.Id}");
            _logger?.LogInformation("[AUTH] Supabase session restored, UID: {Uid}, Email: {Email}", session.User.Id, session.User.Email);

            // Get user from local database
            var supabaseUid = session.User.Id;
            _logger?.LogInformation("[AUTH] Looking up user in local database by Supabase UID: {Uid}", supabaseUid);
            var user = await _userRepository.GetUserBySupabaseAuthUidAsync(supabaseUid);

            if (user == null)
            {
                Debug.WriteLine("[AUTH] ✗ User not found in local database");
                _logger?.LogWarning("[AUTH] User with Supabase UID {Uid} not found in local database", supabaseUid);
                return null;
            }

            if (!user.IsActive)
            {
                Debug.WriteLine("[AUTH] ✗ User account is disabled");
                _logger?.LogWarning("[AUTH] User {Email} account is disabled, cannot restore session", user.Email);
                return null;
            }

            _currentUser = user;
            Debug.WriteLine($"[AUTH] ✓ Session restored for {user.Email}");
            _logger?.LogInformation("[AUTH] ✓ Session restoration complete for user {Email}", user.Email);

            return user;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUTH] ✗ Session restoration error: {ex.Message}");
            _logger?.LogError(ex, "[AUTH] Exception during session restoration: {Message}. Inner: {InnerMessage}", 
                ex.Message, ex.InnerException?.Message ?? "none");
            return null;
        }
    }
}

/// <summary>
/// Supabase User model for users table
/// Maps to the users table in PostgreSQL
/// </summary>
[Postgrest.Attributes.Table("users")]
public class SupabaseUserModel : Postgrest.Models.BaseModel
{
    [Postgrest.Attributes.PrimaryKey("id")]
    public Guid Id { get; set; }

    [Postgrest.Attributes.Column("supabase_auth_uid")]
    public string SupabaseAuthUid { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("email")]
    public string Email { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("role")]
    public string Role { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("status")]
    public string Status { get; set; } = "active";

    [Postgrest.Attributes.Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Postgrest.Attributes.Column("modified_at")]
    public DateTime ModifiedAt { get; set; }
}
