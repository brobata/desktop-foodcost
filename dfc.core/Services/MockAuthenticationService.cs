using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Local-only authentication service.
/// In local mode, users work with local data without cloud authentication.
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    private User? _currentUser = null;

    // Local-only mode: authentication is not required
    public bool IsAuthenticated => false;

    public async Task<User?> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        return null;
    }

    public async Task<User> SignInAsync(string email, string password)
    {
        // Local-only mode - create a local user
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
        // Local-only mode - create a local user
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
        // Not supported in local-only mode
        await Task.CompletedTask;
    }
}
