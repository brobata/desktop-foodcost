using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Data.LocalDatabase;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Data.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly DfcDbContext _context;

    public LocationRepository(DfcDbContext context)
    {
        _context = context;
    }

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"      [LOCATION REPO] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("      ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("      ║ [LOCATION REPOSITORY ERROR]                       ║");
        System.Diagnostics.Debug.WriteLine("      ╠═══════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"      {message}");
        if (ex != null)
        {
            System.Diagnostics.Debug.WriteLine($"      Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"      Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"      Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"      Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"      Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        System.Diagnostics.Debug.WriteLine("      ╚═══════════════════════════════════════════════════╝");
    }

    public async Task<List<Location>> GetAllAsync()
    {
        try
        {
            LogDebug("Starting GetAllAsync");
            // CRITICAL: Use AsNoTracking() to prevent EF from tracking these entities
            // These locations are used for display/selection and stored in CurrentLocationService
            // If tracked, saving new ingredients/recipes causes constraint violations
            var locations = await _context.Locations
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .ToListAsync();
            LogDebug($"✓ Retrieved {locations.Count} locations");
            return locations;
        }
        catch (Exception ex)
        {
            LogError("Failed to get all locations", ex);
            throw;
        }
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        try
        {
            LogDebug($"Starting GetByIdAsync for location ID: {id}");
            // PERFORMANCE FIX: Don't load related entities (causes UI freezes with thousands of records)
            // UpdateAsync clears these collections anyway, so loading them is pointless
            var location = await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location != null)
            {
                LogDebug($"✓ Found location: {location.Name}");
            }
            else
            {
                LogDebug($"⚠ Location not found for ID: {id}");
            }
            return location;
        }
        catch (Exception ex)
        {
            LogError($"Failed to get location by ID: {id}", ex);
            throw;
        }
    }

    public async Task<Location> AddAsync(Location location)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting AddAsync                                 ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Location Name: {location.Name}");
            LogDebug($"Location ID: {location.Id}");
            LogDebug($"User ID: {location.UserId ?? "(none)"}");

            // CRITICAL: Clear navigation collections to prevent EF tracking issues
            // This is essential when syncing from Firebase where navigation properties may be populated
            LogDebug("Clearing navigation collections...");
            location.Ingredients = new List<Ingredient>();
            location.Recipes = new List<Recipe>();
            location.Entrees = new List<Entree>();
            location.Users = new List<User>();

            LogDebug("Adding location to context...");
            await _context.Locations.AddAsync(location);

            LogDebug("Saving changes...");
            await _context.SaveChangesAsync();
            LogDebug("✓ Location saved to database");

            // Reload to ensure proper EF tracking and collection initialization
            LogDebug("Reloading location from database...");
            var reloaded = await GetByIdAsync(location.Id) ?? location;
            LogDebug($"✓ AddAsync complete for location: {location.Name}");
            LogDebug("╚═══════════════════════════════════════════════════╝");
            return reloaded;
        }
        catch (Exception ex)
        {
            LogError($"Failed to add location '{location.Name}'", ex);
            throw;
        }
    }

    public async Task<Location> UpdateAsync(Location location)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting UpdateAsync                              ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Location Name: {location.Name}");
            LogDebug($"Location ID: {location.Id}");

            // CRITICAL: Clear navigation collections to prevent EF tracking issues
            LogDebug("Clearing navigation collections...");
            location.Ingredients = new List<Ingredient>();
            location.Recipes = new List<Recipe>();
            location.Entrees = new List<Entree>();
            location.Users = new List<User>();

            LogDebug("Updating location in context...");
            _context.Locations.Update(location);

            LogDebug("Saving changes...");
            await _context.SaveChangesAsync();
            LogDebug($"✓ UpdateAsync complete for location: {location.Name}");
            LogDebug("╚═══════════════════════════════════════════════════╝");
            return location;
        }
        catch (Exception ex)
        {
            LogError($"Failed to update location '{location.Name}' (ID: {location.Id})", ex);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting DeleteAsync                              ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"Location ID: {id}");

            LogDebug("Finding location...");
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                LogDebug($"Found location: {location.Name}");
                LogDebug("Removing location from context...");
                _context.Locations.Remove(location);

                LogDebug("Saving changes...");
                await _context.SaveChangesAsync();
                LogDebug($"✓ DeleteAsync complete - Location deleted: {location.Name}");
            }
            else
            {
                LogDebug($"⚠ Location not found for ID: {id} - Nothing to delete");
            }
            LogDebug("╚═══════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            LogError($"Failed to delete location ID: {id}", ex);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            LogDebug($"Checking if location exists: {id}");
            var exists = await _context.Locations.AnyAsync(l => l.Id == id);
            LogDebug($"Location {id} exists: {exists}");
            return exists;
        }
        catch (Exception ex)
        {
            LogError($"Failed to check if location exists: {id}", ex);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            LogDebug("Saving changes...");
            var changes = await _context.SaveChangesAsync();
            LogDebug($"✓ Saved {changes} changes");
            return changes;
        }
        catch (Exception ex)
        {
            LogError("Failed to save changes", ex);
            throw;
        }
    }
}
