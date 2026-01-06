using Dfc.Core.Models;
using System;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IAutoSaveService
{
    /// <summary>
    /// Save a draft of the current item being edited
    /// </summary>
    Task<DraftItem> SaveDraftAsync<T>(T item, DraftType draftType, Guid? originalItemId = null) where T : class;

    /// <summary>
    /// Load a draft by ID
    /// </summary>
    Task<T?> LoadDraftAsync<T>(Guid draftId) where T : class;

    /// <summary>
    /// Get draft for an item being edited
    /// </summary>
    Task<DraftItem?> GetDraftForItemAsync(Guid originalItemId);

    /// <summary>
    /// Delete a draft after successful save
    /// </summary>
    Task DeleteDraftAsync(Guid draftId);

    /// <summary>
    /// Get all drafts of a specific type
    /// </summary>
    Task<System.Collections.Generic.List<DraftItem>> GetDraftsByTypeAsync(DraftType draftType);

    /// <summary>
    /// Clean up old drafts (default: 30 days)
    /// </summary>
    Task CleanupOldDraftsAsync(int daysOld = 30);
}
