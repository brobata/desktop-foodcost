using Dfc.Core.Models;
using Dfc.Core.Enums;
using Dfc.Data.LocalDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Data;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with sample data for the default offline location (if it exists)
    /// This method is called automatically on app startup
    /// IMPORTANT: Only seeds OFFLINE locations (UserId == null) to prevent polluting synced data
    /// </summary>
    public static void SeedDatabase(DfcDbContext context)
    {
        var logFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs", $"startup_log_{DateTime.Now:yyyyMMdd}.txt");
        System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] SeedDatabase called\n");

        var defaultLocationId = new Guid("00000000-0000-0000-0000-000000000001");

        // Create default location if it doesn't exist
        var defaultLocation = context.Locations.FirstOrDefault(l => l.Id == defaultLocationId);
        if (defaultLocation == null)
        {
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] Creating default location\n");
            defaultLocation = new Location
            {
                Id = defaultLocationId,
                Name = "Default Location",
                UserId = null // Explicitly set as offline location
            };
            context.Locations.Add(defaultLocation);
            context.SaveChanges();
        }
        else
        {
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] Default location already exists: {defaultLocation.Name}, UserId={defaultLocation.UserId}\n");
        }

        // CRITICAL: Only seed if this is an OFFLINE location (UserId == null)
        // Never seed online locations that sync with Firebase
        if (defaultLocation.UserId != null)
        {
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] Skipping seed - ONLINE location (UserId={defaultLocation.UserId})\n");
            System.Diagnostics.Debug.WriteLine($"[DatabaseSeeder] Skipping seed for location '{defaultLocation.Name}' - this is an ONLINE location (synced with Firebase)");
            return; // This is an online location - don't seed it
        }

        // Count existing ingredients for this location
        var existingCount = context.Ingredients.Count(i => i.LocationId == defaultLocationId);
        System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] Existing ingredients for location {defaultLocationId}: {existingCount}\n");

        // Only seed if the default location has no data
        if (!context.Ingredients.Any(i => i.LocationId == defaultLocationId))
        {
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] No data found - SEEDING location '{defaultLocation.Name}'\n");
            System.Diagnostics.Debug.WriteLine($"[DatabaseSeeder] Seeding sample data for OFFLINE location '{defaultLocation.Name}'");
            SeedLocationData(context, defaultLocationId);
        }
        else
        {
            System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] [SEEDER] Data already exists - SKIPPING seed\n");
            System.Diagnostics.Debug.WriteLine($"[DatabaseSeeder] Location '{defaultLocation.Name}' already has data - skipping seed");
        }
    }

    /// <summary>
    /// Seeds sample data for a specific location
    /// Can be called manually to populate any location with sample ingredients, recipes, and entrees
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="locationId">ID of the location to seed</param>
    public static void SeedLocationData(DfcDbContext context, Guid locationId)
    {
        // Verify location exists
        if (!context.Locations.Any(l => l.Id == locationId))
        {
            throw new ArgumentException($"Location with ID {locationId} does not exist", nameof(locationId));
        }

        // Check if this location already has data
        if (context.Ingredients.Any(i => i.LocationId == locationId))
        {
            // Location already has data, skip seeding
            return;
        }

        Ingredient? chicken = null;
        var ingredients = new List<Ingredient>
        {
            new Ingredient
            {
                Name = "Onions, Yellow",
                Category = "Produce",
                CurrentPrice = 2.49m,
                Unit = UnitType.Pound,
                VendorName = "Ex. Foods",
                VendorSku = "84729163",
                LocationId = locationId,
                // Nutritional data per 100g (USDA)
                CaloriesPerUnit = 40,
                ProteinPerUnit = 1.1m,
                CarbohydratesPerUnit = 9.3m,
                FatPerUnit = 0.1m,
                FiberPerUnit = 1.7m,
                SugarPerUnit = 4.2m,
                SodiumPerUnit = 4m,
                // Alias
                Aliases = new List<IngredientAlias>
                {
                    new IngredientAlias { AliasName = "Yellow Onion", IsPrimary = false }
                }
            },
            new Ingredient
            {
                Name = "Tomatoes, Roma",
                Category = "Produce",
                CurrentPrice = 3.99m,
                Unit = UnitType.Pound,
                VendorName = "Ex. Foods",
                VendorSku = "59283746",
                LocationId = locationId,
                // Nutritional data per 100g (USDA)
                CaloriesPerUnit = 18,
                ProteinPerUnit = 0.9m,
                CarbohydratesPerUnit = 3.9m,
                FatPerUnit = 0.2m,
                FiberPerUnit = 1.2m,
                SugarPerUnit = 2.6m,
                SodiumPerUnit = 5m,
                // Alias
                Aliases = new List<IngredientAlias>
                {
                    new IngredientAlias { AliasName = "Roma Tomato", IsPrimary = false }
                }
            },
            new Ingredient
            {
                Name = "Olive Oil, Extra Virgin",
                Category = "Pantry",
                CurrentPrice = 12.99m,
                Unit = UnitType.Liter,
                VendorName = "Ex. Foods",
                VendorSku = "31648257",
                LocationId = locationId,
                // Nutritional data per 100g (USDA)
                CaloriesPerUnit = 884,
                ProteinPerUnit = 0m,
                CarbohydratesPerUnit = 0m,
                FatPerUnit = 100m,
                FiberPerUnit = 0m,
                SugarPerUnit = 0m,
                SodiumPerUnit = 2m,
                // Alias
                Aliases = new List<IngredientAlias>
                {
                    new IngredientAlias { AliasName = "EVOO", IsPrimary = false }
                }
            },
            new Ingredient
            {
                Name = "Chicken Breast, Boneless",
                Category = "Protein",
                CurrentPrice = 4.99m,
                Unit = UnitType.Pound,
                VendorName = "Ex. Foods",
                VendorSku = "72914538",
                LocationId = locationId,
                // Nutritional data per 100g (USDA - raw)
                CaloriesPerUnit = 165,
                ProteinPerUnit = 31m,
                CarbohydratesPerUnit = 0m,
                FatPerUnit = 3.6m,
                FiberPerUnit = 0m,
                SugarPerUnit = 0m,
                SodiumPerUnit = 74m,
                // Alias
                Aliases = new List<IngredientAlias>
                {
                    new IngredientAlias { AliasName = "Chicken Breast", IsPrimary = false }
                }
            },
            new Ingredient
            {
                Name = "Flour, All Purpose",
                Category = "Baking",
                CurrentPrice = 18.99m,
                Unit = UnitType.Pound,
                VendorName = "Ex. Foods",
                VendorSku = "46158392",
                LocationId = locationId,
                // Nutritional data per 100g (USDA)
                CaloriesPerUnit = 364,
                ProteinPerUnit = 10.3m,
                CarbohydratesPerUnit = 76.3m,
                FatPerUnit = 1m,
                FiberPerUnit = 2.7m,
                SugarPerUnit = 0.3m,
                SodiumPerUnit = 2m,
                // Alias
                Aliases = new List<IngredientAlias>
                {
                    new IngredientAlias { AliasName = "AP Flour", IsPrimary = false }
                }
            }
        };
        context.Ingredients.AddRange(ingredients);
        context.SaveChanges();
        chicken = ingredients.First(i => i.Name == "Chicken Breast, Boneless");

        Recipe? gorgonzolaSauce = null;
        var recipes = new List<Recipe>
        {
            new Recipe { Name = "House Salad Dressing", Description = "Light vinaigrette for house salads", Yield = 32, YieldUnit = "oz", LocationId = locationId },
            new Recipe { Name = "Marinara Sauce", Description = "Classic tomato sauce", Yield = 1, YieldUnit = "gallon", LocationId = locationId },
            new Recipe { Name = "Gorgonzola Cream Sauce", Description = "Rich cream sauce", Yield = 7.25m, YieldUnit = "cups", LocationId = locationId }
        };
        context.Recipes.AddRange(recipes);
        context.SaveChanges();
        gorgonzolaSauce = recipes.First(r => r.Name == "Gorgonzola Cream Sauce");

        var entrees = new List<Entree>
        {
            new Entree
            {
                Name = "Chicken Gorgonzola",
                Description = "Grilled chicken breast with our signature gorgonzola cream sauce.",
                MenuPrice = 24.99m,
                LocationId = locationId,
                EntreeRecipes = new List<EntreeRecipe>
                {
                    // Add 1 serving (by quantity) of the Gorgonzola Cream Sauce recipe
                    new EntreeRecipe { RecipeId = gorgonzolaSauce.Id, Quantity = 1, Unit = UnitType.Each }
                },
                EntreeIngredients = new List<EntreeIngredient>
                {
                    // Add 1 piece (8oz) of chicken breast directly
                    new EntreeIngredient { IngredientId = chicken.Id, Quantity = 8, Unit = UnitType.Ounce }
                }
            }
        };
        context.Entrees.AddRange(entrees);
        context.SaveChanges();
    }
}