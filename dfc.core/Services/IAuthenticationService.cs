using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Generic authentication service interface
/// TODO: Currently using SupabaseAuthService for authentication
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets the currently authenticated user
    /// </summary>
    Task<User?> GetCurrentUserAsync();

    /// <summary>
    /// Signs in with email and password
    /// </summary>
    Task<User> SignInAsync(string email, string password);

    /// <summary>
    /// Signs up a new user with email and password
    /// </summary>
    Task<User> SignUpAsync(string email, string password, string displayName);

    /// <summary>
    /// Signs out the current user
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Resets password for the given email
    /// </summary>
    Task SendPasswordResetEmailAsync(string email);

    /// <summary>
    /// Checks if a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
