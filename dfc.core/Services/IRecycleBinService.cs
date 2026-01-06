using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IRecycleBinService
{
    Task<DeletedItem> MoveToRecycleBinAsync<T>(T entity, DeletedItemType type, Guid locationId) where T : BaseEntity;
    Task<T?> RestoreAsync<T>(Guid deletedItemId) where T : BaseEntity;
    Task PermanentlyDeleteAsync(Guid deletedItemId);
    Task<List<DeletedItem>> GetDeletedItemsAsync(Guid locationId);
    Task<List<DeletedItem>> GetDeletedItemsByTypeAsync(Guid locationId, DeletedItemType type);
    Task CleanupExpiredItemsAsync();
    Task EmptyRecycleBinAsync(Guid locationId);
}
