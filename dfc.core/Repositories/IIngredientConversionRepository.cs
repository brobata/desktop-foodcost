using Dfc.Core.Enums;
using Dfc.Core.Models;

namespace Dfc.Core.Repositories;

/// <summary>
/// Repository for managing ingredient unit conversions in the database.
/// </summary>
public interface IIngredientConversionRepository
{
    /// <summary>
    /// Gets a specific conversion for an ingredient from one unit to another.
    /// Checks ingredient-specific first, then location-specific, then global.
    /// </summary>
    Task<IngredientConversion?> GetConversionAsync(
        Guid ingredientId,
        UnitType fromUnit,
        UnitType toUnit,
        Guid? locationId = null);

    /// <summary>
    /// Gets all conversions for a specific ingredient.
    /// </summary>
    Task<List<IngredientConversion>> GetByIngredientIdAsync(Guid ingredientId);

    /// <summary>
    /// Gets all generic (non-ingredient-specific) conversions for a location.
    /// If locationId is null, returns global conversions.
    /// </summary>
    Task<List<IngredientConversion>> GetGenericConversionsAsync(Guid? locationId = null);

    /// <summary>
    /// Gets all conversions by source (e.g., "USDA", "BuiltIn", "UserDefined").
    /// </summary>
    Task<List<IngredientConversion>> GetBySourceAsync(string source);

    /// <summary>
    /// Gets all conversions for a specific location (including global).
    /// </summary>
    Task<List<IngredientConversion>> GetByLocationIdAsync(Guid locationId);

    /// <summary>
    /// Adds a new conversion to the database.
    /// </summary>
    Task AddAsync(IngredientConversion conversion);

    /// <summary>
    /// Adds multiple conversions in a batch.
    /// </summary>
    Task AddRangeAsync(List<IngredientConversion> conversions);

    /// <summary>
    /// Updates an existing conversion.
    /// </summary>
    Task UpdateAsync(IngredientConversion conversion);

    /// <summary>
    /// Deletes a conversion by ID.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Deletes all conversions for a specific ingredient.
    /// </summary>
    Task DeleteByIngredientIdAsync(Guid ingredientId);

    /// <summary>
    /// Deletes conversions by source (useful for refreshing USDA data).
    /// </summary>
    Task DeleteBySourceAsync(string source, Guid? ingredientId = null);

    /// <summary>
    /// Checks if a conversion already exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid? ingredientId, UnitType fromUnit, UnitType toUnit, Guid? locationId = null);

    /// <summary>
    /// Gets statistics about conversions in the database.
    /// </summary>
    Task<(int Total, int IngredientSpecific, int Generic, int USDA, int UserDefined)> GetStatisticsAsync();

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
