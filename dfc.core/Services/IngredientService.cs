using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Dfc.Core.Services;

public class IngredientService : IIngredientService
{
    private readonly IIngredientRepository _repository;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly ICacheService? _cacheService;
    private readonly ILogger<IngredientService>? _logger;
    private readonly INutritionalDataService? _nutritionalDataService;
    private readonly IIngredientConversionRepository? _conversionRepository;

    private const string CACHE_KEY_PREFIX = "ingredients";

    public IngredientService(
        IIngredientRepository repository,
        IPriceHistoryService priceHistoryService,
        ICacheService? cacheService = null,
        ILogger<IngredientService>? logger = null,
        INutritionalDataService? nutritionalDataService = null,
        IIngredientConversionRepository? conversionRepository = null)
    {
        _repository = repository;
        _priceHistoryService = priceHistoryService;
        _cacheService = cacheService;
        _logger = logger;
        _nutritionalDataService = nutritionalDataService;
        _conversionRepository = conversionRepository;
    }

    private void InvalidateCache(Guid locationId)
    {
        _cacheService?.Remove($"{CACHE_KEY_PREFIX}:{locationId}");
        _cacheService?.ClearPattern(CACHE_KEY_PREFIX);
    }

    public async Task<List<Ingredient>> GetAllIngredientsAsync(Guid locationId)
    {
        return await _repository.GetAllAsync(locationId);
    }

    public async Task<Ingredient?> GetIngredientByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Ingredient?> GetIngredientBySkuAsync(string sku, Guid locationId)
    {
        return await _repository.GetBySkuAsync(sku, locationId);
    }

    public async Task<Ingredient> CreateIngredientAsync(Ingredient ingredient)
    {
        if (string.IsNullOrWhiteSpace(ingredient.Name))
            throw new ArgumentException("Ingredient name is required");

        if (ingredient.CurrentPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        if (ingredient.LocationId == Guid.Empty)
            throw new ArgumentException("Location ID is required");

        var created = await _repository.AddAsync(ingredient);

        if (created.CurrentPrice > 0)
        {
            await _priceHistoryService.RecordPriceChangeAsync(created.Id, created.CurrentPrice);
        }

        // Auto-extract USDA conversions if enabled
        if (created.AutoConversionEnabled &&
            _nutritionalDataService != null &&
            _conversionRepository != null)
        {
            try
            {
                var conversions = await _nutritionalDataService.ExtractConversionsAsync(
                    created.Name,
                    created.Id,
                    created.LocationId);

                if (conversions.Any())
                {
                    await _conversionRepository.AddRangeAsync(conversions);
                    created.ConversionLastUpdated = DateTime.UtcNow;
                    created.ConversionSource = "USDA";
                    await _repository.UpdateAsync(created);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to auto-extract conversions for {IngredientName}", created.Name);
            }
        }

        InvalidateCache(created.LocationId);
        return created;
    }

    public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
    {
        if (string.IsNullOrWhiteSpace(ingredient.Name))
            throw new ArgumentException("Ingredient name is required");

        if (ingredient.CurrentPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        var existingIngredient = await _repository.GetByIdAsync(ingredient.Id);
        if (existingIngredient == null)
            throw new InvalidOperationException("Ingredient not found");

        var oldPrice = existingIngredient.CurrentPrice;

        try
        {
            var updated = await _repository.UpdateAsync(ingredient);

            if (oldPrice != updated.CurrentPrice)
            {
                await _priceHistoryService.RecordPriceChangeAsync(updated.Id, updated.CurrentPrice);
            }

            // Auto-extract USDA conversions if enabled and name changed
            var nameChanged = !string.Equals(existingIngredient.Name, updated.Name, StringComparison.OrdinalIgnoreCase);
            var conversionEnabledChanged = !existingIngredient.AutoConversionEnabled && updated.AutoConversionEnabled;

            if (updated.AutoConversionEnabled &&
                (nameChanged || conversionEnabledChanged) &&
                _nutritionalDataService != null &&
                _conversionRepository != null)
            {
                try
                {
                    var conversions = await _nutritionalDataService.ExtractConversionsAsync(
                        updated.Name,
                        updated.Id,
                        updated.LocationId);

                    if (conversions.Any())
                    {
                        var existingConversions = await _conversionRepository.GetByIngredientIdAsync(updated.Id);
                        foreach (var existing in existingConversions)
                        {
                            await _conversionRepository.DeleteAsync(existing.Id);
                        }

                        await _conversionRepository.AddRangeAsync(conversions);
                        updated.ConversionLastUpdated = DateTime.UtcNow;
                        updated.ConversionSource = "USDA";
                        await _repository.UpdateAsync(updated);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to re-extract conversions for {IngredientName}", updated.Name);
                }
            }

            InvalidateCache(updated.LocationId);
            return updated;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(
                "This ingredient was modified by another user. Please refresh and try again.",
                ingredient,
                ex);
        }
    }

    public async Task DeleteIngredientAsync(Guid id)
    {
        var ingredient = await _repository.GetByIdAsync(id);
        if (ingredient == null)
            throw new InvalidOperationException("Ingredient not found");

        var locationId = ingredient.LocationId;
        await _repository.DeleteAsync(id);
        InvalidateCache(locationId);
    }

    public async Task<List<Ingredient>> SearchIngredientsAsync(string searchTerm, Guid locationId)
    {
        return await _repository.SearchAsync(searchTerm, locationId);
    }
}
