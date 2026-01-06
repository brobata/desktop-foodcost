using System;
using Freecost.Core.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Freecost.Core.Models;
using Freecost.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Simplified sync service for Supabase
/// Replaces the 2000+ line SyncService with ~500 lines using SQL-based delta sync
/// Key improvements:
/// - Uses WHERE modified_at > last_sync_time for delta sync (no complex tracking)
/// - Uses Supabase upsert for simple conflict resolution
/// - SQL queries instead of document-by-document operations
/// - Much faster and more reliable
/// </summary>
public class SupabaseSyncService : ISyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IUserSessionService _sessionService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly SupabaseDataService _dataService;
    private readonly SupabasePhotoService _photoService;
    private readonly ILogger<SupabaseSyncService>? _logger;

    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;
    public event EventHandler<SyncCompletedEventArgs>? SyncCompleted;

    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncTime { get; private set; }

    public SupabaseSyncService(
        IServiceScopeFactory scopeFactory,
        IUserSessionService sessionService,
        ICurrentLocationService currentLocationService,
        SupabaseDataService dataService,
        SupabasePhotoService photoService,
        ILogger<SupabaseSyncService>? logger = null)
    {
        _scopeFactory = scopeFactory;
        _sessionService = sessionService;
        _currentLocationService = currentLocationService;
        _dataService = dataService;
        _photoService = photoService;
        _logger = logger;

        // Load last sync time from disk
        LoadLastSyncTime();
    }

    private string GetSyncTimeFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var freecostPath = System.IO.Path.Combine(appDataPath, "Freecost");
        System.IO.Directory.CreateDirectory(freecostPath);
        return System.IO.Path.Combine(freecostPath, "last_sync_time.txt");
    }

    private void LoadLastSyncTime()
    {
        try
        {
            var filePath = GetSyncTimeFilePath();
            // logFile variable removed - using SafeFileLogger

            if (System.IO.File.Exists(filePath))
            {
                var timeString = System.IO.File.ReadAllText(filePath);
                SafeFileLogger.Log("sync", $"[SYNC] Found last_sync_time.txt: {timeString}");

                if (DateTime.TryParse(timeString, out var lastSync))
                {
                    LastSyncTime = lastSync;
                    Debug.WriteLine($"[Sync] Loaded last sync time: {lastSync}");
                    SafeFileLogger.Log("sync", $"[SYNC] LastSyncTime loaded successfully: {lastSync:O}");
                }
                else
                {
                    SafeFileLogger.Log("sync", "[SYNC] Failed to parse timestamp");
                }
            }
            else
            {
                SafeFileLogger.Log("sync", "[SYNC] last_sync_time.txt does not exist - will download all records");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Sync] Failed to load last sync time: {ex.Message}");
            // logFile variable removed - using SafeFileLogger
            SafeFileLogger.Log("sync", $"[SYNC] Error loading last sync time: {ex.Message}");
        }
    }

    private void SaveLastSyncTime()
    {
        try
        {
            var filePath = GetSyncTimeFilePath();
            if (LastSyncTime.HasValue)
            {
                System.IO.File.WriteAllText(filePath, LastSyncTime.Value.ToString("O"));
                Debug.WriteLine($"[Sync] Saved last sync time: {LastSyncTime.Value}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Sync] Failed to save last sync time: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform bidirectional sync: Pull changes from Supabase, then push local changes
    /// Much simpler than Firebase version - uses SQL delta queries
    /// </summary>
    public async Task<SyncResult> SyncAsync()
    {
        if (IsSyncing)
        {
            return SyncResult.Failure("Sync already in progress");
        }

        if (_sessionService == null || !_sessionService.IsAuthenticated)
        {
            return SyncResult.Failure("Not authenticated. Please sign in to sync.");
        }

        var locationId = _currentLocationService.HasLocation ? _currentLocationService.CurrentLocationId : Guid.Empty;
        if (locationId == Guid.Empty)
        {
            return SyncResult.Failure("No location selected. Please select a location to sync.");
        }

        IsSyncing = true;
        var result = new SyncResult();
        var stopwatch = Stopwatch.StartNew();

        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Resolve repositories from the new scope (fresh DbContext)
        var ingredientRepo = serviceProvider.GetRequiredService<IIngredientRepository>();
        var recipeRepo = serviceProvider.GetRequiredService<IRecipeRepository>();
        var entreeRepo = serviceProvider.GetRequiredService<IEntreeRepository>();

        try
        {
            _logger?.LogInformation("Starting Supabase sync for location {LocationId}...", locationId);
            ReportProgress("Starting sync...", 0);

            // Phase 1: Pull from Supabase (0-50%)
            ReportProgress("Downloading changes from cloud...", 10);
            var pullResult = await PullFromSupabaseAsync(locationId, ingredientRepo, recipeRepo, entreeRepo);
            result.ItemsDownloaded = pullResult.ItemsDownloaded;
            result.Conflicts.AddRange(pullResult.Conflicts);

            // Phase 2: Push to Supabase (50-90%)
            ReportProgress("Uploading local changes to cloud...", 50);
            var pushResult = await PushToSupabaseAsync(locationId, ingredientRepo, recipeRepo, entreeRepo);
            result.ItemsUploaded = pushResult.ItemsUploaded;

            // Phase 3: Finalize (90-100%)
            ReportProgress("Finalizing sync...", 90);
            LastSyncTime = DateTime.UtcNow;
            SaveLastSyncTime();
            result.IsSuccess = true;
            result.SyncDuration = stopwatch.Elapsed;

            ReportProgress("Sync complete!", 100);
            _logger?.LogInformation(
                "Sync completed successfully. Downloaded: {Downloaded}, Uploaded: {Uploaded}, Duration: {Duration}ms",
                result.ItemsDownloaded, result.ItemsUploaded, stopwatch.ElapsedMilliseconds);

            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Sync failed");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.SyncDuration = stopwatch.Elapsed;

            ReportProgress($"Sync failed: {ex.Message}", 0);
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });
            return result;
        }
        finally
        {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Upload photo to Supabase Storage if it's a local file path
    /// Returns the public URL or the original URL if already uploaded
    /// </summary>
    private async Task<string?> UploadPhotoIfLocalAsync(string? photoUrl, string entityType, Guid entityId)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return photoUrl;

        // Check if it's already a cloud URL (starts with http:// or https://)
        if (photoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            photoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine($"[PhotoSync] Already a cloud URL: {photoUrl}");
            return photoUrl;
        }

        // If it's just a filename (no path separators), construct the full path
        string localFilePath;
        if (!photoUrl.Contains("\\") && !photoUrl.Contains("/"))
        {
            // Just a filename - construct path to EntreePhotos folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            localFilePath = System.IO.Path.Combine(appDataPath, "Freecost", "Photos", "EntreePhotos", photoUrl);
            Debug.WriteLine($"[PhotoSync] Converted filename to full path: {localFilePath}");
        }
        else
        {
            // Already has path separators, use as-is
            localFilePath = photoUrl;
        }

        try
        {
            if (!System.IO.File.Exists(localFilePath))
            {
                Debug.WriteLine($"[PhotoSync] ❌ Local photo not found: {localFilePath}");
                return photoUrl; // Return original, don't break sync
            }

            Debug.WriteLine($"[PhotoSync] ✓ Found local photo: {localFilePath}");
            Debug.WriteLine($"[PhotoSync] ⬆ Uploading to Supabase Storage...");
            var uploadResult = await _photoService.UploadPhotoAsync(
                localFilePath,
                entityType,
                entityId,
                caption: null);

            if (uploadResult.IsSuccess && uploadResult.Photo != null)
            {
                Debug.WriteLine($"[PhotoSync] ✓ Photo uploaded: {uploadResult.Photo.PublicUrl}");
                _logger?.LogInformation("Uploaded photo for {EntityType} {EntityId}", entityType, entityId);
                return uploadResult.Photo.PublicUrl;
            }
            else
            {
                Debug.WriteLine($"[PhotoSync] ✗ Upload failed: {uploadResult.ErrorMessage}");
                _logger?.LogWarning("Failed to upload photo for {EntityType} {EntityId}: {Error}",
                    entityType, entityId, uploadResult.ErrorMessage);
                return photoUrl; // Return original, don't break sync
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PhotoSync] ❌ Exception uploading photo: {ex.GetType().Name}");
            Debug.WriteLine($"[PhotoSync] ❌ Error message: {ex.Message}");
            Debug.WriteLine($"[PhotoSync] ❌ Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"[PhotoSync] ❌ Inner exception: {ex.InnerException.Message}");
            }
            _logger?.LogError(ex, "Exception uploading photo for {EntityType} {EntityId}", entityType, entityId);
            return photoUrl; // Return original, don't break sync
        }
    }

    /// <summary>
    /// Pull changes from Supabase since last sync
    /// Uses SQL WHERE modified_at > last_sync_time for efficiency
    /// </summary>
    private async Task<SyncResult> PullFromSupabaseAsync(
        Guid locationId,
        IIngredientRepository ingredientRepo,
        IRecipeRepository recipeRepo,
        IEntreeRepository entreeRepo)
    {
        var result = new SyncResult();

        // Check if local database is empty for this location - force full sync
        var localIngredients = await ingredientRepo.GetAllAsync(locationId);
        var localRecipes = (await recipeRepo.GetAllRecipesAsync(locationId)).ToList();
        var localEntrees = await entreeRepo.GetAllAsync(locationId);
        var isLocationEmpty = !localIngredients.Any() && !localRecipes.Any() && !localEntrees.Any();

        // Force full sync if location is empty, even if LastSyncTime exists
        var syncStart = (isLocationEmpty || !LastSyncTime.HasValue) ? DateTime.MinValue : LastSyncTime.Value;

        // logFile variable removed - using SafeFileLogger

        SafeFileLogger.Log("sync", "=== SYNC STARTED ===");
        SafeFileLogger.Log("sync", $"Location ID: {locationId}");
        SafeFileLogger.Log("sync", $"Local data: {localIngredients.Count()} ingredients, {localRecipes.Count} recipes, {localEntrees.Count()} entrees");
        SafeFileLogger.Log("sync", $"Is location empty: {isLocationEmpty}");
        SafeFileLogger.Log("sync", $"LastSyncTime: {(LastSyncTime.HasValue ? LastSyncTime.Value.ToString("O") : "NULL")}");
        SafeFileLogger.Log("sync", $"Sync mode: {(syncStart == DateTime.MinValue ? "FULL SYNC (downloading ALL records)" : "DELTA SYNC (downloading changes only)")}");
        SafeFileLogger.Log("sync", $"Sync start time: {syncStart:O}");

        Debug.WriteLine($"[Sync] Pulling changes since {syncStart} (Full sync: {syncStart == DateTime.MinValue})");

        try
        {
            // Pull ingredients modified since last sync
            var ingredientsResult = await _dataService.GetAllAsync<SupabaseIngredient>(
                locationId: locationId,
                modifiedAfter: syncStart);

            if (ingredientsResult.IsSuccess && ingredientsResult.Data != null)
            {
                SafeFileLogger.Log("sync", $"Supabase returned {ingredientsResult.Data.Count} ingredients");

                int addedCount = 0, updatedCount = 0, skippedCount = 0;
                foreach (var supabaseIngredient in ingredientsResult.Data)
                {
                    var ingredient = supabaseIngredient.ToEfCore();

                    // Check if exists locally
                    var existing = await ingredientRepo.GetByIdAsync(ingredient.Id);
                    if (existing == null)
                    {
                        // New from cloud - add to local
                        await ingredientRepo.AddAsync(ingredient);
                        addedCount++;
                        Debug.WriteLine($"[Sync] Added ingredient from cloud: {ingredient.Name}");
                    }
                    else if (ingredient.ModifiedAt > existing.ModifiedAt)
                    {
                        // Cloud is newer - update local
                        // Merge logic: cloud wins by default
                        await ingredientRepo.UpdateAsync(ingredient);
                        updatedCount++;
                        Debug.WriteLine($"[Sync] Updated ingredient from cloud: {ingredient.Name}");
                    }
                    else if (ingredient.ModifiedAt < existing.ModifiedAt)
                    {
                        // Local is newer - conflict (will be resolved in push phase)
                        // Conflict: local version newer
                        skippedCount++;
                        Debug.WriteLine($"[Sync] Conflict: Ingredient {ingredient.Name} - local newer, will upload in push phase");
                    }
                    else
                    {
                        skippedCount++;
                    }

                    result.ItemsDownloaded++;
                }

                SafeFileLogger.Log("sync", $"Ingredients - Added: {addedCount}, Updated: {updatedCount}, Skipped: {skippedCount}");
                Debug.WriteLine($"[Sync] Pulled {ingredientsResult.Data.Count} ingredients");
            }
            else
            {
                SafeFileLogger.Log("sync", "Supabase returned 0 ingredients");
            }

            // Pull recipes modified since last sync
            var recipesResult = await _dataService.GetAllAsync<SupabaseRecipe>(
                locationId: locationId,
                modifiedAfter: syncStart);

            if (recipesResult.IsSuccess && recipesResult.Data != null)
            {
                foreach (var supabaseRecipe in recipesResult.Data)
                {
                    var recipe = supabaseRecipe.ToEfCore();

                    var existing = await recipeRepo.GetRecipeByIdAsync(recipe.Id);
                    if (existing == null)
                    {
                        await recipeRepo.CreateRecipeAsync(recipe);
                        Debug.WriteLine($"[Sync] Added recipe from cloud: {recipe.Name}");
                    }
                    else if (recipe.ModifiedAt > existing.ModifiedAt)
                    {
                        await recipeRepo.UpdateRecipeAsync(recipe);
                        Debug.WriteLine($"[Sync] Updated recipe from cloud: {recipe.Name}");
                    }
                    else if (recipe.ModifiedAt < existing.ModifiedAt)
                    {
                        // Conflict: local version newer
                        Debug.WriteLine($"[Sync] Conflict: Recipe {recipe.Name} - local newer");
                    }

                    result.ItemsDownloaded++;
                }

                Debug.WriteLine($"[Sync] Pulled {recipesResult.Data.Count} recipes");
            }

            // Pull entrees modified since last sync
            var entreesResult = await _dataService.GetAllAsync<SupabaseEntree>(
                locationId: locationId,
                modifiedAfter: syncStart);

            if (entreesResult.IsSuccess && entreesResult.Data != null)
            {
                foreach (var supabaseEntree in entreesResult.Data)
                {
                    var entree = supabaseEntree.ToEfCore();

                    var existing = await entreeRepo.GetByIdAsync(entree.Id);
                    if (existing == null)
                    {
                        await entreeRepo.CreateAsync(entree);
                        Debug.WriteLine($"[Sync] Added entree from cloud: {entree.Name}");
                    }
                    else if (entree.ModifiedAt > existing.ModifiedAt)
                    {
                        await entreeRepo.UpdateAsync(entree);
                        Debug.WriteLine($"[Sync] Updated entree from cloud: {entree.Name}");
                    }
                    else if (entree.ModifiedAt < existing.ModifiedAt)
                    {
                        // Conflict: local version newer
                        Debug.WriteLine($"[Sync] Conflict: Entree {entree.Name} - local newer");
                    }

                    result.ItemsDownloaded++;
                }

                Debug.WriteLine($"[Sync] Pulled {entreesResult.Data.Count} entrees");
            }

            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error pulling from Supabase");
            result.IsSuccess = false;
            result.ErrorMessage = $"Pull failed: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Push local changes to Supabase
    /// Uses Supabase upsert for simple conflict resolution
    /// </summary>
    private async Task<SyncResult> PushToSupabaseAsync(
        Guid locationId,
        IIngredientRepository ingredientRepo,
        IRecipeRepository recipeRepo,
        IEntreeRepository entreeRepo)
    {
        var result = new SyncResult();
        var syncStart = LastSyncTime ?? DateTime.MinValue;

        Debug.WriteLine($"[Sync] Pushing changes since {syncStart}");

        try
        {
            // Push ingredients modified since last sync
            var localIngredients = await ingredientRepo.GetAllAsync(locationId);
            SyncDebugLogger.WriteSection("PUSH INGREDIENTS TO SUPABASE");
            SyncDebugLogger.WriteInfo($"LastSyncTime: {LastSyncTime?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "NULL (first sync)"}");
            SyncDebugLogger.WriteInfo($"Sync threshold: {syncStart:yyyy-MM-dd HH:mm:ss.fff}");
            SyncDebugLogger.WriteInfo($"Found {localIngredients.Count} total local ingredients");

            var modifiedIngredients = localIngredients.Where(i => i.ModifiedAt > syncStart).ToList();
            SyncDebugLogger.WriteInfo($"Filtered to {modifiedIngredients.Count} ingredients modified after threshold");
            
            if (localIngredients.Count > 0 && modifiedIngredients.Count == 0)
            {
                SyncDebugLogger.WriteWarning("⚠️ ALL INGREDIENTS FILTERED OUT! Showing ingredient timestamps:");
                foreach (var ing in localIngredients.Take(5))
                {
                    SyncDebugLogger.WriteInfo($"  - {ing.Name}: ModifiedAt={ing.ModifiedAt:yyyy-MM-dd HH:mm:ss.fff}");
                }
            }

            if (modifiedIngredients.Any())
            {
                SyncDebugLogger.WriteInfo($"Converting {modifiedIngredients.Count} ingredients to Supabase format...");
                var supabaseIngredients = modifiedIngredients.Select(i => i.ToSupabase()).ToList();
                SyncDebugLogger.WriteInfo($"Converted successfully. Calling UpsertBulkAsync...");

                // Use upsert to insert new or update existing
                var upsertResult = await _dataService.UpsertBulkAsync(supabaseIngredients);
                SyncDebugLogger.WriteInfo($"UpsertBulkAsync returned. IsSuccess: {upsertResult.IsSuccess}");

                if (upsertResult.IsSuccess)
                {
                    result.ItemsUploaded += modifiedIngredients.Count;
                    SyncDebugLogger.WriteSuccess($"Successfully pushed {modifiedIngredients.Count} ingredients to cloud");
                    Debug.WriteLine($"[Sync] Pushed {modifiedIngredients.Count} ingredients to cloud");
                }
                else
                {
                    SyncDebugLogger.WriteError("Failed to push ingredients", new Exception(upsertResult.Error ?? "Unknown error"));
                    _logger?.LogWarning("Failed to push ingredients: {Error}", upsertResult.Error);
                }
            }
            else
            {
                SyncDebugLogger.WriteInfo("No modified ingredients to push (list is empty)");
            }

            // Push recipes modified since last sync
            var localRecipes = await recipeRepo.GetAllRecipesAsync(locationId);
            var modifiedRecipes = localRecipes.Where(r => r.ModifiedAt > syncStart).ToList();

            if (modifiedRecipes.Any())
            {
                var supabaseRecipes = modifiedRecipes.Select(r => r.ToSupabase()).ToList();
                var upsertResult = await _dataService.UpsertBulkAsync(supabaseRecipes);
                if (upsertResult.IsSuccess)
                {
                    result.ItemsUploaded += modifiedRecipes.Count;
                    Debug.WriteLine($"[Sync] Pushed {modifiedRecipes.Count} recipes to cloud");
                }
                else
                {
                    _logger?.LogWarning("Failed to push recipes: {Error}", upsertResult.Error);
                }
            }

            // Push entrees modified since last sync
            var localEntrees = await entreeRepo.GetAllAsync(locationId);

            // IMPORTANT: Check ALL entrees for local photos (not just modified ones)
            // This ensures existing photos get uploaded to cloud even if entree hasn't been modified
            Debug.WriteLine($"[PhotoSync] Checking {localEntrees.Count()} entrees for local photos...");
            int photosUploaded = 0;
            int photosChecked = 0;
            int photosWithUrl = 0;
            foreach (var entree in localEntrees)
            {
                photosChecked++;
                Debug.WriteLine($"[PhotoSync] Entree #{photosChecked}: '{entree.Name}' - PhotoUrl: {(string.IsNullOrEmpty(entree.PhotoUrl) ? "NULL/EMPTY" : entree.PhotoUrl)}");

                if (!string.IsNullOrEmpty(entree.PhotoUrl))
                {
                    photosWithUrl++;
                    Debug.WriteLine($"[PhotoSync] Attempting to upload photo for '{entree.Name}'...");
                    var cloudUrl = await UploadPhotoIfLocalAsync(entree.PhotoUrl, "Entree", entree.Id);
                    Debug.WriteLine($"[PhotoSync] Upload result: Original={entree.PhotoUrl}, Cloud={cloudUrl}");
                    if (cloudUrl != entree.PhotoUrl)
                    {
                        // Photo was uploaded, update the URL
                        entree.PhotoUrl = cloudUrl;
                        await entreeRepo.UpdateAsync(entree);
                        photosUploaded++;
                        Debug.WriteLine($"[PhotoSync] Updated entree '{entree.Name}' with cloud photo URL");
                    }
                }
            }

            Debug.WriteLine($"[PhotoSync] Summary: Checked {photosChecked} entrees, {photosWithUrl} had PhotoUrl, {photosUploaded} uploaded");
            if (photosUploaded > 0)
            {
                Debug.WriteLine($"[PhotoSync] ✓ Uploaded {photosUploaded} photo(s) to cloud");
                _logger?.LogInformation("Uploaded {Count} photos to Supabase Storage", photosUploaded);
            }
            else if (photosWithUrl > 0)
            {
                Debug.WriteLine($"[PhotoSync] ℹ No photos uploaded (already have cloud URLs or upload failed)");
            }
            else
            {
                Debug.WriteLine($"[PhotoSync] ℹ No entrees have PhotoUrl set");
            }

            var modifiedEntrees = localEntrees.Where(e => e.ModifiedAt > syncStart).ToList();

            if (modifiedEntrees.Any())
            {
                // Photos already uploaded above, just push the entree data
                var supabaseEntrees = modifiedEntrees.Select(e => e.ToSupabase()).ToList();
                var upsertResult = await _dataService.UpsertBulkAsync(supabaseEntrees);
                if (upsertResult.IsSuccess)
                {
                    result.ItemsUploaded += modifiedEntrees.Count;
                    Debug.WriteLine($"[Sync] Pushed {modifiedEntrees.Count} entrees to cloud");
                }
                else
                {
                    _logger?.LogWarning("Failed to push entrees: {Error}", upsertResult.Error);
                }
            }

            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error pushing to Supabase");
            result.IsSuccess = false;
            result.ErrorMessage = $"Push failed: {ex.Message}";
            return result;
        }
    }

    private void ReportProgress(string message, int percentage)
    {
        Debug.WriteLine($"[Sync] {percentage}% - {message}");
        SyncProgressChanged?.Invoke(this, new SyncProgressEventArgs
        {
            Message = message,
            PercentComplete = percentage
        });
    }

    /// <summary>
    /// Push local changes to Supabase only (no pull)
    /// </summary>
    public async Task<SyncResult> PushOnlyAsync()
    {
        if (IsSyncing) return SyncResult.Failure("Sync already in progress");
        IsSyncing = true;
        var startTime = DateTime.UtcNow;
        var result = new SyncResult();
        try
        {
            var locationId = _currentLocationService.HasLocation ? _currentLocationService.CurrentLocationId : Guid.Empty;
            if (locationId == Guid.Empty) return SyncResult.Failure("No location selected");
            using var scope = _scopeFactory.CreateScope();
            var ingredientRepo = scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
            var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
            var entreeRepo = scope.ServiceProvider.GetRequiredService<IEntreeRepository>();
            ReportProgress("Pushing changes...", 50);
            result = await PushToSupabaseAsync(locationId, ingredientRepo, recipeRepo, entreeRepo);
            LastSyncTime = DateTime.UtcNow;
            SaveLastSyncTime();
            return result;
        }
        finally
        {
            IsSyncing = false;
            result.SyncDuration = DateTime.UtcNow - startTime;
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });
        }
    }

    public async Task<SyncResult> PullOnlyAsync()
    {
        if (IsSyncing) return SyncResult.Failure("Sync already in progress");
        IsSyncing = true;
        var startTime = DateTime.UtcNow;
        var result = new SyncResult();
        try
        {
            var locationId = _currentLocationService.HasLocation ? _currentLocationService.CurrentLocationId : Guid.Empty;
            if (locationId == Guid.Empty) return SyncResult.Failure("No location selected");
            using var scope = _scopeFactory.CreateScope();
            var ingredientRepo = scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
            var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
            var entreeRepo = scope.ServiceProvider.GetRequiredService<IEntreeRepository>();
            ReportProgress("Pulling changes...", 50);
            result = await PullFromSupabaseAsync(locationId, ingredientRepo, recipeRepo, entreeRepo);
            LastSyncTime = DateTime.UtcNow;
            SaveLastSyncTime();
            return result;
        }
        finally
        {
            IsSyncing = false;
            result.SyncDuration = DateTime.UtcNow - startTime;
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });
        }
    }

    public async Task<SyncResult> SyncRestaurantsAsync()
    {
        var result = new SyncResult();

        if (!_sessionService.IsAuthenticated)
        {
            return SyncResult.Failure("Not authenticated. Please sign in.");
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var locationRepo = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

            // Get all locations from Supabase for the current user
            var locationsResult = await _dataService.GetAllLocationsAsync();

            if (!locationsResult.IsSuccess)
            {
                return SyncResult.Failure($"Failed to fetch locations: {locationsResult.Error}");
            }

            if (locationsResult.Data == null || !locationsResult.Data.Any())
            {
                return SyncResult.Success(); // No locations yet - user needs to create one
            }

            // Sync locations to local database
            foreach (var supabaseLocation in locationsResult.Data)
            {
                var localLocation = await locationRepo.GetByIdAsync(supabaseLocation.Id);

                if (localLocation == null)
                {
                    // Add new location
                    var newLocation = new Location
                    {
                        Id = supabaseLocation.Id,
                        UserId = supabaseLocation.UserId, // Already a string (Supabase Auth UID)
                        Name = supabaseLocation.Name,
                        Address = supabaseLocation.Address,
                        Phone = supabaseLocation.Phone,
                        IsActive = supabaseLocation.IsActive
                    };
                    await locationRepo.AddAsync(newLocation);
                    result.ItemsDownloaded++;
                    Debug.WriteLine($"[SyncRestaurants] ✓ Added location: {newLocation.Name} (UserId: {newLocation.UserId})");
                }
                else
                {
                    // Update existing location - IMPORTANT: Update UserId too!
                    localLocation.UserId = supabaseLocation.UserId; // Sync the Supabase Auth UID
                    localLocation.Name = supabaseLocation.Name;
                    localLocation.Address = supabaseLocation.Address;
                    localLocation.Phone = supabaseLocation.Phone;
                    localLocation.IsActive = supabaseLocation.IsActive;
                    await locationRepo.UpdateAsync(localLocation);
                    Debug.WriteLine($"[SyncRestaurants] ✓ Updated location: {localLocation.Name} (UserId: {localLocation.UserId})");
                }
            }

            result.IsSuccess = true;
            _logger?.LogInformation("Synced {Count} locations from Supabase", result.ItemsDownloaded);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to sync restaurants/locations");
            return SyncResult.Failure($"Error syncing locations: {ex.Message}");
        }
    }

    public async Task<SyncResult> ForceUploadAllAsync()
    {
        if (IsSyncing) return SyncResult.Failure("Sync already in progress");
        IsSyncing = true;
        var startTime = DateTime.UtcNow;
        var result = new SyncResult();
        try
        {
            var locationId = _currentLocationService.HasLocation ? _currentLocationService.CurrentLocationId : Guid.Empty;
            if (locationId == Guid.Empty) return SyncResult.Failure("No location selected");
            using var scope = _scopeFactory.CreateScope();
            var ingredientRepo = scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
            var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
            var entreeRepo = scope.ServiceProvider.GetRequiredService<IEntreeRepository>();
            ReportProgress("Force uploading all data...", 0);
            var allIngredients = await ingredientRepo.GetAllAsync(locationId);
            if (allIngredients.Any())
            {
                var supabaseIngredients = allIngredients.Select(i => i.ToSupabase()).ToList();
                await _dataService.UpsertBulkAsync(supabaseIngredients);
                result.ItemsUploaded += allIngredients.Count;
            }
            var allRecipes = await recipeRepo.GetAllRecipesAsync(locationId);
            if (allRecipes.Any())
            {
                var supabaseRecipes = allRecipes.Select(r => r.ToSupabase()).ToList();
                await _dataService.UpsertBulkAsync(supabaseRecipes);
                result.ItemsUploaded += allRecipes.Count();
            }
            var allEntrees = await entreeRepo.GetAllAsync(locationId);
            if (allEntrees.Any())
            {
                // Upload photos for all entrees with local photo paths
                foreach (var entree in allEntrees)
                {
                    if (!string.IsNullOrEmpty(entree.PhotoUrl))
                    {
                        var cloudUrl = await UploadPhotoIfLocalAsync(entree.PhotoUrl, "Entree", entree.Id);
                        if (cloudUrl != entree.PhotoUrl)
                        {
                            entree.PhotoUrl = cloudUrl;
                            await entreeRepo.UpdateAsync(entree);
                        }
                    }
                }

                var supabaseEntrees = allEntrees.Select(e => e.ToSupabase()).ToList();
                await _dataService.UpsertBulkAsync(supabaseEntrees);
                result.ItemsUploaded += allEntrees.Count();
            }
            LastSyncTime = DateTime.UtcNow;
            SaveLastSyncTime();
            result.IsSuccess = true;
            return result;
        }
        finally
        {
            IsSyncing = false;
            result.SyncDuration = DateTime.UtcNow - startTime;
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs { Result = result });
        }
    }
}
