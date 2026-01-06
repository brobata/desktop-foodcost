using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Freecost.Core.Services;

public class IngredientService : IIngredientService
{
    private readonly IIngredientRepository _repository;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly ICacheService? _cacheService;
    private readonly IUserSessionService? _sessionService;
    private readonly SupabaseDataService? _dataService;
    private readonly ILogger<IngredientService>? _logger;
    private readonly INutritionalDataService? _nutritionalDataService;
    private readonly IIngredientConversionRepository? _conversionRepository;


    // Cache key pattern: "ingredients:{locationId}"
    private const string CACHE_KEY_PREFIX = "ingredients";

    public IngredientService(
        IIngredientRepository repository,
        IPriceHistoryService priceHistoryService,
        ICacheService? cacheService = null,
        IUserSessionService? sessionService = null,
        SupabaseDataService? dataService = null,
        ILogger<IngredientService>? logger = null,
        INutritionalDataService? nutritionalDataService = null,
        IIngredientConversionRepository? conversionRepository = null)
    {
        _repository = repository;
        _priceHistoryService = priceHistoryService;
        _cacheService = cacheService;
        _sessionService = sessionService;
        _dataService = dataService;
        _logger = logger;
        _nutritionalDataService = nutritionalDataService;
        _conversionRepository = conversionRepository;
    }

    /// <summary>
    /// Invalidates ingredient list cache for a specific location
    /// </summary>
    private void InvalidateCache(Guid locationId)
    {
        _cacheService?.Remove($"{CACHE_KEY_PREFIX}:{locationId}");
        _cacheService?.ClearPattern(CACHE_KEY_PREFIX); // Clear all ingredient-related caches
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
        // Validation
        if (string.IsNullOrWhiteSpace(ingredient.Name))
            throw new ArgumentException("Ingredient name is required");

        if (ingredient.CurrentPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        // Business logic: Ensure LocationId is set
        if (ingredient.LocationId == Guid.Empty)
            throw new ArgumentException("Location ID is required");

        var created = await _repository.AddAsync(ingredient);

        // Record initial price in history
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
                Debug.WriteLine($"[IngredientService] Auto-extracting conversions for '{created.Name}' from USDA");
                SyncDebugLogger.WriteInfo($"Auto-extracting conversions for '{created.Name}' from USDA");

                var conversions = await _nutritionalDataService.ExtractConversionsAsync(
                    created.Name,
                    created.Id,
                    created.LocationId);

                if (conversions.Any())
                {
                    await _conversionRepository.AddRangeAsync(conversions);

                    // Update ingredient metadata
                    created.ConversionLastUpdated = DateTime.UtcNow;
                    created.ConversionSource = "USDA";
                    await _repository.UpdateAsync(created);

                    SyncDebugLogger.WriteSuccess($"Extracted {conversions.Count} conversions for '{created.Name}'");
                    Debug.WriteLine($"[IngredientService] Successfully extracted {conversions.Count} conversions");
                    _logger?.LogInformation("Auto-extracted {Count} conversions for {IngredientName}",
                        conversions.Count, created.Name);
                }
                else
                {
                    Debug.WriteLine($"[IngredientService] No conversions found for '{created.Name}'");
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError($"Failed to extract conversions for '{created.Name}'", ex);
                Debug.WriteLine($"[IngredientService] Exception extracting conversions: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to auto-extract conversions for {IngredientName}", created.Name);
                // Don't fail the entire operation if conversion extraction fails
            }
        }

        // Invalidate cache to reflect new ingredient
        InvalidateCache(created.LocationId);

        // Auto-sync to Supabase if authenticated
        if (_sessionService?.IsAuthenticated == true && _dataService != null)
        {
            try
            {
                var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                if (!string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine($"[IngredientService] Auto-syncing ingredient '{created.Name}' to Supabase");
                    SyncDebugLogger.WriteInfo($"Auto-syncing ingredient '{created.Name}' to Supabase");

                    var supabaseIngredient = created.ToSupabase();
                    var result = await _dataService.UpsertAsync(supabaseIngredient);

                    if (result.IsSuccess)
                    {
                        SyncDebugLogger.WriteSuccess($"Ingredient '{created.Name}' synced to Supabase");
                        Debug.WriteLine($"[IngredientService] Successfully synced ingredient to Supabase");
                    }
                    else
                    {
                        SyncDebugLogger.WriteError("Auto-sync ingredient failed", new Exception(result.Error ?? "Unknown"));
                        _logger?.LogWarning("Failed to auto-sync ingredient to Supabase: {Error}", result.Error);
                    }
                }
                else
                {
                    Debug.WriteLine("[IngredientService] Skipping auto-sync - no Supabase Auth UID");
                }
            }
            catch (Exception ex)
            {
                SyncDebugLogger.WriteError("Auto-sync ingredient exception", ex);
                Debug.WriteLine($"[IngredientService] Exception auto-syncing ingredient: {ex.Message}");
                _logger?.LogWarning(ex, "Failed to auto-sync ingredient to Supabase");
            }
        }

        return created;
    }

