using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly DfcDbContext _context;
    private readonly ILocalModificationService? _modificationService;

    public RecipeRepository(DfcDbContext context, ILocalModificationService? modificationService = null)
    {
        _context = context;
        _modificationService = modificationService;
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync(Guid locationId)
    {
        // CRITICAL: Use AsNoTracking() for display queries
        // These entities are shown in UI grids and must not be tracked
        // Tracked entities cause constraint violations when adding/deleting
        return await _context.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .Include(r => r.RecipeRecipes)
                .ThenInclude(rr => rr.ComponentRecipe)
            .Where(r => r.LocationId == locationId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
    {
        // CRITICAL: Use AsNoTracking() to prevent EF from tracking this entity
        // When this is used for deletion (fresh copy for recycle bin), tracked entities
        // cause constraint violations when RecycleBinService clears navigation properties
        return await _context.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient!)
                    .ThenInclude(i => i.IngredientAllergens!)
                        .ThenInclude(ia => ia.Allergen)
            .Include(r => r.RecipeRecipes)
                .ThenInclude(rr => rr.ComponentRecipe)
            .Include(r => r.RecipeAllergens)
                .ThenInclude(ra => ra.Allergen)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string searchTerm, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllRecipesAsync(locationId);

        var term = searchTerm.ToLower();
        return await _context.Recipes
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .Include(r => r.RecipeRecipes)
                .ThenInclude(rr => rr.ComponentRecipe)
            .Where(r => r.LocationId == locationId &&
                       (r.Name.ToLower().Contains(term) ||
                        (r.Description != null && r.Description.ToLower().Contains(term))))
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ Starting CreateRecipeAsync                        ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe Name: {recipe.Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe ID: {recipe.Id}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Location ID: {recipe.LocationId}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] RecipeIngredients count: {recipe.RecipeIngredients?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] RecipeAllergens count: {recipe.RecipeAllergens?.Count ?? 0}");

            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            // from previous operations (shared DbContext across repositories)
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Clearing change tracker...");
            _context.ChangeTracker.Clear();
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Change tracker cleared");

            // CRITICAL: Only generate new ID if not set (preserve Supabase IDs)


            if (recipe.Id == Guid.Empty)


            {


                recipe.Id = Guid.NewGuid();


            }
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.ModifiedAt = DateTime.UtcNow;
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Generated new ID: {recipe.Id}");

            // Set RecipeId for all ingredients and generate IDs
            if (recipe.RecipeIngredients != null)
            {
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Processing {recipe.RecipeIngredients.Count} recipe ingredients...");
                int ingredientIndex = 0;
                foreach (var ingredient in recipe.RecipeIngredients)
                {
                    ingredient.Id = Guid.NewGuid();
                    ingredient.RecipeId = recipe.Id;
                    ingredient.CreatedAt = DateTime.UtcNow;
                    ingredient.ModifiedAt = DateTime.UtcNow;

                    // CRITICAL FIX: Convert Guid.Empty to null to prevent foreign key constraint violations
                    // SQLite will reject Guid.Empty as a foreign key value since it doesn't exist in Ingredients table
                    if (ingredient.IngredientId == Guid.Empty)
                    {
                        ingredient.IngredientId = null;
                        System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO]   Ingredient #{++ingredientIndex}: ID={ingredient.Id}, IngredientId=NULL (was Guid.Empty)");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO]   Ingredient #{++ingredientIndex}: ID={ingredient.Id}, IngredientId={ingredient.IngredientId}");
                    }

                    // CRITICAL: Clear navigation properties to prevent EF from tracking/updating related entities
                    ingredient.Recipe = null!;
                    ingredient.Ingredient = null!;
                }
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] All recipe ingredients processed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] No recipe ingredients to process");
            }

            // Set RecipeId for all allergens and generate IDs
            if (recipe.RecipeAllergens != null)
            {
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Processing {recipe.RecipeAllergens.Count} recipe allergens...");
                foreach (var allergen in recipe.RecipeAllergens)
                {
                    allergen.Id = Guid.NewGuid();
                    allergen.RecipeId = recipe.Id;
                    allergen.CreatedAt = DateTime.UtcNow;
                    allergen.ModifiedAt = DateTime.UtcNow;

                    // CRITICAL: Clear child navigation properties
                    allergen.Recipe = null!;
                    allergen.Allergen = null!;
                }
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] All recipe allergens processed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] No recipe allergens to process");
            }

            // CRITICAL: Clear navigation properties before Add to avoid EF foreign key constraint violations
            // This is essential when syncing from Firebase where navigation properties may be populated
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Clearing navigation properties...");
            recipe.Location = null!;
            recipe.EntreeRecipes = new List<EntreeRecipe>();
            recipe.Photos = new List<Photo>();
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Navigation properties cleared");

            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Adding recipe to DbSet...");
            _context.Recipes.Add(recipe);
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Calling SaveChangesAsync...");
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] SaveChangesAsync complete");

            // Track modification for delta sync
            if (_modificationService != null)
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Tracking creation in LocalModificationService...");
                await _modificationService.TrackCreationAsync("Recipe", recipe.Id, recipe.LocationId);
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Creation tracked");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ModificationService is null, skipping tracking");
            }

            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ✓ Recipe created successfully");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");

            return recipe;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ [EXCEPTION IN CreateRecipeAsync]                  ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe Name: {recipe.Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ Starting UpdateRecipeAsync                        ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe Name: {recipe.Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe ID: {recipe.Id}");

            // Step 1: Delete existing child entities FIRST, outside transaction
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting existing RecipeIngredients...");
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM RecipeIngredients WHERE RecipeId = {0}", recipe.Id);

            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting existing RecipeAllergens...");
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM RecipeAllergens WHERE RecipeId = {0}", recipe.Id);

        // Step 2: Update recipe scalar properties
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE Recipes SET Name = {0}, Description = {1}, Instructions = {2},
              Yield = {3}, YieldUnit = {4}, PrepTimeMinutes = {5}, ModifiedAt = {6}
              WHERE Id = {7}",
            recipe.Name,
            recipe.Description ?? string.Empty,
            recipe.Instructions ?? string.Empty,
            recipe.Yield,
            recipe.YieldUnit,
            recipe.PrepTimeMinutes.HasValue ? (object)recipe.PrepTimeMinutes.Value : null!,
            DateTime.UtcNow,
            recipe.Id);

        // Step 3: Insert new ingredients
        if (recipe.RecipeIngredients != null)
        {
            foreach (var ri in recipe.RecipeIngredients)
            {
                // CRITICAL FIX: Convert Guid.Empty to null to prevent foreign key constraint violations
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type
#pragma warning disable CS8601 // Possible null reference assignment
                object ingredientId = (ri.IngredientId == Guid.Empty) ? DBNull.Value : (object)ri.IngredientId;

                var newId = Guid.NewGuid();
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO RecipeIngredients (Id, RecipeId, IngredientId, Quantity, Unit, DisplayText, IsOptional, SortOrder, CreatedAt, ModifiedAt)
                      VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})",
                    new object[]
                    {
                        newId,
                        recipe.Id,
                        ingredientId,
                        ri.Quantity,
                        (int)ri.Unit,
                        ri.DisplayText ?? string.Empty,
                        ri.IsOptional ? 1 : 0,
                        ri.SortOrder,
                        DateTime.UtcNow,
                        DateTime.UtcNow
                    });
