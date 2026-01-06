using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Microsoft.Extensions.Logging;
using Dfc.Core.Models;
using static Postgrest.Constants;

namespace Dfc.Core.Services;

/// <summary>
/// Supabase data service for CRUD operations
/// Replaces FirestoreDataService with cleaner Supabase Postgrest API
/// Much simpler than Firestore - no JSON wrapping/unwrapping needed
/// </summary>
public class SupabaseDataService
{
    private readonly ILogger<SupabaseDataService>? _logger;

    public SupabaseDataService(ILogger<SupabaseDataService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get a single record by ID from a table
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="id">Record ID (Guid)</param>
    /// <returns>The record or null if not found</returns>
    public async Task<SupabaseResult<T?>> GetByIdAsync<T>(Guid id)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] GetByIdAsync<{typeof(T).Name}> - ID: {id}");

            var response = await client
                .From<T>()
                .Filter("id", Operator.Equals, id.ToString())
                .Single();

            if (response == null)
            {
                Debug.WriteLine($"[Supabase] Record not found: {typeof(T).Name}/{id}");
                return SupabaseResult<T?>.Success(null);
            }

            Debug.WriteLine($"[Supabase] ✓ Retrieved record from {typeof(T).Name}");
            return SupabaseResult<T?>.Success(response);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ GetByIdAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error getting record {Id} from {Table}", id, typeof(T).Name);
            return SupabaseResult<T?>.Failure($"Failed to get record: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all records from a table with optional filtering
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="locationId">Optional location ID for filtering multi-tenant data</param>
    /// <param name="modifiedAfter">Optional datetime to get only records modified after this time</param>
    /// <returns>List of records</returns>
    public async Task<SupabaseResult<List<T>>> GetAllAsync<T>(
        Guid? locationId = null,
        DateTime? modifiedAfter = null)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] GetAllAsync<{typeof(T).Name}>");
            Debug.WriteLine($"[Supabase] Filters - LocationId: {locationId}, ModifiedAfter: {modifiedAfter}");

            // CRITICAL DEBUGGING: Check auth state
            var currentUser = client.Auth.CurrentUser;
            Debug.WriteLine($"[Supabase] *** AUTH STATE CHECK ***");
            Debug.WriteLine($"[Supabase]   CurrentUser: {(currentUser != null ? "AUTHENTICATED" : "NOT AUTHENTICATED")}");
            if (currentUser != null)
            {
                Debug.WriteLine($"[Supabase]   User ID: {currentUser.Id}");
                Debug.WriteLine($"[Supabase]   Email: {currentUser.Email}");
            }
            else
            {
                Debug.WriteLine($"[Supabase]   ⚠️ WARNING: No authenticated user! RLS policies will block all queries!");
            }
            Debug.WriteLine($"[Supabase] *** END AUTH STATE CHECK ***");
            dynamic query = client.From<T>(); // Postgrest.Table<T>

            // Apply location filter if specified (for multi-tenant data)
            if (locationId.HasValue)
            {
                query = query.Filter("location_id", Operator.Equals, locationId.Value.ToString());
            }

            // Apply modified_at filter if specified (for delta sync)
            if (modifiedAfter.HasValue)
            {
                query = query.Filter("modified_at", Operator.GreaterThan, modifiedAfter.Value.ToString("O"));
            }

            var response = await query.Get();
            var items = response?.Models ?? new List<T>();

