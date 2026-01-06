using Freecost.Core.Enums;
using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Freecost.Data.Repositories;

/// <summary>
/// Repository implementation for IngredientConversion entities.
/// </summary>
public class IngredientConversionRepository : IIngredientConversionRepository
{
    private readonly FreecostDbContext _context;

    public IngredientConversionRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<IngredientConversion?> GetConversionAsync(
        Guid ingredientId,
        UnitType fromUnit,
        UnitType toUnit,
        Guid? locationId = null)
    {
        // Priority order:
        // 1. Ingredient-specific conversion for this location
        // 2. Ingredient-specific conversion (global)
        // 3. Generic conversion for this location
        // 4. Generic conversion (global)

        // Try ingredient-specific for this location
        if (locationId.HasValue)
        {
            var locationSpecific = await _context.IngredientConversions
                .Where(c => c.IngredientId == ingredientId &&
                           c.LocationId == locationId &&
                           c.FromUnit == fromUnit &&
                           c.ToUnit == toUnit)
                .FirstOrDefaultAsync();

            if (locationSpecific != null)
                return locationSpecific;
        }

        // Try ingredient-specific global
        var ingredientSpecific = await _context.IngredientConversions
            .Where(c => c.IngredientId == ingredientId &&
                       c.LocationId == null &&
                       c.FromUnit == fromUnit &&
                       c.ToUnit == toUnit)
            .FirstOrDefaultAsync();

        if (ingredientSpecific != null)
            return ingredientSpecific;

        // Try generic for this location
        if (locationId.HasValue)
        {
            var genericLocation = await _context.IngredientConversions
                .Where(c => c.IngredientId == null &&
                           c.LocationId == locationId &&
                           c.FromUnit == fromUnit &&
                           c.ToUnit == toUnit)
                .FirstOrDefaultAsync();

            if (genericLocation != null)
                return genericLocation;
        }

        // Try generic global
        var genericGlobal = await _context.IngredientConversions
            .Where(c => c.IngredientId == null &&
                       c.LocationId == null &&
                       c.FromUnit == fromUnit &&
                       c.ToUnit == toUnit)
            .FirstOrDefaultAsync();

        return genericGlobal;
    }

    public async Task<List<IngredientConversion>> GetByIngredientIdAsync(Guid ingredientId)
    {
        return await _context.IngredientConversions
            .Where(c => c.IngredientId == ingredientId)
            .OrderBy(c => c.FromUnit)
            .ThenBy(c => c.ToUnit)
            .ToListAsync();
    }

    public async Task<List<IngredientConversion>> GetGenericConversionsAsync(Guid? locationId = null)
    {
        return await _context.IngredientConversions
            .Where(c => c.IngredientId == null && c.LocationId == locationId)
            .OrderBy(c => c.Source)
            .ThenBy(c => c.FromUnit)
            .ToListAsync();
    }

    public async Task<List<IngredientConversion>> GetBySourceAsync(string source)
    {
        return await _context.IngredientConversions
            .Where(c => c.Source == source)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<IngredientConversion>> GetByLocationIdAsync(Guid locationId)
    {
        // Include both location-specific and global conversions
        return await _context.IngredientConversions
            .Where(c => c.LocationId == locationId || c.LocationId == null)
            .OrderBy(c => c.IngredientId)
            .ThenBy(c => c.FromUnit)
            .ToListAsync();
    }

    public async Task AddAsync(IngredientConversion conversion)
    {
        await _context.IngredientConversions.AddAsync(conversion);
    }

    public async Task AddRangeAsync(List<IngredientConversion> conversions)
    {
        await _context.IngredientConversions.AddRangeAsync(conversions);
    }

    public async Task UpdateAsync(IngredientConversion conversion)
    {
        conversion.ModifiedAt = DateTime.UtcNow;
        _context.IngredientConversions.Update(conversion);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var conversion = await _context.IngredientConversions.FindAsync(id);
        if (conversion != null)
        {
            _context.IngredientConversions.Remove(conversion);
        }
    }

    public async Task DeleteByIngredientIdAsync(Guid ingredientId)
    {
        var conversions = await _context.IngredientConversions
            .Where(c => c.IngredientId == ingredientId)
            .ToListAsync();

        _context.IngredientConversions.RemoveRange(conversions);
    }

    public async Task DeleteBySourceAsync(string source, Guid? ingredientId = null)
    {
        var query = _context.IngredientConversions.Where(c => c.Source == source);

        if (ingredientId.HasValue)
        {
            query = query.Where(c => c.IngredientId == ingredientId);
        }

        var conversions = await query.ToListAsync();
        _context.IngredientConversions.RemoveRange(conversions);
    }

    public async Task<bool> ExistsAsync(
        Guid? ingredientId,
        UnitType fromUnit,
        UnitType toUnit,
        Guid? locationId = null)
    {
        return await _context.IngredientConversions
            .AnyAsync(c => c.IngredientId == ingredientId &&
                          c.LocationId == locationId &&
                          c.FromUnit == fromUnit &&
                          c.ToUnit == toUnit);
    }

    public async Task<(int Total, int IngredientSpecific, int Generic, int USDA, int UserDefined)> GetStatisticsAsync()
    {
        var total = await _context.IngredientConversions.CountAsync();
        var ingredientSpecific = await _context.IngredientConversions.CountAsync(c => c.IngredientId != null);
        var generic = await _context.IngredientConversions.CountAsync(c => c.IngredientId == null);
        var usda = await _context.IngredientConversions.CountAsync(c => c.Source == "USDA");
        var userDefined = await _context.IngredientConversions.CountAsync(c => c.Source == "UserDefined");

        return (total, ingredientSpecific, generic, usda, userDefined);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
