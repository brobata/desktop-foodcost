using Dfc.Core.Models;

namespace Dfc.Core.Repositories;

public interface IImportBatchRepository
{
    /// <summary>
    /// Get recent import batches for a location
    /// </summary>
    Task<List<ImportBatch>> GetRecentAsync(Guid locationId, int count = 10);

    /// <summary>
    /// Get a specific import batch by ID, including its items
    /// </summary>
    Task<ImportBatch?> GetByIdWithItemsAsync(Guid id);

    /// <summary>
    /// Add a new import batch
    /// </summary>
    Task<ImportBatch> AddAsync(ImportBatch batch);

    /// <summary>
    /// Add items to an existing batch
    /// </summary>
    Task AddItemsAsync(Guid batchId, List<ImportBatchItem> items);

    /// <summary>
    /// Mark a batch as no longer undoable
    /// </summary>
    Task ExpireUndoAsync(Guid batchId);

    /// <summary>
    /// Get all batches that have expired undo windows
    /// </summary>
    Task<List<ImportBatch>> GetExpiredUndoBatchesAsync();

    /// <summary>
    /// Delete a batch and all its items (used after successful undo)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Check if a batch can still be undone
    /// </summary>
    Task<bool> CanUndoAsync(Guid batchId);
}