            Debug.WriteLine($"[Supabase] ✓ Retrieved {items.Count} records from {typeof(T).Name}");
            return SupabaseResult<List<T>>.Success(items);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ GetAllAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error getting records from {Table}", typeof(T).Name);
            return SupabaseResult<List<T>>.Failure($"Failed to get records: {ex.Message}");
        }
    }

    /// <summary>
    /// Insert a new record into a table
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="data">Record to insert</param>
    /// <returns>The inserted record with server-generated fields</returns>
    public async Task<SupabaseResult<T?>> InsertAsync<T>(T data)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] InsertAsync<{typeof(T).Name}>");

            var response = await client
                .From<T>()
                .Insert(data);

            var inserted = response?.Models?.FirstOrDefault();

            if (inserted == null)
            {
                Debug.WriteLine($"[Supabase] ⚠ Insert succeeded but no record returned from {typeof(T).Name}");
                return SupabaseResult<T?>.Failure("Insert succeeded but no record returned");
            }

            Debug.WriteLine($"[Supabase] ✓ Inserted record into {typeof(T).Name}");
            _logger?.LogInformation("Inserted record into {Table}", typeof(T).Name);
            return SupabaseResult<T?>.Success(inserted);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ InsertAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error inserting record into {Table}", typeof(T).Name);
            return SupabaseResult<T?>.Failure($"Failed to insert record: {ex.Message}");
        }
    }

    /// <summary>
    /// Insert multiple records into a table in a single operation
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="items">Records to insert</param>
    /// <returns>The inserted records with server-generated fields</returns>
    public async Task<SupabaseResult<List<T>>> InsertBulkAsync<T>(List<T> items)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] InsertBulkAsync<{typeof(T).Name}> - Count: {items.Count}");

            var response = await client
                .From<T>()
                .Insert(items);

            var inserted = response?.Models ?? new List<T>();

            Debug.WriteLine($"[Supabase] ✓ Inserted {inserted.Count} records into {typeof(T).Name}");
            _logger?.LogInformation("Bulk inserted {Count} records into {Table}", inserted.Count, typeof(T).Name);
            return SupabaseResult<List<T>>.Success(inserted);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ InsertBulkAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error bulk inserting records into {Table}", typeof(T).Name);
            return SupabaseResult<List<T>>.Failure($"Failed to bulk insert records: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing record in a table
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="id">ID of record to update</param>
    /// <param name="data">Updated record data</param>
    /// <returns>The updated record</returns>
    public async Task<SupabaseResult<T?>> UpdateAsync<T>(Guid id, T data)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] UpdateAsync<{typeof(T).Name}> - ID: {id}");

            var response = await client
                .From<T>()
                .Filter("id", Operator.Equals, id.ToString())
                .Update(data);

            var updated = response?.Models?.FirstOrDefault();

            if (updated == null)
            {
                Debug.WriteLine($"[Supabase] ⚠ Update succeeded but no record returned from {typeof(T).Name}");
                return SupabaseResult<T?>.Failure("Update succeeded but no record returned");
            }

            Debug.WriteLine($"[Supabase] ✓ Updated record in {typeof(T).Name}, ID: {id}");
            _logger?.LogInformation("Updated record in {Table}, ID: {Id}", typeof(T).Name, id);
            return SupabaseResult<T?>.Success(updated);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ UpdateAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error updating record in {Table}, ID: {Id}", typeof(T).Name, id);
            return SupabaseResult<T?>.Failure($"Failed to update record: {ex.Message}");
        }
    }

    /// <summary>
    /// Upsert a record (insert if not exists, update if exists)
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="data">Record to upsert</param>
    /// <returns>The upserted record</returns>
    public async Task<SupabaseResult<T?>> UpsertAsync<T>(T data)
        where T : Postgrest.Models.BaseModel, new()
    {
        SyncDebugLogger.WriteInfo($"UpsertAsync<{typeof(T).Name}> called");
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] UpsertAsync<{typeof(T).Name}>");
            SyncDebugLogger.WriteInfo($"Got Supabase client, attempting upsert to table: {typeof(T).Name}");

            // CRITICAL DEBUG: Check auth state before upsert
            var currentUser = client.Auth.CurrentUser;
            var currentSession = client.Auth.CurrentSession;
            Debug.WriteLine($"[UPSERT DEBUG] Current User: {(currentUser != null ? currentUser.Email : "NULL")}");
            Debug.WriteLine($"[UPSERT DEBUG] User ID: {(currentUser != null ? currentUser.Id : "NULL")}");
            Debug.WriteLine($"[UPSERT DEBUG] Session exists: {(currentSession != null ? "YES" : "NO")}");
            Debug.WriteLine($"[UPSERT DEBUG] Access Token length: {(currentSession?.AccessToken?.Length ?? 0)}");
            SyncDebugLogger.WriteInfo($"Auth check: User={currentUser?.Email ?? "NULL"}, Session={(currentSession != null ? "EXISTS" : "NULL")}");

            // Log the data being upserted (for location debugging)
            if (data is SupabaseLocation loc)
            {
                SyncDebugLogger.WriteInfo($"  Location data: Id={loc.Id}, UserId={loc.UserId}, Name={loc.Name}");
            }

            var response = await client
                .From<T>()
                .Upsert(data);

            var upserted = response?.Models?.FirstOrDefault();

            if (upserted == null)
            {
                SyncDebugLogger.WriteWarning($"Upsert succeeded but no record returned from {typeof(T).Name}");
                Debug.WriteLine($"[Supabase] ⚠ Upsert succeeded but no record returned from {typeof(T).Name}");
                return SupabaseResult<T?>.Failure("Upsert succeeded but no record returned");
            }

            SyncDebugLogger.WriteSuccess($"Upserted record in {typeof(T).Name}");
            Debug.WriteLine($"[Supabase] ✓ Upserted record in {typeof(T).Name}");
            _logger?.LogInformation("Upserted record in {Table}", typeof(T).Name);
            return SupabaseResult<T?>.Success(upserted);
        }
        catch (Exception ex)
        {
            SyncDebugLogger.WriteError($"UpsertAsync<{typeof(T).Name}>", ex);
            Debug.WriteLine($"[Supabase] ❌ UpsertAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error upserting record in {Table}", typeof(T).Name);
            return SupabaseResult<T?>.Failure($"Failed to upsert record: {ex.Message}");
        }
    }

    /// <summary>
    /// Upsert multiple records in a single operation
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="items">Records to upsert</param>
    /// <returns>The upserted records</returns>
    public async Task<SupabaseResult<List<T>>> UpsertBulkAsync<T>(List<T> items)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] UpsertBulkAsync<{typeof(T).Name}> - Count: {items.Count}");

            var response = await client
                .From<T>()
                .Upsert(items);

            var upserted = response?.Models ?? new List<T>();

            Debug.WriteLine($"[Supabase] ✓ Upserted {upserted.Count} records in {typeof(T).Name}");
            _logger?.LogInformation("Bulk upserted {Count} records in {Table}", upserted.Count, typeof(T).Name);
            return SupabaseResult<List<T>>.Success(upserted);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ UpsertBulkAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error bulk upserting records in {Table}", typeof(T).Name);
            return SupabaseResult<List<T>>.Failure($"Failed to bulk upsert records: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a record from a table
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="id">Record ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<SupabaseResult<bool>> DeleteAsync<T>(Guid id)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] DeleteAsync<{typeof(T).Name}> - ID: {id}");

            await client
                .From<T>()
                .Filter("id", Operator.Equals, id.ToString())
                .Delete();

            Debug.WriteLine($"[Supabase] ✓ Deleted record from {typeof(T).Name}, ID: {id}");
            _logger?.LogInformation("Deleted record from {Table}, ID: {Id}", typeof(T).Name, id);
            return SupabaseResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ DeleteAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error deleting record from {Table}, ID: {Id}", typeof(T).Name, id);
            return SupabaseResult<bool>.Failure($"Failed to delete record: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete multiple records from a table by IDs
    /// </summary>
    /// <typeparam name="T">Model type (must inherit from Postgrest.Models.BaseModel)</typeparam>
    /// <param name="ids">Record IDs to delete</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<SupabaseResult<bool>> DeleteBulkAsync<T>(List<Guid> ids)
        where T : Postgrest.Models.BaseModel, new()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            Debug.WriteLine($"[Supabase] DeleteBulkAsync<{typeof(T).Name}> - Count: {ids.Count}");

            // Delete records in parallel for performance
            var deleteTasks = ids.Select(id =>
                client.From<T>()
                    .Filter("id", Operator.Equals, id.ToString())
                    .Delete()
            );

            await Task.WhenAll(deleteTasks);

            Debug.WriteLine($"[Supabase] ✓ Deleted {ids.Count} records from {typeof(T).Name}");
            _logger?.LogInformation("Bulk deleted {Count} records from {Table}", ids.Count, typeof(T).Name);
            return SupabaseResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ DeleteBulkAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error bulk deleting records from {Table}", typeof(T).Name);
            return SupabaseResult<bool>.Failure($"Failed to bulk delete records: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all locations for the current user
    /// </summary>
    /// <returns>List of locations</returns>
    public async Task<SupabaseResult<List<SupabaseLocation>>> GetAllLocationsAsync()
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();
            var userId = client.Auth.CurrentUser?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                return SupabaseResult<List<SupabaseLocation>>.Failure("User not authenticated");
            }

            Debug.WriteLine($"[Supabase] GetAllLocationsAsync - UserId: {userId}");

            var response = await client
                .From<SupabaseLocation>()
                .Get();

            var locations = response?.Models ?? new List<SupabaseLocation>();

            Debug.WriteLine($"[Supabase] ✓ Retrieved {locations.Count} locations (owned + shared)");
            return SupabaseResult<List<SupabaseLocation>>.Success(locations);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ GetAllLocationsAsync error: {ex.Message}");
            _logger?.LogError(ex, "Error getting locations");
            return SupabaseResult<List<SupabaseLocation>>.Failure($"Failed to get locations: {ex.Message}");
        }
    }

    // Location Users methods
    public async Task<SupabaseResult<List<SupabaseLocationUser>>> GetLocationUsersAsync(Guid locationId)
    {
        try
        {
            var client = await SupabaseClientProvider.GetClientAsync();

            var response = await client
                .From<SupabaseLocationUser>()
                .Filter("location_id", Operator.Equals, locationId.ToString())
                .Get();

            var locationUsers = response?.Models ?? new List<SupabaseLocationUser>();
            Debug.WriteLine($"[Supabase] ✓ Retrieved {locationUsers.Count} users for location {locationId}");
            return SupabaseResult<List<SupabaseLocationUser>>.Success(locationUsers);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Supabase] ❌ GetLocationUsersAsync error: {ex.Message}");
            return SupabaseResult<List<SupabaseLocationUser>>.Failure($"Failed to get location users: {ex.Message}");
        }
    }

    public async Task<SupabaseResult<SupabaseLocationUser?>> AddLocationUserAsync(SupabaseLocationUser locationUser)
    {
        return await InsertAsync(locationUser);
    }

    public async Task<SupabaseResult<SupabaseLocationUser?>> UpdateLocationUserAsync(SupabaseLocationUser locationUser)
    {
        return await UpdateAsync(locationUser.Id, locationUser);
    }

    public async Task<SupabaseResult<bool>> DeleteLocationUserAsync(Guid locationUserId)
    {
        return await DeleteAsync<SupabaseLocationUser>(locationUserId);
    }
}

/// <summary>
/// Result wrapper for Supabase operations
/// </summary>
public class SupabaseResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static SupabaseResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static SupabaseResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
