using Freecost.Core.Enums;
using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FreecostDbContext _context;

    public UserRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserBySupabaseAuthUidAsync(string supabaseAuthUid)
    {
        return await _context.Users
            .Include(u => u.Location)
            .FirstOrDefaultAsync(u => u.SupabaseAuthUid == supabaseAuthUid);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Location)
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Include(u => u.Location)
            .Where(u => u.Role == role)
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Location)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            // Soft delete
            user.IsActive = false;
            await UpdateAsync(user);
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}
