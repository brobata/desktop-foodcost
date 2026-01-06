using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Core.Enums;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Freecost.Data.Repositories;

public class PriceHistoryRepository : IPriceHistoryRepository
{
    private readonly FreecostDbContext _context;

    public PriceHistoryRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<List<PriceHistory>> GetByIngredientIdAsync(Guid ingredientId)
    {
        return await _context.PriceHistories
            .Where(ph => ph.IngredientId == ingredientId)
            .OrderByDescending(ph => ph.RecordedDate)
            .ToListAsync();
    }

    public async Task<PriceHistory?> GetLatestForIngredientAsync(Guid ingredientId)
    {
        return await _context.PriceHistories
            .Where(ph => ph.IngredientId == ingredientId)
            .OrderByDescending(ph => ph.RecordedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PriceHistory>> GetAllUnaggregatedAsync()
    {
        return await _context.PriceHistories
            .Where(ph => !ph.IsAggregated)
            .OrderBy(ph => ph.RecordedDate)
            .ToListAsync();
    }

    public async Task AddAsync(PriceHistory priceHistory)
    {
        await _context.PriceHistories.AddAsync(priceHistory);
    }

    public async Task AddRangeAsync(List<PriceHistory> priceHistories)
    {
        await _context.PriceHistories.AddRangeAsync(priceHistories);
    }

    public async Task UpdateAsync(PriceHistory priceHistory)
    {
        _context.PriceHistories.Update(priceHistory);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var priceHistory = await _context.PriceHistories.FindAsync(id);
        if (priceHistory != null)
        {
            _context.PriceHistories.Remove(priceHistory);
        }
    }

    public async Task DeleteRangeAsync(List<PriceHistory> priceHistories)
    {
        _context.PriceHistories.RemoveRange(priceHistories);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
