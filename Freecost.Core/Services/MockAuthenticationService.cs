using Freecost.Core.Models;

namespace Freecost.Core.Services;

/// <summary>
/// Mock authentication service for development
/// TODO: Mock service - production uses SupabaseAuthService
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    private User? _currentUser;

    public bool IsAuthenticated => _currentUser != null;

    public async Task<User?> GetCurrentUserAsync()
    {
        // For now, return a mock user for testing
        // In production, this would check for stored auth tokens
        await Task.CompletedTask;

        if (_currentUser == null)
        {
            // Auto-login as default user for development
            _currentUser = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Email = "demo@freecost.com",
                LocationId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };
        }

        return _currentUser;
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
