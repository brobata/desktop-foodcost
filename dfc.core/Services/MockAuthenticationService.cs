using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Mock authentication service for local-only mode.
/// Always returns not authenticated - users work locally without cloud auth.
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    private User? _currentUser = null;

    // Local-only mode: never authenticated with cloud
    public bool IsAuthenticated => false;

    public async Task<User?> GetCurrentUserAsync()
    {
        // Local-only mode: no cloud user
        await Task.CompletedTask;
        return null;
    }

    public async Task<User> SignInAsync(string email, string password)
    {
        // TODO: This is a mock service for testing - production uses SupabaseAuthService
        // For now, just create a mock user
        await Task.CompletedTask;

        _currentUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            LocationId = Guid.Parse("00000000-0000-0000-0000-000000000001")
        };

        return _currentUser;
    }

    public async Task<User> SignUpAsync(string email, string password, string displayName)
    {
        // TODO: This is a mock service for testing - production uses SupabaseAuthService
        // For now, just create a mock user
        await Task.CompletedTask;

        _currentUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            LocationId = Guid.Parse("00000000-0000-0000-0000-000000000001")
        };

        return _currentUser;
    }

    public async Task SignOutAsync()
    {
        await Task.CompletedTask;
        _currentUser = null;
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        // TODO: This is a mock service - production uses SupabaseAuthService
        await Task.CompletedTask;
        // For now, just a no-op
    }
}
