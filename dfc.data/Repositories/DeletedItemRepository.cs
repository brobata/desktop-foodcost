using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class DeletedItemRepository : IDeletedItemRepository
{
    private readonly DfcDbContext _context;

    public DeletedItemRepository(DfcDbContext context)
    {
        _context = context;
    }

    public async Task<DeletedItem?> GetByIdAsync(Guid id)
    {
        return await _context.DeletedItems.FindAsync(id);
    }

    public async Task<IEnumerable<DeletedItem>> GetAllAsync(Guid locationId)
    {
        return await _context.DeletedItems
            .Where(di => di.LocationId == locationId)
            .OrderByDescending(di => di.DeletedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeletedItem>> GetByTypeAsync(Guid locationId, DeletedItemType type)
    {
        return await _context.DeletedItems
            .Where(di => di.LocationId == locationId && di.ItemType == type)
            .OrderByDescending(di => di.DeletedDate)
            .ToListAsync();
    }

    public async Task<DeletedItem> AddAsync(DeletedItem deletedItem)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Starting AddAsync");
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] ItemId: {deletedItem.ItemId}");
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] ItemType: {deletedItem.ItemType}");
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] ItemName: {deletedItem.ItemName}");
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] LocationId: {deletedItem.LocationId}");

            // CRITICAL: Generate new Id for the deleted item
            deletedItem.Id = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Generated new Id: {deletedItem.Id}");

            deletedItem.CreatedAt = DateTime.UtcNow;
            deletedItem.ModifiedAt = DateTime.UtcNow;

            // Set expiration date (30 days from now by default)
            if (!deletedItem.ExpirationDate.HasValue)
            {
                deletedItem.ExpirationDate = DateTime.UtcNow.AddDays(30);
            }

            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Clearing change tracker...");
            // CRITICAL: Clear change tracker BEFORE adding the DeletedItem
            _context.ChangeTracker.Clear();
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Change tracker cleared");

            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Adding to DbSet...");
            _context.DeletedItems.Add(deletedItem);

            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] Calling SaveChangesAsync...");
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"    [DELETED ITEM REPO] SaveChangesAsync complete");

            return deletedItem;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("    ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("    ║ [DELETED ITEM REPO EXCEPTION]                     ║");
            System.Diagnostics.Debug.WriteLine("    ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"    Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"    Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"    Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"    Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"    Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("    ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var deletedItem = await _context.DeletedItems.FindAsync(id);
        if (deletedItem != null)
        {
            _context.DeletedItems.Remove(deletedItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expiredItems = await _context.DeletedItems
            .Where(di => di.ExpirationDate.HasValue && di.ExpirationDate.Value < now)
            .ToListAsync();

        if (expiredItems.Any())
        {
            _context.DeletedItems.RemoveRange(expiredItems);
            await _context.SaveChangesAsync();
        }
    }
}
