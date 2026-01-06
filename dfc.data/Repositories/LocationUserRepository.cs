using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class LocationUserRepository : ILocationUserRepository
{
    private readonly DfcDbContext _context;

    public LocationUserRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<List<LocationUser>> GetByLocationIdAsync(Guid locationId)
    {
        return await _context.LocationUsers
            .AsNoTracking()
            .Where(lu => lu.LocationId == locationId)
            .OrderBy(lu => lu.UserId)
            .ToListAsync();
    }

    public async Task<LocationUser?> GetByIdAsync(Guid id)
    {
        return await _context.LocationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.Id == id);
    }

    public async Task<LocationUser?> GetByLocationAndUserAsync(Guid locationId, string userId)
    {
        return await _context.LocationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(lu => lu.LocationId == locationId && lu.UserId == userId);
    }

    public async Task<LocationUser> AddAsync(LocationUser locationUser)
    {
        locationUser.CreatedAt = DateTime.UtcNow;
        locationUser.ModifiedAt = DateTime.UtcNow;

        await _context.LocationUsers.AddAsync(locationUser);
        await _context.SaveChangesAsync();
        return locationUser;
    }

    public async Task UpdateAsync(LocationUser locationUser)
    {
        locationUser.ModifiedAt = DateTime.UtcNow;
        _context.LocationUsers.Update(locationUser);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var locationUser = await _context.LocationUsers.FindAsync(id);
        if (locationUser != null)
        {
            _context.LocationUsers.Remove(locationUser);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByLocationIdAsync(Guid locationId)
    {
        var locationUsers = await _context.LocationUsers
            .Where(lu => lu.LocationId == locationId)
            .ToListAsync();

        _context.LocationUsers.RemoveRange(locationUsers);
        await _context.SaveChangesAsync();
    }
}