#pragma warning restore CS8601 // Possible null reference assignment
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type
            }
        }

        // Step 4: Insert new allergens
        if (recipe.RecipeAllergens != null)
        {
            foreach (var ra in recipe.RecipeAllergens)
            {
                var newId = Guid.NewGuid();
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO RecipeAllergens (Id, RecipeId, AllergenId, IsAutoDetected, IsEnabled, SourceIngredients, CreatedAt, ModifiedAt)
                      VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                    newId,
                    recipe.Id,
                    ra.AllergenId,
                    ra.IsAutoDetected ? 1 : 0,
                    ra.IsEnabled ? 1 : 0,
                    ra.SourceIngredients ?? string.Empty,
                    DateTime.UtcNow,
                    DateTime.UtcNow);
            }
        }

            // CRITICAL: Clear EF's change tracker so it doesn't return stale cached entities
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Clearing change tracker...");
            _context.ChangeTracker.Clear();

            // Track modification for delta sync
            if (_modificationService != null)
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Tracking update...");
                await _modificationService.TrackUpdateAsync("Recipe", recipe.Id, recipe.LocationId);
            }

            // Reload with navigation properties
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Reloading recipe with navigation properties...");
            var reloaded = await GetRecipeByIdAsync(recipe.Id) ?? recipe;
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ✓ Recipe updated successfully");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");
            return reloaded;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ [EXCEPTION IN UpdateRecipeAsync]                  ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe Name: {recipe.Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task DeleteRecipeAsync(Guid id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ Starting DeleteRecipeAsync                        ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe ID: {id}");

            // CRITICAL: Clear change tracker FIRST to remove any stale tracked entities
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Clearing change tracker...");
            _context.ChangeTracker.Clear();

            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Finding recipe...");
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                var locationId = recipe.LocationId;
                var recipeName = recipe.Name; // CRITICAL: Store name before deletion for modification tracking
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Found recipe: '{recipeName}', LocationId: {locationId}");

                // CRITICAL: Manually delete all child entities using raw SQL to avoid EF tracking issues
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting RecipeIngredients...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RecipeIngredients WHERE RecipeId = {0}", id);

                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting RecipeAllergens...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RecipeAllergens WHERE RecipeId = {0}", id);

                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting EntreeRecipes...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM EntreeRecipes WHERE RecipeId = {0}", id);

                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting Photos...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Photos WHERE RecipeId = {0}", id);

                // Clear tracker again after raw SQL operations
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Clearing change tracker again...");
                _context.ChangeTracker.Clear();

                // Now delete the recipe itself
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Deleting recipe entity...");
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Recipes WHERE Id = {0}", id);

                // Track modification for delta sync
                if (_modificationService != null)
                {
                    System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Tracking deletion...");
                    await _modificationService.TrackDeletionAsync("Recipe", id, locationId, recipeName);
                }

                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ✓ Recipe deleted successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] Recipe not found");
            }

            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ║ [EXCEPTION IN DeleteRecipeAsync]                  ║");
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Recipe ID: {id}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"      [RECIPE REPO] Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("      [RECIPE REPO] ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task<bool> RecipeExistsAsync(string name, Guid locationId, Guid? excludeId = null)
    {
        var query = _context.Recipes
            .Where(r => r.LocationId == locationId && r.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Recipe?> GetByNameAsync(string name, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient!)
                    .ThenInclude(i => i.IngredientAllergens!)
                        .ThenInclude(ia => ia.Allergen)
            .Include(r => r.RecipeAllergens)
                .ThenInclude(ra => ra.Allergen)
            .FirstOrDefaultAsync(r => r.Name == name && r.LocationId == locationId);
    }
}