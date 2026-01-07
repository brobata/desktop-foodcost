using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Data.Repositories;

public class EntreeRepository : IEntreeRepository
{
    private readonly DfcDbContext _context;
    private readonly ILocalModificationService? _modificationService;

    public EntreeRepository(DfcDbContext context, ILocalModificationService? modificationService = null)
    {
        _context = context;
        _modificationService = modificationService;
    }

    public async Task<IEnumerable<Entree>> GetAllAsync(Guid locationId)
    {
        // CRITICAL: Use AsNoTracking() for display queries
        // These entities are shown in UI grids and must not be tracked
        // Tracked entities cause constraint violations when adding/deleting
        return await _context.Entrees
            .AsNoTracking()
            .Include(e => e.EntreeRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r!.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
            .Include(e => e.EntreeIngredients)
                .ThenInclude(ei => ei.Ingredient)
            .Where(e => e.LocationId == locationId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Entree?> GetByIdAsync(Guid id)
    {
        // CRITICAL: Use AsNoTracking() to prevent EF from tracking this entity
        // When this is used for deletion (fresh copy for recycle bin), tracked entities
        // cause constraint violations when RecycleBinService clears navigation properties
        return await _context.Entrees
            .AsNoTracking()
            .Include(e => e.EntreeRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r!.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient!)
                            .ThenInclude(i => i.IngredientAllergens!)
                                .ThenInclude(ia => ia.Allergen)
            .Include(e => e.EntreeRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r!.RecipeAllergens)
                        .ThenInclude(ra => ra.Allergen)
            .Include(e => e.EntreeIngredients)
                .ThenInclude(ei => ei.Ingredient!)
                    .ThenInclude(i => i.IngredientAllergens!)
                        .ThenInclude(ia => ia.Allergen)
            .Include(e => e.EntreeAllergens)
                .ThenInclude(ea => ea.Allergen)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Entree> CreateAsync(Entree entree)
    {
        // CRITICAL: Only generate new ID if not set

        if (entree.Id == Guid.Empty)

        {

            entree.Id = Guid.NewGuid();

        }
        entree.CreatedAt = DateTime.UtcNow;
        entree.ModifiedAt = DateTime.UtcNow;

        // Set IDs for all child entities
        if (entree.EntreeRecipes != null)
        {
            foreach (var entreeRecipe in entree.EntreeRecipes)
            {
                entreeRecipe.Id = Guid.NewGuid();
                entreeRecipe.EntreeId = entree.Id;
                entreeRecipe.CreatedAt = DateTime.UtcNow;
                entreeRecipe.ModifiedAt = DateTime.UtcNow;

                // Ensure DisplayName is not null (for Firebase sync compatibility)
                if (entreeRecipe.DisplayName == null)
                    entreeRecipe.DisplayName = string.Empty;

                // CRITICAL: Clear child navigation properties
                entreeRecipe.Entree = null!;
                entreeRecipe.Recipe = null!;
            }
        }

        if (entree.EntreeIngredients != null)
        {
            foreach (var entreeIngredient in entree.EntreeIngredients)
            {
                entreeIngredient.Id = Guid.NewGuid();
                entreeIngredient.EntreeId = entree.Id;
                entreeIngredient.CreatedAt = DateTime.UtcNow;
                entreeIngredient.ModifiedAt = DateTime.UtcNow;

                // Ensure DisplayName is not null (for Firebase sync compatibility)
                if (entreeIngredient.DisplayName == null)
                    entreeIngredient.DisplayName = string.Empty;

                // CRITICAL: Clear ALL child navigation properties to avoid EF confusion
                entreeIngredient.Entree = null!;
                entreeIngredient.Ingredient = null!;
            }
        }

        // CRITICAL: Clear navigation properties before Add to avoid EF foreign key constraint violations
        // This is essential when syncing from Firebase where navigation properties may be populated
        entree.Location = null!;
        entree.EntreeAllergens = new List<EntreeAllergen>();
        entree.Photos = new List<Photo>();

        _context.Entrees.Add(entree);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // If child entities fail due to FK constraints (Recipe/Ingredient doesn't exist locally),
            // retry without them. This can happen during sync when recipes/ingredients aren't synced yet.
            entree.EntreeRecipes = new List<EntreeRecipe>();
            entree.EntreeIngredients = new List<EntreeIngredient>();
            _context.ChangeTracker.Clear();
            _context.Entrees.Add(entree);
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            // Same handling for SQLite-specific FK constraint failures
            entree.EntreeRecipes = new List<EntreeRecipe>();
            entree.EntreeIngredients = new List<EntreeIngredient>();
            _context.ChangeTracker.Clear();
            _context.Entrees.Add(entree);
            await _context.SaveChangesAsync();
        }

        // Track modification for delta sync
        if (_modificationService != null)
        {
            await _modificationService.TrackCreationAsync("Entree", entree.Id, entree.LocationId);
        }

        // Reload with navigation properties
        return await GetByIdAsync(entree.Id) ?? entree;
    }

    public async Task<Entree> UpdateAsync(Entree entree)
    {
        // Step 1: Delete existing child entities FIRST, outside transaction
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM EntreeRecipes WHERE EntreeId = {0}", entree.Id);

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM EntreeIngredients WHERE EntreeId = {0}", entree.Id);

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM EntreeAllergens WHERE EntreeId = {0}", entree.Id);

        // Step 2: Update entree scalar properties - Include PhotoUrl and PlatingEquipment
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE Entrees SET Name = {0}, Description = {1}, MenuPrice = {2},
          PhotoUrl = {3}, PlatingEquipment = {4}, ModifiedAt = {5} WHERE Id = {6}",
            entree.Name,
            entree.Description ?? string.Empty,
            entree.MenuPrice ?? 0m,
            entree.PhotoUrl ?? string.Empty,
            entree.PlatingEquipment ?? string.Empty,
            DateTime.UtcNow,
            entree.Id);

        // Step 3: Insert new child entities (check for null after ClearNavigationProperties)
        if (entree.EntreeRecipes != null)
        {
            foreach (var er in entree.EntreeRecipes)
            {
                try
                {
                    var newId = Guid.NewGuid();
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO EntreeRecipes (Id, EntreeId, RecipeId, Quantity, Unit, SortOrder, DisplayName, CreatedAt, ModifiedAt)
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                        newId, entree.Id, er.RecipeId, er.Quantity, (int)er.Unit,
                        0, er.DisplayName ?? string.Empty, DateTime.UtcNow, DateTime.UtcNow);
                }
                catch (DbUpdateException)
                {
                    // Skip recipes with FK constraint failures (Recipe doesn't exist locally)
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    // Skip recipes with FK constraint failures (Recipe doesn't exist locally)
                }
            }
        }

        if (entree.EntreeIngredients != null)
        {
            foreach (var ei in entree.EntreeIngredients)
            {
                try
                {
                    var newId = Guid.NewGuid();
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO EntreeIngredients (Id, EntreeId, IngredientId, Quantity, Unit, DisplayName, CreatedAt, ModifiedAt)
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                        newId, entree.Id, ei.IngredientId, ei.Quantity,
                        (int)ei.Unit, ei.DisplayName ?? string.Empty, DateTime.UtcNow, DateTime.UtcNow);
                }
                catch (DbUpdateException)
                {
                    // Skip ingredients with FK constraint failures (Ingredient doesn't exist locally)
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    // Skip ingredients with FK constraint failures (Ingredient doesn't exist locally)
                }
            }
        }

        // CRITICAL: Clear EF's change tracker so it doesn't return stale cached entities
        _context.ChangeTracker.Clear();

        // Track modification for delta sync
        if (_modificationService != null)
        {
            await _modificationService.TrackUpdateAsync("Entree", entree.Id, entree.LocationId);
        }

        // Reload with navigation properties
        return await GetByIdAsync(entree.Id) ?? entree;
    }

