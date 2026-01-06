using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class ImportMapRepository : IImportMapRepository
{
    private readonly DfcDbContext _context;

    public ImportMapRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<List<ImportMap>> GetUserMapsAsync(Guid locationId)
    {
        return await _context.ImportMaps
            .AsNoTracking()
            .Where(m => m.LocationId == locationId && m.IsSavedByUser)
            .OrderByDescending(m => m.LastUsedAt)
            .ThenBy(m => m.DisplayName)
            .ToListAsync();
    }

    public async Task<ImportMap?> GetByIdAsync(Guid id)
    {
        return await _context.ImportMaps
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ImportMap> AddAsync(ImportMap map)
    {
        map.CreatedAt = DateTime.UtcNow;
        map.ModifiedAt = DateTime.UtcNow;

        _context.ImportMaps.Add(map);
        await _context.SaveChangesAsync();
        return map;
    }

    public async Task<ImportMap> UpdateAsync(ImportMap map)
    {
        map.ModifiedAt = DateTime.UtcNow;

        var existing = await _context.ImportMaps.FindAsync(map.Id);
        if (existing == null)
            throw new InvalidOperationException($"ImportMap with ID {map.Id} not found");

        _context.Entry(existing).CurrentValues.SetValues(map);
        await _context.SaveChangesAsync();
        return map;
    }

    public async Task DeleteAsync(Guid id)
    {
        var map = await _context.ImportMaps.FindAsync(id);
        if (map != null)
        {
            _context.ImportMaps.Remove(map);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ImportMap?> FindMatchingMapAsync(List<string> headers, Guid locationId)
    {
        // Get all user-saved maps for this location
        var userMaps = await _context.ImportMaps
            .AsNoTracking()
            .Where(m => m.LocationId == locationId && m.IsSavedByUser)
            .OrderByDescending(m => m.LastUsedAt) // Prefer recently used
            .ToListAsync();

        var headerSet = new HashSet<string>(headers.Select(h => h.Trim().ToLowerInvariant()));

        foreach (var map in userMaps)
        {
            if (string.IsNullOrEmpty(map.DetectionPattern))
                continue;

            var patterns = map.DetectionPattern
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLowerInvariant())
                .ToList();

            // Count how many patterns match
            var matchCount = patterns.Count(pattern =>
                headerSet.Any(h => h.Contains(pattern, StringComparison.OrdinalIgnoreCase)));

            // If 70% or more of patterns match, consider it a match
            if (patterns.Count > 0 && matchCount >= patterns.Count * 0.7)
            {
                return map;
            }
        }

        return null;
    }

    public async Task UpdateUsageAsync(Guid mapId)
    {
        var map = await _context.ImportMaps.FindAsync(mapId);
        if (map != null)
        {
            map.LastUsedAt = DateTime.UtcNow;
            map.ImportCount++;
            map.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
