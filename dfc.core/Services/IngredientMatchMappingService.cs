using Dfc.Core.Models;
using Dfc.Core.Repositories;

namespace Dfc.Core.Services;

public class IngredientMatchMappingService : IIngredientMatchMappingService
{
    private readonly IIngredientMatchMappingRepository _repository;

    public IngredientMatchMappingService(IIngredientMatchMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IngredientMatchMapping?> GetMappingForNameAsync(string importName, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            return null;

        return await _repository.GetByImportNameAsync(importName, locationId);
    }

    public async Task<List<IngredientMatchMapping>> GetAllMappingsForLocationAsync(Guid locationId)
    {
        return await _repository.GetAllByLocationAsync(locationId);
    }

    public async Task<IngredientMatchMapping> SaveIngredientMappingAsync(string importName, Guid ingredientId, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            throw new ArgumentException("Import name cannot be empty", nameof(importName));

        if (ingredientId == Guid.Empty)
            throw new ArgumentException("Ingredient ID cannot be empty", nameof(ingredientId));

        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));

        // Check if mapping already exists
        var existingMapping = await _repository.GetByImportNameAsync(importName, locationId);

        if (existingMapping != null)
        {
            // Update existing mapping to point to the new ingredient
            existingMapping.MatchedIngredientId = ingredientId;
            existingMapping.MatchedRecipeId = null; // Clear recipe mapping
            return await _repository.UpdateAsync(existingMapping);
        }

        // Create new mapping
        var mapping = new IngredientMatchMapping
        {
            ImportName = importName,
            MatchedIngredientId = ingredientId,
            MatchedRecipeId = null,
            LocationId = locationId
        };

        return await _repository.AddAsync(mapping);
    }

    public async Task<IngredientMatchMapping> SaveRecipeMappingAsync(string importName, Guid recipeId, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            throw new ArgumentException("Import name cannot be empty", nameof(importName));

        if (recipeId == Guid.Empty)
            throw new ArgumentException("Recipe ID cannot be empty", nameof(recipeId));

        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));

        // Check if mapping already exists
        var existingMapping = await _repository.GetByImportNameAsync(importName, locationId);

        if (existingMapping != null)
        {
            // Update existing mapping to point to the new recipe
            existingMapping.MatchedIngredientId = null; // Clear ingredient mapping
            existingMapping.MatchedRecipeId = recipeId;
            return await _repository.UpdateAsync(existingMapping);
        }

        // Create new mapping
        var mapping = new IngredientMatchMapping
        {
            ImportName = importName,
            MatchedIngredientId = null,
            MatchedRecipeId = recipeId,
            LocationId = locationId
        };

        return await _repository.AddAsync(mapping);
    }

    public async Task<IngredientMatchMapping> UpdateToIngredientAsync(Guid mappingId, Guid ingredientId)
    {
        if (mappingId == Guid.Empty)
            throw new ArgumentException("Mapping ID cannot be empty", nameof(mappingId));

        if (ingredientId == Guid.Empty)
            throw new ArgumentException("Ingredient ID cannot be empty", nameof(ingredientId));

        var mapping = await _repository.GetByIdAsync(mappingId);

        if (mapping == null)
            throw new InvalidOperationException($"Mapping with ID {mappingId} not found");

        mapping.MatchedIngredientId = ingredientId;
        mapping.MatchedRecipeId = null; // Clear recipe mapping

        return await _repository.UpdateAsync(mapping);
    }

    public async Task<IngredientMatchMapping> UpdateToRecipeAsync(Guid mappingId, Guid recipeId)
    {
        if (mappingId == Guid.Empty)
            throw new ArgumentException("Mapping ID cannot be empty", nameof(mappingId));

        if (recipeId == Guid.Empty)
            throw new ArgumentException("Recipe ID cannot be empty", nameof(recipeId));

        var mapping = await _repository.GetByIdAsync(mappingId);

        if (mapping == null)
            throw new InvalidOperationException($"Mapping with ID {mappingId} not found");

        mapping.MatchedIngredientId = null; // Clear ingredient mapping
        mapping.MatchedRecipeId = recipeId;

        return await _repository.UpdateAsync(mapping);
    }

    public async Task DeleteMappingAsync(Guid mappingId)
    {
        if (mappingId == Guid.Empty)
            throw new ArgumentException("Mapping ID cannot be empty", nameof(mappingId));

        await _repository.DeleteAsync(mappingId);
    }

    public async Task<bool> MappingExistsAsync(string importName, Guid locationId)
    {
        if (string.IsNullOrWhiteSpace(importName))
            return false;

        return await _repository.ExistsByImportNameAsync(importName, locationId);
    }
}
