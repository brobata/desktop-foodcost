using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class IngredientRepository : IIngredientRepository
{
    private readonly DfcDbContext _context;
    private readonly ILocalModificationService? _modificationService;

    public IngredientRepository(DfcDbContext context, ILocalModificationService? modificationService = null)
    {
        _context = context;
        _modificationService = modificationService;
    }

    public async Task<List<Ingredient>> GetAllAsync(Guid locationId)
    {
        // CRITICAL: Use AsNoTracking() for display queries
        // These entities are shown in UI grids and must not be tracked
        // Tracked entities cause constraint violations when adding/deleting
        return await _context.Ingredients
            .AsNoTracking()
            .Include(i => i.Aliases)  // NEW: Include aliases for search functionality
            .Include(i => i.IngredientAllergens)
                .ThenInclude(ia => ia.Allergen)
            .Where(i => i.LocationId == locationId)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Ingredient?> GetByIdAsync(Guid id)
    {
        // CRITICAL: Use AsNoTracking() to prevent EF from tracking this entity
        // When this is used for deletion (fresh copy for recycle bin), tracked entities
        // cause constraint violations when RecycleBinService clears navigation properties
        return await _context.Ingredients
            .AsNoTracking()
            .Include(i => i.Aliases)
            .Include(i => i.PriceHistory)
            .Include(i => i.IngredientAllergens)
                .ThenInclude(ia => ia.Allergen)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Ingredient?> GetBySkuAsync(string sku, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        return await _context.Ingredients
            .Include(i => i.Aliases)
            .Include(i => i.PriceHistory)
            .Include(i => i.IngredientAllergens)
                .ThenInclude(ia => ia.Allergen)
            .FirstOrDefaultAsync(i => i.VendorSku == sku && i.LocationId == locationId);
    }

    public async Task<Ingredient> AddAsync(Ingredient ingredient)
    {
        // CRITICAL: Only generate new ID if not already set
        if (ingredient.Id == Guid.Empty)
        {
            ingredient.Id = Guid.NewGuid();
        }

        ingredient.CreatedAt = DateTime.UtcNow;
        ingredient.ModifiedAt = DateTime.UtcNow;

        // Set IDs for allergens if any
        if (ingredient.IngredientAllergens != null)
        {
            foreach (var allergen in ingredient.IngredientAllergens)
            {
                allergen.IngredientId = ingredient.Id;
                allergen.CreatedAt = DateTime.UtcNow;
                allergen.ModifiedAt = DateTime.UtcNow;

                // CRITICAL: Clear child navigation properties
                allergen.Ingredient = null!;
                allergen.Allergen = null!;
            }
        }

        // Set IDs for aliases if any
        if (ingredient.Aliases != null)
        {
            foreach (var alias in ingredient.Aliases)
            {
                alias.Id = Guid.NewGuid();
                alias.IngredientId = ingredient.Id;
                alias.CreatedAt = DateTime.UtcNow;

                // CRITICAL: Clear child navigation properties
                alias.Ingredient = null!;
            }
        }

        // CRITICAL: Clear navigation properties before Add to avoid EF foreign key constraint violations
        // This is essential when syncing from Firebase where navigation properties may be populated
        ingredient.Location = null!;
        ingredient.PriceHistory = new();
        ingredient.RecipeIngredients = new();
        ingredient.EntreeIngredients = new();

        _context.Ingredients.Add(ingredient);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // If allergens fail due to FK constraints (Allergen doesn't exist locally),
            // retry without allergens. This can happen during sync when allergens aren't synced yet.
            ingredient.IngredientAllergens = new();
            _context.ChangeTracker.Clear();
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            // Same handling for SQLite-specific FK constraint failures
            ingredient.IngredientAllergens = new();
            _context.ChangeTracker.Clear();
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
        }

        // Track modification for delta sync
        if (_modificationService != null)
        {
            await _modificationService.TrackCreationAsync("Ingredient", ingredient.Id, ingredient.LocationId);
        }

        return ingredient;
    }

    public async Task<Ingredient> UpdateAsync(Ingredient ingredient)
    {
        // Step 1: Delete existing allergens first, outside of EF tracking
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM IngredientAllergens WHERE IngredientId = {0}", ingredient.Id);

        // Step 2: Update ingredient scalar properties using raw SQL
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE Ingredients SET Name = {0}, Category = {1}, CategoryColor = {2}, VendorName = {3},
              VendorSku = {4}, CurrentPrice = {5}, CaseQuantity = {6}, Unit = {7}, UseAlternateUnit = {8},
              AlternateUnit = {9}, AlternateConversionQuantity = {10}, AlternateConversionUnit = {11},
              CaloriesPerUnit = {12}, ProteinPerUnit = {13}, CarbohydratesPerUnit = {14}, FatPerUnit = {15},
              FiberPerUnit = {16}, SugarPerUnit = {17}, SodiumPerUnit = {18},
              ModifiedAt = {19} WHERE Id = {20}",
            ingredient.Name,
            ingredient.Category ?? string.Empty,
            ingredient.CategoryColor ?? string.Empty,
            ingredient.VendorName ?? string.Empty,
            ingredient.VendorSku ?? string.Empty,
            ingredient.CurrentPrice,
            ingredient.CaseQuantity,
            (int)ingredient.Unit,
            ingredient.UseAlternateUnit ? 1 : 0,
            ingredient.AlternateUnit.HasValue ? (object)(int)ingredient.AlternateUnit.Value : null!,
            ingredient.AlternateConversionQuantity ?? (object)null!,
            ingredient.AlternateConversionUnit.HasValue ? (object)(int)ingredient.AlternateConversionUnit.Value : null!,
            ingredient.CaloriesPerUnit ?? (object)null!,
            ingredient.ProteinPerUnit ?? (object)null!,
            ingredient.CarbohydratesPerUnit ?? (object)null!,
            ingredient.FatPerUnit ?? (object)null!,
            ingredient.FiberPerUnit ?? (object)null!,
            ingredient.SugarPerUnit ?? (object)null!,
            ingredient.SodiumPerUnit ?? (object)null!,
            DateTime.UtcNow,
            ingredient.Id);

        // Step 3: Insert new allergens
        if (ingredient.IngredientAllergens != null)
        {
            foreach (var allergen in ingredient.IngredientAllergens)
            {
                try
                {
                    var newId = Guid.NewGuid();
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO IngredientAllergens (Id, IngredientId, AllergenId, IsAutoDetected, IsEnabled, SourceIngredients, CreatedAt, ModifiedAt)
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                        newId,
                        ingredient.Id,
                        allergen.AllergenId,
                        allergen.IsAutoDetected ? 1 : 0,
                        allergen.IsEnabled ? 1 : 0,
                        allergen.SourceIngredients ?? string.Empty,
                        DateTime.UtcNow,
                        DateTime.UtcNow);
                }
                catch (DbUpdateException)
                {
                    // Skip allergens with FK constraint failures (Allergen doesn't exist locally)
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    // Skip allergens with FK constraint failures (Allergen doesn't exist locally)
                }
            }
        }

        // Step 4: Delete existing aliases
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM IngredientAliases WHERE IngredientId = {0}", ingredient.Id);

        // Step 5: Insert new aliases
        if (ingredient.Aliases != null)
        {
            foreach (var alias in ingredient.Aliases)
            {
                var newId = Guid.NewGuid();
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO IngredientAliases (Id, IngredientId, AliasName, IsPrimary, CreatedAt)
                      VALUES ({0}, {1}, {2}, {3}, {4})",
                    newId,
                    ingredient.Id,
                    alias.AliasName,
                    alias.IsPrimary ? 1 : 0,
                    DateTime.UtcNow);
            }
        }

        // CRITICAL: Clear EF's change tracker so it doesn't return stale cached entities
        _context.ChangeTracker.Clear();

        // Track modification for delta sync
        if (_modificationService != null)
        {
            await _modificationService.TrackUpdateAsync("Ingredient", ingredient.Id, ingredient.LocationId);
        }

        // Reload with navigation properties
        return await GetByIdAsync(ingredient.Id) ?? ingredient;
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Starting DeleteAsync");
            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Ingredient ID: {id}");

            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Clearing change tracker...");
            _context.ChangeTracker.Clear();
            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Change tracker cleared");

            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Finding ingredient...");
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                var locationId = ingredient.LocationId;
                var ingredientName = ingredient.Name; // CRITICAL: Store name before deletion for modification tracking
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Found ingredient: '{ingredientName}'. LocationId: {locationId}");

                // CRITICAL: Manually delete all child entities using raw SQL to avoid EF tracking issues
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting RecipeIngredients...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RecipeIngredients WHERE IngredientId = {0}", id);

                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting EntreeIngredients...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM EntreeIngredients WHERE IngredientId = {0}", id);

                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting IngredientAllergens...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM IngredientAllergens WHERE IngredientId = {0}", id);

                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting IngredientAliases...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM IngredientAliases WHERE IngredientId = {0}", id);

                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting PriceHistories...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM PriceHistories WHERE IngredientId = {0}", id);

                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] All child entities deleted");

                // Clear tracker again after raw SQL operations
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Clearing change tracker again...");
                _context.ChangeTracker.Clear();

                // Now delete the ingredient itself
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deleting ingredient entity...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Ingredients WHERE Id = {0}", id);
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Ingredient deleted");

                // Track modification for delta sync
                if (_modificationService != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Tracking deletion in LocalModificationService...");
                    await _modificationService.TrackDeletionAsync("Ingredient", id, locationId, ingredientName);
                    System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Deletion tracked");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] ModificationService is null, skipping tracking");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] Ingredient not found");
            }

            System.Diagnostics.Debug.WriteLine($"        [INGREDIENT REPO] DeleteAsync complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("        ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("        ║ [INGREDIENT REPO EXCEPTION]                       ║");
            System.Diagnostics.Debug.WriteLine("        ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"        Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"        Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"        Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"        Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"        Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("        ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task<List<Ingredient>> SearchAsync(string searchTerm, Guid locationId)
    {
        var query = _context.Ingredients
            .Include(i => i.Aliases)  // NEW: Include aliases for search
            .Where(i => i.LocationId == locationId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i =>
                i.Name.Contains(searchTerm) ||
                (i.Category != null && i.Category.Contains(searchTerm)) ||
                (i.VendorName != null && i.VendorName.Contains(searchTerm)) ||
                (i.VendorSku != null && i.VendorSku.Contains(searchTerm)) ||
                // NEW: Search in aliases
                (i.Aliases != null && i.Aliases.Any(a => a.AliasName.Contains(searchTerm)))
            );
        }

        return await query.OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Ingredients.AnyAsync(i => i.Id == id);
    }

    public async Task<Ingredient?> GetByNameAsync(string name, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Ingredients
            .AsNoTracking()
            .Include(i => i.Aliases)
            .Include(i => i.PriceHistory)
            .Include(i => i.IngredientAllergens)
                .ThenInclude(ia => ia.Allergen)
            .FirstOrDefaultAsync(i => i.Name == name && i.LocationId == locationId);
    }
}