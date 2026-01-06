using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Data.Repositories;

public class WasteRecordRepository : IWasteRecordRepository
{
    private readonly FreecostDbContext _context;

    public WasteRecordRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<WasteRecord?> GetByIdAsync(Guid id)
    {
        return await _context.WasteRecords
            .Include(wr => wr.Ingredient)
            .FirstOrDefaultAsync(wr => wr.Id == id);
    }

    public async Task<IEnumerable<WasteRecord>> GetAllAsync(Guid locationId)
    {
        return await _context.WasteRecords
            .Include(wr => wr.Ingredient)
            .Where(wr => wr.LocationId == locationId)
            .OrderByDescending(wr => wr.WasteDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WasteRecord>> GetByDateRangeAsync(Guid locationId, DateTime startDate, DateTime endDate)
    {
        return await _context.WasteRecords
            .Include(wr => wr.Ingredient)
            .Where(wr => wr.LocationId == locationId &&
                        wr.WasteDate >= startDate &&
                        wr.WasteDate <= endDate)
            .OrderByDescending(wr => wr.WasteDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WasteRecord>> GetByIngredientIdAsync(Guid ingredientId)
    {
        return await _context.WasteRecords
            .Include(wr => wr.Ingredient)
            .Where(wr => wr.IngredientId == ingredientId)
            .OrderByDescending(wr => wr.WasteDate)
            .ToListAsync();
    }

    public async Task<WasteRecord> AddAsync(WasteRecord wasteRecord)
    {
        wasteRecord.CreatedAt = DateTime.UtcNow;
        wasteRecord.ModifiedAt = DateTime.UtcNow;

        _context.WasteRecords.Add(wasteRecord);
        await _context.SaveChangesAsync();
        return wasteRecord;
    }

    public async Task<WasteRecord> UpdateAsync(WasteRecord wasteRecord)
    {
        wasteRecord.ModifiedAt = DateTime.UtcNow;

        _context.WasteRecords.Update(wasteRecord);
        await _context.SaveChangesAsync();
        return wasteRecord;
    }

    public async Task DeleteAsync(Guid id)
    {
        var wasteRecord = await _context.WasteRecords.FindAsync(id);
        if (wasteRecord != null)
        {
            _context.WasteRecords.Remove(wasteRecord);
            await _context.SaveChangesAsync();
        }
    }
}
