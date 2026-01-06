using Freecost.Core.Models;
using System;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IUserPreferencesRepository
{
    Task<UserPreferences?> GetByUserIdAsync(Guid userId);
    Task<UserPreferences> CreateAsync(UserPreferences preferences);
    Task UpdateAsync(UserPreferences preferences);
    Task DeleteAsync(Guid userId);
}
