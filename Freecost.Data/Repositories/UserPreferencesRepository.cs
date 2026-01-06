using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class UserPreferencesRepository : IUserPreferencesRepository
{
    private readonly FreecostDbContext _context;

    public UserPreferencesRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreferences?> GetByUserIdAsync(Guid userId)
    {
        var preferences = await _context.UserPreferences
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        // Detach the User navigation property to prevent EF from tracking it
        // This prevents constraint violations when updating preferences
        if (preferences?.User != null)
        {
            _context.Entry(preferences.User).State = EntityState.Detached;
        }

        return preferences;
    }

    public async Task<UserPreferences> CreateAsync(UserPreferences preferences)
    {
        // Clear navigation property to prevent EF from tracking/updating related User entity
        preferences.User = null!;

        _context.UserPreferences.Add(preferences);
        await _context.SaveChangesAsync();
        return preferences;
    }

    public async Task UpdateAsync(UserPreferences preferences)
    {
        // Clear navigation property to prevent EF from tracking/updating related User entity
        preferences.User = null!;

        _context.UserPreferences.Update(preferences);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        var preferences = await GetByUserIdAsync(userId);
        if (preferences != null)
        {
            _context.UserPreferences.Remove(preferences);
            await _context.SaveChangesAsync();
        }
    }
}
