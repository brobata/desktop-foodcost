using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Repositories;

public interface IWasteRecordRepository
{
    Task<WasteRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<WasteRecord>> GetAllAsync(Guid locationId);
    Task<IEnumerable<WasteRecord>> GetByDateRangeAsync(Guid locationId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<WasteRecord>> GetByIngredientIdAsync(Guid ingredientId);
    Task<WasteRecord> AddAsync(WasteRecord wasteRecord);
    Task<WasteRecord> UpdateAsync(WasteRecord wasteRecord);
    Task DeleteAsync(Guid id);
}
