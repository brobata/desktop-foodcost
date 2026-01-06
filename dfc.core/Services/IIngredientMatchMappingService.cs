using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Service for managing ingredient match mappings
/// Provides business logic layer for remembering user-defined ingredient matches
/// </summary>
public interface IIngredientMatchMappingService
{
    /// <summary>
    /// Get mapping for a specific import name and location
    /// Returns null if no mapping exists
    /// </summary>
    Task<IngredientMatchMapping?> GetMappingForNameAsync(string importName, Guid locationId);

    /// <summary>
    /// Get all mappings for a location
    /// </summary>
    Task<List<IngredientMatchMapping>> GetAllMappingsForLocationAsync(Guid locationId);

    /// <summary>
    /// Save a new mapping between an import name and an ingredient
    /// </summary>
    Task<IngredientMatchMapping> SaveIngredientMappingAsync(string importName, Guid ingredientId, Guid locationId);

    /// <summary>
    /// Save a new mapping between an import name and a recipe
    /// </summary>
    Task<IngredientMatchMapping> SaveRecipeMappingAsync(string importName, Guid recipeId, Guid locationId);

    /// <summary>
    /// Update an existing mapping to point to a different ingredient
    /// </summary>
    Task<IngredientMatchMapping> UpdateToIngredientAsync(Guid mappingId, Guid ingredientId);

    /// <summary>
    /// Update an existing mapping to point to a different recipe
    /// </summary>
    Task<IngredientMatchMapping> UpdateToRecipeAsync(Guid mappingId, Guid recipeId);

    /// <summary>
    /// Delete a mapping
    /// </summary>
    Task DeleteMappingAsync(Guid mappingId);

    /// <summary>
    /// Check if a mapping exists for a given import name and location
    /// </summary>
    Task<bool> MappingExistsAsync(string importName, Guid locationId);
}
