using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Freecost.Core.Enums;
using Freecost.Core.Models;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Service for managing global configuration
/// Simplified version using hardcoded fallbacks (Firebase removed)
/// </summary>
public interface IGlobalConfigService
{
    /// <summary>
    /// Load all global configuration (now uses hardcoded fallbacks)
    /// </summary>
    Task<bool> LoadAllConfigAsync(string? authToken = null);

    /// <summary>
    /// Get ingredient/recipe name mapping
    /// </summary>
    GlobalIngredientMapping? GetIngredientMapping(string importName);

    /// <summary>
    /// Get all vendor import maps
    /// </summary>
    IReadOnlyList<GlobalVendorMap> GetVendorMaps();

    /// <summary>
    /// Get unit mapping for a unit text
    /// </summary>
    string? GetUnitMapping(string unitText);

    /// <summary>
    /// Get all allergen keywords for a specific allergen type
    /// </summary>
    List<string> GetAllergenKeywords(AllergenType allergenType);

    /// <summary>
    /// Get all allergen keywords grouped by type
    /// </summary>
    Dictionary<AllergenType, List<string>> GetAllAllergenKeywords();

    /// <summary>
    /// Check if global configuration has been loaded
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// When the configuration was last loaded
    /// </summary>
    DateTime? LastLoadTime { get; }

    /// <summary>
    /// Seed Firebase (REMOVED - stub for compatibility)
    /// </summary>
    Task<bool> SeedFirebaseIfEmptyAsync(string? authToken = null);
}

public class GlobalConfigService : IGlobalConfigService
{
    private readonly ILogger<GlobalConfigService>? _logger;

    // Caches
    private Dictionary<string, GlobalIngredientMapping> _ingredientMappingCache = new(StringComparer.OrdinalIgnoreCase);
    private List<GlobalVendorMap> _vendorMaps = new();
    private Dictionary<string, string> _unitMappings = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<AllergenType, List<string>> _allergenKeywords = new();

    private bool _isLoaded = false;
    private DateTime? _lastLoadTime = null;

    public bool IsLoaded => _isLoaded;
    public DateTime? LastLoadTime => _lastLoadTime;

    public GlobalConfigService(ILogger<GlobalConfigService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Load hardcoded configuration (Firebase removed)
    /// </summary>
    public async Task<bool> LoadAllConfigAsync(string? authToken = null)
    {
        await Task.CompletedTask; // Make async for interface compatibility

        Debug.WriteLine("[GlobalConfigService] Loading hardcoded global configuration...");
        _logger?.LogInformation("Loading hardcoded global configuration");

        LoadHardcodedAllergenKeywords();
        LoadHardcodedUnitMappings();

        _isLoaded = true;
        _lastLoadTime = DateTime.UtcNow;

        Debug.WriteLine("[GlobalConfigService] Global configuration loaded successfully");
        return true;
    }

    /// <summary>
    /// Firebase seeding removed - stub for compatibility
    /// </summary>
    public async Task<bool> SeedFirebaseIfEmptyAsync(string? authToken = null)
    {
        await Task.CompletedTask;
        Debug.WriteLine("[GlobalConfigService] Firebase seeding skipped - using hardcoded config");
        return false; // Indicate nothing was seeded
    }

    public GlobalIngredientMapping? GetIngredientMapping(string importName)
    {
        _ingredientMappingCache.TryGetValue(importName, out var mapping);
        return mapping;
    }

    public IReadOnlyList<GlobalVendorMap> GetVendorMaps()
    {
        return _vendorMaps.AsReadOnly();
    }

    public string? GetUnitMapping(string unitText)
    {
        _unitMappings.TryGetValue(unitText, out var mapping);
        return mapping;
    }

    public List<string> GetAllergenKeywords(AllergenType allergenType)
    {
        if (_allergenKeywords.TryGetValue(allergenType, out var keywords))
        {
            return new List<string>(keywords);
        }
        return new List<string>();
    }

    public Dictionary<AllergenType, List<string>> GetAllAllergenKeywords()
    {
        return new Dictionary<AllergenType, List<string>>(_allergenKeywords);
    }

    /// <summary>
    /// Load hardcoded allergen keywords
    /// </summary>
    private void LoadHardcodedAllergenKeywords()
    {
        _allergenKeywords = new Dictionary<AllergenType, List<string>>
        {
            [AllergenType.Milk] = new List<string> { "milk", "dairy", "cream", "butter", "cheese", "yogurt", "whey", "casein", "lactose" },
            [AllergenType.Eggs] = new List<string> { "egg", "eggs", "albumin", "mayo", "mayonnaise" },
            [AllergenType.Fish] = new List<string> { "fish", "salmon", "tuna", "cod", "anchovy", "bass", "catfish", "halibut" },
            [AllergenType.Shellfish] = new List<string> { "shellfish", "shrimp", "crab", "lobster", "clam", "oyster", "mussel", "scallop" },
            [AllergenType.TreeNuts] = new List<string> { "almond", "cashew", "walnut", "pecan", "pistachio", "hazelnut", "macadamia", "nut" },
            [AllergenType.Peanuts] = new List<string> { "peanut", "peanuts", "peanut butter" },
            [AllergenType.Wheat] = new List<string> { "wheat", "flour", "bread", "pasta", "gluten" },
            [AllergenType.Soybeans] = new List<string> { "soy", "soybean", "tofu", "tempeh", "edamame", "miso" }
        };
    }

    /// <summary>
    /// Load hardcoded unit mappings
    /// </summary>
    private void LoadHardcodedUnitMappings()
    {
        _unitMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Weight
            ["lb"] = "Pounds",
            ["lbs"] = "Pounds",
            ["pound"] = "Pounds",
            ["pounds"] = "Pounds",
            ["oz"] = "Ounces",
            ["ounce"] = "Ounces",
            ["ounces"] = "Ounces",
            ["g"] = "Grams",
            ["gram"] = "Grams",
            ["grams"] = "Grams",
            ["kg"] = "Kilograms",
            ["kilogram"] = "Kilograms",
            ["kilograms"] = "Kilograms",

            // Volume
            ["gal"] = "Gallons",
            ["gallon"] = "Gallons",
            ["gallons"] = "Gallons",
            ["qt"] = "Quarts",
            ["quart"] = "Quarts",
            ["quarts"] = "Quarts",
            ["pt"] = "Pints",
            ["pint"] = "Pints",
            ["pints"] = "Pints",
            ["cup"] = "Cups",
            ["cups"] = "Cups",
            ["fl oz"] = "FluidOunces",
            ["floz"] = "FluidOunces",
            ["fluid ounce"] = "FluidOunces",
            ["fluid ounces"] = "FluidOunces",
            ["tbsp"] = "Tablespoons",
            ["tablespoon"] = "Tablespoons",
            ["tablespoons"] = "Tablespoons",
            ["tsp"] = "Teaspoons",
            ["teaspoon"] = "Teaspoons",
            ["teaspoons"] = "Teaspoons",
            ["ml"] = "Milliliters",
            ["milliliter"] = "Milliliters",
            ["milliliters"] = "Milliliters",
            ["l"] = "Liters",
            ["liter"] = "Liters",
            ["liters"] = "Liters",

            // Count
            ["each"] = "Each",
            ["ea"] = "Each",
            ["piece"] = "Each",
            ["pieces"] = "Each",
            ["item"] = "Each",
            ["items"] = "Each"
        };
    }
}