    public async Task DeleteAsync(Guid id)
    {
        System.Diagnostics.Debug.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("║ [ENTREE DELETE] Starting DeleteAsync                          ║");
        System.Diagnostics.Debug.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"  Entree ID: {id}");

        // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
        _context.ChangeTracker.Clear();
        System.Diagnostics.Debug.WriteLine("  ✓ Change tracker cleared");

        var entree = await _context.Entrees.FindAsync(id);
        if (entree != null)
        {
            var locationId = entree.LocationId;
            var entreeName = entree.Name; // CRITICAL: Store name before deletion for modification tracking

            System.Diagnostics.Debug.WriteLine($"  Entree Found: '{entreeName}'");
            System.Diagnostics.Debug.WriteLine($"  Location ID: {locationId}");

            // CRITICAL: Manually delete all child entities using raw SQL to avoid EF tracking issues
            System.Diagnostics.Debug.WriteLine("  🗑️ Deleting child records...");
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM EntreeRecipes WHERE EntreeId = {0}", id);
            System.Diagnostics.Debug.WriteLine("     ✓ EntreeRecipes deleted");

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM EntreeIngredients WHERE EntreeId = {0}", id);
            System.Diagnostics.Debug.WriteLine("     ✓ EntreeIngredients deleted");

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM EntreeAllergens WHERE EntreeId = {0}", id);
            System.Diagnostics.Debug.WriteLine("     ✓ EntreeAllergens deleted");

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM Photos WHERE EntreeId = {0}", id);
            System.Diagnostics.Debug.WriteLine("     ✓ Photos deleted");

            // Clear tracker again after raw SQL operations
            _context.ChangeTracker.Clear();
            System.Diagnostics.Debug.WriteLine("  ✓ Change tracker cleared again");

            // Now delete the entree itself
            System.Diagnostics.Debug.WriteLine("  🗑️ Deleting entree record...");
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM Entrees WHERE Id = {0}", id);
            System.Diagnostics.Debug.WriteLine("     ✓ Entree deleted from database");

            // Track modification for delta sync
            if (_modificationService != null)
            {
                System.Diagnostics.Debug.WriteLine("  📝 Tracking deletion in LocalModifications...");
                await _modificationService.TrackDeletionAsync("Entree", id, locationId, entreeName);
                System.Diagnostics.Debug.WriteLine("     ✓ Deletion tracked successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("  ⚠️ WARNING: ModificationService is NULL - deletion NOT tracked!");
            }

            System.Diagnostics.Debug.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            System.Diagnostics.Debug.WriteLine($"✅ DELETE COMPLETE: '{entreeName}' removed from database and tracked for sync\n");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  ⚠️ WARNING: Entree {id} not found in database");
            System.Diagnostics.Debug.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");
        }
    }

    public async Task<Entree?> GetByNameAsync(string name, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Entrees
            .AsNoTracking()
            .Include(e => e.EntreeRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r!.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient!)
                            .ThenInclude(i => i.IngredientAllergens!)
                                .ThenInclude(ia => ia.Allergen)
            .Include(e => e.EntreeRecipes)
                .ThenInclude(er => er.Recipe)
                    .ThenInclude(r => r!.RecipeAllergens)
                        .ThenInclude(ra => ra.Allergen)
            .Include(e => e.EntreeIngredients)
                .ThenInclude(ei => ei.Ingredient!)
                    .ThenInclude(i => i.IngredientAllergens!)
                        .ThenInclude(ia => ia.Allergen)
            .Include(e => e.EntreeAllergens)
                .ThenInclude(ea => ea.Allergen)
            .FirstOrDefaultAsync(e => e.Name == name && e.LocationId == locationId);
    }
}