    public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(ingredient.Name))
            throw new ArgumentException("Ingredient name is required");

        if (ingredient.CurrentPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        // Verify ingredient exists and get old price
        var existingIngredient = await _repository.GetByIdAsync(ingredient.Id);
        if (existingIngredient == null)
            throw new InvalidOperationException("Ingredient not found");

        var oldPrice = existingIngredient.CurrentPrice;

        try
        {
            var updated = await _repository.UpdateAsync(ingredient);

            // Record price change if price changed
            if (oldPrice != updated.CurrentPrice)
            {
                await _priceHistoryService.RecordPriceChangeAsync(updated.Id, updated.CurrentPrice);
            }

            // Auto-extract USDA conversions if enabled and ingredient name changed
            var nameChanged = !string.Equals(existingIngredient.Name, updated.Name, StringComparison.OrdinalIgnoreCase);
            var conversionEnabledChanged = !existingIngredient.AutoConversionEnabled && updated.AutoConversionEnabled;

            if (updated.AutoConversionEnabled &&
                (nameChanged || conversionEnabledChanged) &&
                _nutritionalDataService != null &&
                _conversionRepository != null)
            {
                try
                {
                    Debug.WriteLine($"[IngredientService] Re-extracting conversions for '{updated.Name}' from USDA (name changed: {nameChanged})");
                    SyncDebugLogger.WriteInfo($"Re-extracting conversions for '{updated.Name}' from USDA");

                    var conversions = await _nutritionalDataService.ExtractConversionsAsync(
                        updated.Name,
                        updated.Id,
                        updated.LocationId);

                    if (conversions.Any())
                    {
                        // Remove old ingredient-specific conversions before adding new ones
                        var existingConversions = await _conversionRepository.GetByIngredientIdAsync(updated.Id);
                        foreach (var existing in existingConversions)
                        {
                            await _conversionRepository.DeleteAsync(existing.Id);
                        }

                        await _conversionRepository.AddRangeAsync(conversions);

                        // Update ingredient metadata
                        updated.ConversionLastUpdated = DateTime.UtcNow;
                        updated.ConversionSource = "USDA";
                        await _repository.UpdateAsync(updated);

                        SyncDebugLogger.WriteSuccess($"Re-extracted {conversions.Count} conversions for '{updated.Name}'");
                        Debug.WriteLine($"[IngredientService] Successfully re-extracted {conversions.Count} conversions");
                        _logger?.LogInformation("Re-extracted {Count} conversions for {IngredientName}",
                            conversions.Count, updated.Name);
                    }
                    else
                    {
                        Debug.WriteLine($"[IngredientService] No conversions found for '{updated.Name}'");
                    }
                }
                catch (Exception ex)
                {
                    SyncDebugLogger.WriteError($"Failed to re-extract conversions for '{updated.Name}'", ex);
                    Debug.WriteLine($"[IngredientService] Exception re-extracting conversions: {ex.Message}");
                    _logger?.LogWarning(ex, "Failed to re-extract conversions for {IngredientName}", updated.Name);
                    // Don't fail the entire operation if conversion extraction fails
                }
            }

            // Invalidate cache to reflect updated ingredient
            InvalidateCache(updated.LocationId);

            // Auto-sync to Supabase if authenticated
            if (_sessionService?.IsAuthenticated == true && _dataService != null)
            {
                try
                {
                    var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        Debug.WriteLine($"[IngredientService] Auto-syncing updated ingredient '{updated.Name}' to Supabase");
                        SyncDebugLogger.WriteInfo($"Auto-syncing updated ingredient '{updated.Name}' to Supabase");

                        var supabaseIngredient = updated.ToSupabase();
                        var result = await _dataService.UpsertAsync(supabaseIngredient);

                        if (result.IsSuccess)
                        {
                            SyncDebugLogger.WriteSuccess($"Updated ingredient '{updated.Name}' synced to Supabase");
                            Debug.WriteLine($"[IngredientService] Successfully synced updated ingredient to Supabase");
                        }
                        else
                        {
                            SyncDebugLogger.WriteError("Auto-sync updated ingredient failed", new Exception(result.Error ?? "Unknown"));
                            _logger?.LogWarning("Failed to auto-sync updated ingredient to Supabase: {Error}", result.Error);
                        }
                    }
                }
                catch (Exception syncEx)
                {
                    SyncDebugLogger.WriteError("Auto-sync update exception", syncEx);
                    Debug.WriteLine($"[IngredientService] Exception auto-syncing updated ingredient: {syncEx.Message}");
                    _logger?.LogWarning(syncEx, "Failed to auto-sync updated ingredient to Supabase");
                }
            }

            return updated;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            // Concurrency conflict detected - throw custom exception with more details
            throw new ConcurrencyException(
                "This ingredient was modified by another user. Please refresh and try again.",
                ingredient,
                ex);
        }
    }

    public async Task DeleteIngredientAsync(Guid id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Starting DeleteIngredientAsync");
            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Ingredient ID: {id}");

            // Verify ingredient exists and get location ID for cache invalidation
            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Checking if ingredient exists...");
            var ingredient = await _repository.GetByIdAsync(id);
            if (ingredient == null)
            {
                System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Ingredient not found!");
                throw new InvalidOperationException("Ingredient not found");
            }
            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Ingredient exists");

            var locationId = ingredient.LocationId;

            // TODO: Later add check if ingredient is used in any recipes
            // For now, just delete it

            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] Calling repository.DeleteAsync...");
            await _repository.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"      [INGREDIENT SERVICE] DeleteAsync complete");

            // Invalidate cache to reflect deleted ingredient
            InvalidateCache(locationId);

            // Auto-sync deletion to Supabase if authenticated
            if (_sessionService?.IsAuthenticated == true && _dataService != null)
            {
                try
                {
                    var userId = _sessionService.CurrentUser?.SupabaseAuthUid;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        Debug.WriteLine($"[IngredientService] Auto-syncing deletion of ingredient {id} to Supabase");
                        SyncDebugLogger.WriteInfo($"Auto-syncing deletion of ingredient {id} to Supabase");

                        var result = await _dataService.DeleteAsync<SupabaseIngredient>(id);

                        if (result.IsSuccess)
                        {
                            SyncDebugLogger.WriteSuccess($"Ingredient {id} deleted from Supabase");
                            Debug.WriteLine($"[IngredientService] Successfully deleted ingredient from Supabase");
                        }
                        else
                        {
                            SyncDebugLogger.WriteError("Auto-sync ingredient deletion failed", new Exception(result.Error ?? "Unknown"));
                            _logger?.LogWarning("Failed to auto-sync ingredient deletion to Supabase: {Error}", result.Error);
                        }
                    }
                }
                catch (Exception syncEx)
                {
                    SyncDebugLogger.WriteError("Auto-sync deletion exception", syncEx);
                    Debug.WriteLine($"[IngredientService] Exception auto-syncing deletion: {syncEx.Message}");
                    _logger?.LogWarning(syncEx, "Failed to auto-sync ingredient deletion to Supabase");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("      ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("      ║ [INGREDIENT SERVICE EXCEPTION]                    ║");
            System.Diagnostics.Debug.WriteLine("      ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"      Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"      Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"      Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"      Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"      Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("      ╚═══════════════════════════════════════════════════╝");
            throw;
        }
    }

    public async Task<List<Ingredient>> SearchIngredientsAsync(string searchTerm, Guid locationId)
    {
        return await _repository.SearchAsync(searchTerm, locationId);
    }
}