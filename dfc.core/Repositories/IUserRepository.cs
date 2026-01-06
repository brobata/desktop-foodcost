using Dfc.Core.Enums;
using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserBySupabaseAuthUidAsync(string supabaseAuthUid);
    Task<List<User>> GetAllUsersAsync();
    Task<List<User>> GetUsersByRoleAsync(UserRole role);
    Task<List<User>> GetActiveUsersAsync();
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string email);
}
