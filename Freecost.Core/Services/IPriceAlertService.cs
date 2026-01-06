using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IPriceAlertService
{
    Task<List<PriceAlertNotification>> GetActiveAlertsAsync(Guid locationId);
    Task<List<PriceAlertNotification>> CheckPriceChangesAsync(Guid locationId, int daysBack = 7);
    Task<PriceAlertNotification?> CheckIngredientPriceChangeAsync(Guid ingredientId);
}
