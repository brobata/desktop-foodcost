using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IDeletedItemRepository
{
    Task<DeletedItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<DeletedItem>> GetAllAsync(Guid locationId);
    Task<IEnumerable<DeletedItem>> GetByTypeAsync(Guid locationId, DeletedItemType type);
    Task<DeletedItem> AddAsync(DeletedItem deletedItem);
    Task DeleteAsync(Guid id); // Permanently delete
    Task DeleteExpiredAsync(); // Clean up expired items
}
