using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class PriceAlertService : IPriceAlertService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;
    private const decimal DefaultThresholdPercent = 10.0m;

    public PriceAlertService(
        IIngredientRepository ingredientRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _ingredientRepository = ingredientRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<List<PriceAlertNotification>> GetActiveAlertsAsync(Guid locationId)
    {
        return await CheckPriceChangesAsync(locationId, 30);
    }

    public async Task<List<PriceAlertNotification>> CheckPriceChangesAsync(Guid locationId, int daysBack = 7)
    {
        var alerts = new List<PriceAlertNotification>();
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var startDate = DateTime.UtcNow.AddDays(-daysBack);

        foreach (var ingredient in ingredients)
        {
            var alert = await CheckIngredientPriceChangeAsync(ingredient.Id, startDate);
            if (alert != null)
            {
                alerts.Add(alert);
            }
        }

        return alerts.OrderByDescending(a => a.Severity)
                     .ThenByDescending(a => Math.Abs(a.ChangePercent))
                     .ToList();
    }

    public async Task<PriceAlertNotification?> CheckIngredientPriceChangeAsync(Guid ingredientId)
    {
        return await CheckIngredientPriceChangeAsync(ingredientId, DateTime.UtcNow.AddDays(-7));
    }

    private async Task<PriceAlertNotification?> CheckIngredientPriceChangeAsync(Guid ingredientId, DateTime sinceDate)
    {
        var ingredient = await _ingredientRepository.GetByIdAsync(ingredientId);
        if (ingredient == null)
        {
            return null;
        }

        var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredientId);
        var recentHistory = priceHistory
            .Where(ph => ph.RecordedDate >= sinceDate)
            .OrderBy(ph => ph.RecordedDate)
            .ToList();

        if (recentHistory.Count < 2)
        {
            return null;
        }

        var oldestPrice = recentHistory.First().Price;
        var newestPrice = recentHistory.Last().Price;

        if (oldestPrice == 0)
        {
            return null;
        }

        var changePercent = ((newestPrice - oldestPrice) / oldestPrice) * 100;

        // Only alert if change is significant (> 5%)
        if (Math.Abs(changePercent) < 5)
        {
            return null;
        }

        var severity = Math.Abs(changePercent) switch
        {
            >= 25 => AlertSeverity.Critical,
            >= 10 => AlertSeverity.Warning,
            _ => AlertSeverity.Info
        };

        var direction = changePercent > 0 ? "increased" : "decreased";
        var message = $"{ingredient.Name} price has {direction} by {Math.Abs(changePercent):F1}% " +
                     $"(${oldestPrice:F2} → ${newestPrice:F2})";

        return new PriceAlertNotification
        {
            IngredientId = ingredientId,
            IngredientName = ingredient.Name,
            OldPrice = oldestPrice,
            NewPrice = newestPrice,
            ChangePercent = changePercent,
            ChangeDate = recentHistory.Last().RecordedDate,
            Severity = severity,
            Message = message
        };
    }
}
