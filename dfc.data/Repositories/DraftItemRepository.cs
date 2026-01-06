using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class DraftItemRepository : IDraftItemRepository
{
    private readonly DfcDbContext _context;

    public DraftItemRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<DraftItem> AddAsync(DraftItem draftItem)
    {
        draftItem.LastSavedAt = DateTime.UtcNow;
        _context.DraftItems.Add(draftItem);
        await _context.SaveChangesAsync();
        return draftItem;
    }

    public async Task<DraftItem?> GetByIdAsync(Guid id)
    {
        return await _context.DraftItems.FindAsync(id);
    }

    public async Task<List<DraftItem>> GetAllAsync()
    {
        return await _context.DraftItems
            .OrderByDescending(d => d.LastSavedAt)
            .ToListAsync();
    }

    public async Task<List<DraftItem>> GetByTypeAsync(DraftType type)
    {
        return await _context.DraftItems
            .Where(d => d.DraftType == type)
            .OrderByDescending(d => d.LastSavedAt)
            .ToListAsync();
    }

    public async Task<DraftItem?> GetByOriginalItemIdAsync(Guid originalItemId)
    {
        return await _context.DraftItems
            .Where(d => d.OriginalItemId == originalItemId)
            .OrderByDescending(d => d.LastSavedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(DraftItem draftItem)
    {
        draftItem.LastSavedAt = DateTime.UtcNow;
        _context.DraftItems.Update(draftItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var draft = await _context.DraftItems.FindAsync(id);
        if (draft != null)
        {
            _context.DraftItems.Remove(draft);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOldDraftsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldDrafts = await _context.DraftItems
            .Where(d => d.LastSavedAt < cutoffDate)
            .ToListAsync();

        _context.DraftItems.RemoveRange(oldDrafts);
        await _context.SaveChangesAsync();
    }
}
