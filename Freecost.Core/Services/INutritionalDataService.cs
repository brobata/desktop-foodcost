using Freecost.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

/// <summary>
/// Service for looking up nutritional data from public APIs (USDA FoodData Central)
/// </summary>
public interface INutritionalDataService
{
    /// <summary>
    /// Searches for nutritional data matches for an ingredient name
    /// </summary>
    /// <param name="ingredientName">Name of ingredient to search for</param>
    /// <param name="maxResults">Maximum number of results to return (default 3)</param>
    /// <returns>List of potential matches with nutritional data</returns>
    Task<List<NutritionalDataResult>> SearchNutritionalDataAsync(string ingredientName, int maxResults = 3);

    /// <summary>
    /// Gets detailed nutritional data for a specific FDC ID
    /// </summary>
    /// <param name="fdcId">USDA FoodData Central ID</param>
    /// <returns>Detailed nutritional data</returns>
    Task<NutritionalDataResult?> GetNutritionalDataByIdAsync(string fdcId);

    /// <summary>
    /// Checks if the API is available and configured
    /// </summary>
    Task<bool> IsAvailableAsync();


    /// <summary>
    /// Extracts ingredient conversions from USDA nutritional data.
    /// Uses serving size information to create unit conversions (e.g., "1 medium = 182g").
    /// </summary>
    /// <param name="ingredientName">Name of ingredient to search for</param>
    /// <param name="ingredientId">Optional ingredient ID to associate conversions with</param>
    /// <param name="locationId">Optional location ID for location-specific conversions</param>
    /// <returns>List of ingredient conversions extracted from USDA data</returns>
    Task<List<IngredientConversion>> ExtractConversionsAsync(string ingredientName, Guid? ingredientId = null, Guid? locationId = null);
}
