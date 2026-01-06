using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Enums;

namespace Dfc.Core.Services;

public class PriceHistoryService : IPriceHistoryService
{
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public PriceHistoryService(IPriceHistoryRepository priceHistoryRepository)
    {
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task RecordPriceChangeAsync(Guid ingredientId, decimal newPrice, DateTime? recordedDate = null)
    {
        // Get the latest price to check if it actually changed
        var latestPrice = await _priceHistoryRepository.GetLatestForIngredientAsync(ingredientId);

        // Only record if price actually changed
        if (latestPrice == null || latestPrice.Price != newPrice)
        {
            var priceHistory = new PriceHistory
            {
                Id = Guid.NewGuid(),
                IngredientId = ingredientId,
                Price = newPrice,
                RecordedDate = recordedDate ?? DateTime.UtcNow,
                IsAggregated = false,
                AggregationType = AggregationType.Daily
            };

            await _priceHistoryRepository.AddAsync(priceHistory);
            await _priceHistoryRepository.SaveChangesAsync();
        }
    }

    public async Task<List<PriceHistory>> GetPriceHistoryAsync(Guid ingredientId)
    {
        return await _priceHistoryRepository.GetByIngredientIdAsync(ingredientId);
    }

    public async Task<PriceHistory?> GetLatestPriceAsync(Guid ingredientId)
    {
        return await _priceHistoryRepository.GetLatestForIngredientAsync(ingredientId);
    }

    public async Task AggregateOldPricesAsync()
    {
        var now = DateTime.UtcNow;
        var allPrices = await _priceHistoryRepository.GetAllUnaggregatedAsync();

        // Group by ingredient
        var pricesByIngredient = allPrices.GroupBy(p => p.IngredientId);

        foreach (var ingredientGroup in pricesByIngredient)
        {
            var ingredientPrices = ingredientGroup.OrderBy(p => p.RecordedDate).ToList();

            // Days 1-30: Keep individual daily prices
            // Day 31+: Compress to monthly averages
            var dailyPricesOlderThan30Days = ingredientPrices
                .Where(p => p.RecordedDate < now.AddDays(-30) && p.AggregationType == AggregationType.Daily)
                .ToList();

            if (dailyPricesOlderThan30Days.Any())
            {
                // Group by month
                var monthlyGroups = dailyPricesOlderThan30Days
                    .GroupBy(p => new { p.RecordedDate.Year, p.RecordedDate.Month });

                var monthlyAggregates = new List<PriceHistory>();

                foreach (var monthGroup in monthlyGroups)
                {
                    var avgPrice = monthGroup.Average(p => p.Price);
                    var monthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);

                    // Only aggregate if older than 30 days
                    if (monthDate < now.AddDays(-30))
                    {
                        monthlyAggregates.Add(new PriceHistory
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredientGroup.Key,
                            Price = avgPrice,
                            RecordedDate = monthDate,
                            IsAggregated = true,
                            AggregationType = AggregationType.Monthly
                        });

                        // Mark old records for deletion
                        await _priceHistoryRepository.DeleteRangeAsync(monthGroup.ToList());
                    }
                }

                if (monthlyAggregates.Any())
                {
                    await _priceHistoryRepository.AddRangeAsync(monthlyAggregates);
                }
            }

            // Months 1-3: Keep individual monthly averages
            // Month 4+: Compress to quarterly averages
            var monthlyPricesOlderThan3Months = ingredientPrices
                .Where(p => p.RecordedDate < now.AddMonths(-3) && p.AggregationType == AggregationType.Monthly)
                .ToList();

            if (monthlyPricesOlderThan3Months.Any())
            {
                // Group by quarter
                var quarterlyGroups = monthlyPricesOlderThan3Months
                    .GroupBy(p => new { p.RecordedDate.Year, Quarter = (p.RecordedDate.Month - 1) / 3 + 1 });

                var quarterlyAggregates = new List<PriceHistory>();

                foreach (var quarterGroup in quarterlyGroups)
                {
                    var avgPrice = quarterGroup.Average(p => p.Price);
                    var quarterStartMonth = (quarterGroup.Key.Quarter - 1) * 3 + 1;
                    var quarterDate = new DateTime(quarterGroup.Key.Year, quarterStartMonth, 1);

                    if (quarterDate < now.AddMonths(-3))
                    {
                        quarterlyAggregates.Add(new PriceHistory
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredientGroup.Key,
                            Price = avgPrice,
                            RecordedDate = quarterDate,
                            IsAggregated = true,
                            AggregationType = AggregationType.Quarterly
                        });

                        await _priceHistoryRepository.DeleteRangeAsync(quarterGroup.ToList());
                    }
                }

                if (quarterlyAggregates.Any())
                {
                    await _priceHistoryRepository.AddRangeAsync(quarterlyAggregates);
                }
            }

            // Quarters 1-4: Keep individual quarterly averages
            // Year 2+: Compress to yearly averages
            var quarterlyPricesOlderThan1Year = ingredientPrices
                .Where(p => p.RecordedDate < now.AddYears(-1) && p.AggregationType == AggregationType.Quarterly)
                .ToList();

            if (quarterlyPricesOlderThan1Year.Any())
            {
                // Group by year
                var yearlyGroups = quarterlyPricesOlderThan1Year
                    .GroupBy(p => p.RecordedDate.Year);

                var yearlyAggregates = new List<PriceHistory>();

                foreach (var yearGroup in yearlyGroups)
                {
                    var avgPrice = yearGroup.Average(p => p.Price);
                    var yearDate = new DateTime(yearGroup.Key, 1, 1);

                    if (yearDate < now.AddYears(-1))
                    {
                        yearlyAggregates.Add(new PriceHistory
                        {
                            Id = Guid.NewGuid(),
                            IngredientId = ingredientGroup.Key,
                            Price = avgPrice,
                            RecordedDate = yearDate,
                            IsAggregated = true,
                            AggregationType = AggregationType.Yearly
                        });

                        await _priceHistoryRepository.DeleteRangeAsync(yearGroup.ToList());
                    }
                }

                if (yearlyAggregates.Any())
                {
                    await _priceHistoryRepository.AddRangeAsync(yearlyAggregates);
                }
            }
        }

        await _priceHistoryRepository.SaveChangesAsync();
    }
}
