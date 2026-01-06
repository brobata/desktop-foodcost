using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class AutoSaveService : IAutoSaveService
{
    private readonly IDraftItemRepository _draftRepository;

    public AutoSaveService(IDraftItemRepository draftRepository)
    {
        _draftRepository = draftRepository;
    }

    public async Task<DraftItem> SaveDraftAsync<T>(T item, DraftType draftType, Guid? originalItemId = null) where T : class
    {
        var serializedData = JsonSerializer.Serialize(item);
        var draftName = GetDraftName(item, draftType);

        // Check if draft already exists for this item
        DraftItem? existingDraft = null;
        if (originalItemId.HasValue)
        {
            existingDraft = await _draftRepository.GetByOriginalItemIdAsync(originalItemId.Value);
        }

        if (existingDraft != null)
        {
            // Update existing draft
            existingDraft.SerializedData = serializedData;
            existingDraft.DraftName = draftName;
            existingDraft.LastSavedAt = DateTime.UtcNow;
            await _draftRepository.UpdateAsync(existingDraft);
            return existingDraft;
        }
        else
        {
            // Create new draft
            var draft = new DraftItem
            {
                DraftType = draftType,
                DraftName = draftName,
                SerializedData = serializedData,
                OriginalItemId = originalItemId,
                LastSavedAt = DateTime.UtcNow
            };

            return await _draftRepository.AddAsync(draft);
        }
    }

    public async Task<T?> LoadDraftAsync<T>(Guid draftId) where T : class
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);
        if (draft == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(draft.SerializedData);
        }
        catch
        {
            return null;
        }
    }

    public async Task<DraftItem?> GetDraftForItemAsync(Guid originalItemId)
    {
        return await _draftRepository.GetByOriginalItemIdAsync(originalItemId);
    }

    public async Task DeleteDraftAsync(Guid draftId)
    {
        await _draftRepository.DeleteAsync(draftId);
    }

    public async Task<List<DraftItem>> GetDraftsByTypeAsync(DraftType draftType)
    {
        return await _draftRepository.GetByTypeAsync(draftType);
    }

    public async Task CleanupOldDraftsAsync(int daysOld = 30)
    {
        await _draftRepository.DeleteOldDraftsAsync(daysOld);
    }

    private string GetDraftName<T>(T item, DraftType draftType)
    {
        return draftType switch
        {
            DraftType.Recipe when item is Recipe recipe => $"Recipe: {recipe.Name ?? "Untitled"}",
            DraftType.Ingredient when item is Ingredient ingredient => $"Ingredient: {ingredient.Name ?? "Untitled"}",
            DraftType.Entree when item is Entree entree => $"Entree: {entree.Name ?? "Untitled"}",
            _ => $"{draftType} Draft"
        };
    }
}
