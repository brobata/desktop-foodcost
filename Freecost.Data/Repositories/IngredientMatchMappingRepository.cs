using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Freecost.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Freecost.Data.Repositories;

public class IngredientMatchMappingRepository : IIngredientMatchMappingRepository
{
    private readonly FreecostDbContext _context;

    public IngredientMatchMappingRepository(FreecostDbContext context)
    {
        _context = context;
    }

    public async Task<List<IngredientMatchMapping>> GetAllByLocationAsync(Guid locationId)
    {
        return await _context.IngredientMatchMappings
            .AsNoTracking()
            .Include(m => m.MatchedIngredient)
            .Include(m => m.MatchedRecipe)
            .Where(m => m.LocationId == locationId)
            .OrderBy(m => m.ImportName)
            .ToListAsync();
    }

    public async Task<IngredientMatchMapping?> GetByIdAsync(Guid id)
    {
        return await _context.IngredientMatchMappings
            .AsNoTracking()
            .Include(m => m.MatchedIngredient)
            .Include(m => m.MatchedRecipe)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IngredientMatchMapping?> GetByImportNameAsync(string importName, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            return null;

        // Normalize the import name for matching (lowercase, trimmed)
        var normalizedName = importName.Trim().ToLowerInvariant();

        return await _context.IngredientMatchMappings
            .AsNoTracking()
            .Include(m => m.MatchedIngredient)
            .Include(m => m.MatchedRecipe)
            .FirstOrDefaultAsync(m => m.ImportName == normalizedName && m.LocationId == locationId);
    }

    public async Task<IngredientMatchMapping> AddAsync(IngredientMatchMapping mapping)
    {
        mapping.Id = Guid.NewGuid();
        mapping.CreatedAt = DateTime.UtcNow;
        mapping.ModifiedAt = DateTime.UtcNow;

        // Normalize the import name (lowercase, trimmed)
        mapping.ImportName = mapping.ImportName.Trim().ToLowerInvariant();

        // CRITICAL: Clear navigation properties before Add to avoid EF foreign key constraint violations
        mapping.MatchedIngredient = null;
        mapping.MatchedRecipe = null;
        mapping.Location = null;

        _context.IngredientMatchMappings.Add(mapping);
        await _context.SaveChangesAsync();

        return mapping;
    }

    public async Task<IngredientMatchMapping> UpdateAsync(IngredientMatchMapping mapping)
    {
        mapping.ModifiedAt = DateTime.UtcNow;

        // Normalize the import name (lowercase, trimmed)
        mapping.ImportName = mapping.ImportName.Trim().ToLowerInvariant();

        // Update using raw SQL to avoid EF tracking issues
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE IngredientMatchMappings
              SET ImportName = {0},
                  MatchedIngredientId = {1},
                  MatchedRecipeId = {2},
                  ModifiedAt = {3}
              WHERE Id = {4}",
            mapping.ImportName,
            mapping.MatchedIngredientId ?? (object)DBNull.Value,
            mapping.MatchedRecipeId ?? (object)DBNull.Value,
            mapping.ModifiedAt,
            mapping.Id);

        // CRITICAL: Clear EF's change tracker so it doesn't return stale cached entities
        _context.ChangeTracker.Clear();

        // Reload with navigation properties
        return await GetByIdAsync(mapping.Id) ?? mapping;
    }

    public async Task DeleteAsync(Guid id)
    {
        // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
        _context.ChangeTracker.Clear();

        // Delete using raw SQL to avoid EF tracking issues
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM IngredientMatchMappings WHERE Id = {0}", id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.IngredientMatchMappings.AnyAsync(m => m.Id == id);
    }

    public async Task<bool> ExistsByImportNameAsync(string importName, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            return false;

        // Normalize the import name for matching (lowercase, trimmed)
        var normalizedName = importName.Trim().ToLowerInvariant();

        return await _context.IngredientMatchMappings
            .AnyAsync(m => m.ImportName == normalizedName && m.LocationId == locationId);
    }
}
