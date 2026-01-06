using Freecost.Core.Models;

namespace Freecost.Core.Services;

public interface IPriceHistoryService
{
    /// <summary>
    /// Records a price change for an ingredient
    /// </summary>
    Task RecordPriceChangeAsync(Guid ingredientId, decimal newPrice, DateTime? recordedDate = null);

    /// <summary>
    /// Gets all price history for an ingredient
    /// </summary>
    Task<List<PriceHistory>> GetPriceHistoryAsync(Guid ingredientId);

    /// <summary>
    /// Gets the latest price record for an ingredient
    /// </summary>
    Task<PriceHistory?> GetLatestPriceAsync(Guid ingredientId);

    /// <summary>
    /// Performs smart retention aggregation according to roadmap rules:
    /// - Days 1-30: Individual daily prices
    /// - Day 31: Compress to monthly average
    /// - Months 1-3: Individual monthly averages
    /// - Month 4: Compress to quarterly average
    /// - Quarters 1-4: Individual quarterly averages
    /// - Year 2+: Yearly averages
    /// </summary>
    Task AggregateOldPricesAsync();
}
