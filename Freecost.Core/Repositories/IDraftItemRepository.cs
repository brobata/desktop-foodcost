using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IDraftItemRepository
{
    Task<DraftItem> AddAsync(DraftItem draftItem);
    Task<DraftItem?> GetByIdAsync(Guid id);
    Task<List<DraftItem>> GetAllAsync();
    Task<List<DraftItem>> GetByTypeAsync(DraftType type);
    Task<DraftItem?> GetByOriginalItemIdAsync(Guid originalItemId);
    Task UpdateAsync(DraftItem draftItem);
    Task DeleteAsync(Guid id);
    Task DeleteOldDraftsAsync(int daysOld = 30);
}
