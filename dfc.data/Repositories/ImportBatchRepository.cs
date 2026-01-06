using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class ImportBatchRepository : IImportBatchRepository
{
    private readonly DfcDbContext _context;

    public ImportBatchRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<List<ImportBatch>> GetRecentAsync(Guid locationId, int count = 10)
    {
        return await _context.ImportBatches
            .AsNoTracking()
            .Where(b => b.LocationId == locationId)
            .OrderByDescending(b => b.ImportedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ImportBatch?> GetByIdWithItemsAsync(Guid id)
    {
        return await _context.ImportBatches
            .AsNoTracking()
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<ImportBatch> AddAsync(ImportBatch batch)
    {
        batch.CreatedAt = DateTime.UtcNow;
        batch.ModifiedAt = DateTime.UtcNow;

        _context.ImportBatches.Add(batch);
        await _context.SaveChangesAsync();
        return batch;
    }

    public async Task AddItemsAsync(Guid batchId, List<ImportBatchItem> items)
    {
        foreach (var item in items)
        {
            item.ImportBatchId = batchId;
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();
        }

        _context.ImportBatchItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task ExpireUndoAsync(Guid batchId)
    {
        var batch = await _context.ImportBatches.FindAsync(batchId);
        if (batch != null)
        {
            batch.CanUndo = false;
            batch.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ImportBatch>> GetExpiredUndoBatchesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ImportBatches
            .Where(b => b.CanUndo && b.UndoExpiresAt <= now)
            .ToListAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var batch = await _context.ImportBatches
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (batch != null)
        {
            _context.ImportBatches.Remove(batch);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CanUndoAsync(Guid batchId)
    {
        var batch = await _context.ImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId);

        if (batch == null)
            return false;

        return batch.CanUndo && batch.UndoExpiresAt > DateTime.UtcNow;
    }
}
