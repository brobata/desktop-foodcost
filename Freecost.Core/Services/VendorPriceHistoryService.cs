using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class VendorPriceHistoryService : IVendorPriceHistoryService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public VendorPriceHistoryService(
        IIngredientRepository ingredientRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _ingredientRepository = ingredientRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<VendorPriceHistoryReport> GetVendorPriceHistoryAsync(Guid ingredientId, int daysBack = 90)
    {
        var ingredient = await _ingredientRepository.GetByIdAsync(ingredientId);
        if (ingredient == null)
        {
            throw new InvalidOperationException($"Ingredient with ID {ingredientId} not found");
        }

        var report = new VendorPriceHistoryReport
        {
            IngredientId = ingredientId,
            IngredientName = ingredient.Name,
            CurrentVendor = ingredient.VendorName ?? "Unknown",
            CurrentPrice = ingredient.CurrentPrice
        };

        var startDate = DateTime.UtcNow.AddDays(-daysBack);
        var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredientId);
        var recentHistory = priceHistory
            .Where(ph => ph.RecordedDate >= startDate)
            .OrderBy(ph => ph.RecordedDate)
            .ToList();

        // Group by vendor
        var vendorGroups = recentHistory
            .Where(ph => !string.IsNullOrWhiteSpace(ph.VendorName))
            .GroupBy(ph => ph.VendorName!)
            .ToList();

        foreach (var vendorGroup in vendorGroups)
        {
            var vendorPrices = vendorGroup.OrderBy(ph => ph.RecordedDate).ToList();

            var pricePoints = new List<VendorPricePoint>();
            decimal? previousPrice = null;

            foreach (var priceRecord in vendorPrices)
            {
                var changePercent = previousPrice.HasValue && previousPrice.Value > 0
                    ? ((priceRecord.Price - previousPrice.Value) / previousPrice.Value) * 100
                    : (decimal?)null;

                pricePoints.Add(new VendorPricePoint
                {
                    Date = priceRecord.RecordedDate,
                    Price = priceRecord.Price,
                    ChangePercent = changePercent
                });

                previousPrice = priceRecord.Price;
            }

            var prices = vendorPrices.Select(vp => vp.Price).ToList();
            var avgPrice = prices.Average();
            var minPrice = prices.Min();
            var maxPrice = prices.Max();

            // Calculate price stability (coefficient of variation)
            var stdDev = CalculateStandardDeviation(prices);
            var stability = avgPrice > 0 ? (stdDev / avgPrice) * 100 : 0;

            var vendorHistory = new VendorPriceHistory
            {
                VendorName = vendorGroup.Key,
                CurrentPrice = vendorPrices.Last().Price,
                AveragePrice = avgPrice,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PriceStability = stability,
                PriceChangeCount = pricePoints.Count(pp => pp.ChangePercent.HasValue && Math.Abs(pp.ChangePercent.Value) > 0.5m),
                PricePoints = pricePoints,
                Trend = DeterminePriceTrend(pricePoints)
            };

            report.VendorHistories.Add(vendorHistory);
        }

        return report;
    }

    public async Task<List<VendorPriceTrend>> GetVendorPriceTrendsAsync(Guid locationId, int daysBack = 90)
    {
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var startDate = DateTime.UtcNow.AddDays(-daysBack);
        var trends = new Dictionary<string, VendorPriceTrend>();

        foreach (var ingredient in ingredients)
        {
            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredient.Id);
            var recentHistory = priceHistory
                .Where(ph => ph.RecordedDate >= startDate && !string.IsNullOrWhiteSpace(ph.VendorName))
                .OrderBy(ph => ph.RecordedDate)
                .ToList();

            var vendorGroups = recentHistory.GroupBy(ph => ph.VendorName!);

            foreach (var vendorGroup in vendorGroups)
            {
                if (!trends.ContainsKey(vendorGroup.Key))
                {
                    trends[vendorGroup.Key] = new VendorPriceTrend
                    {
                        VendorName = vendorGroup.Key
                    };
                }

                var trend = trends[vendorGroup.Key];
                trend.IngredientsTracked++;

                var vendorPrices = vendorGroup.OrderBy(ph => ph.RecordedDate).ToList();
                if (vendorPrices.Count >= 2)
                {
                    var oldestPrice = vendorPrices.First().Price;
                    var newestPrice = vendorPrices.Last().Price;
                    var changePercent = oldestPrice > 0
                        ? ((newestPrice - oldestPrice) / oldestPrice) * 100
                        : 0;

                    trend.AveragePriceIncrease += changePercent;

                    if (Math.Abs(changePercent) < 2)
                        trend.StablePriceCount++;
                    else if (changePercent > 0)
                        trend.PriceIncreaseCount++;
                    else
                        trend.PriceDecreaseCount++;
                }
            }
        }

        // Calculate averages and reliability
        foreach (var trend in trends.Values)
        {
            if (trend.IngredientsTracked > 0)
            {
                trend.AveragePriceIncrease /= trend.IngredientsTracked;
                trend.Reliability = DetermineVendorReliability(trend);
            }
        }

        return trends.Values
            .OrderByDescending(t => t.IngredientsTracked)
            .ToList();
    }

    public async Task<VendorSwitchRecommendation?> GetVendorSwitchRecommendationAsync(Guid ingredientId)
    {
        var report = await GetVendorPriceHistoryAsync(ingredientId, 90);

        if (report.VendorHistories.Count < 2)
        {
            return null; // Need at least 2 vendors to compare
        }

        // Find the best alternative vendor (lowest average price with good stability)
        var alternatives = report.VendorHistories
            .Where(vh => vh.VendorName != report.CurrentVendor)
            .OrderBy(vh => vh.AveragePrice ?? decimal.MaxValue)
            .ThenBy(vh => vh.PriceStability ?? 100)
            .ToList();

        if (!alternatives.Any())
        {
            return null;
        }

        var bestAlternative = alternatives.First();
        var currentVendorHistory = report.VendorHistories
            .FirstOrDefault(vh => vh.VendorName == report.CurrentVendor);

        var currentVendorAvgPrice = currentVendorHistory?.AveragePrice ?? report.CurrentPrice;
        var savings = currentVendorAvgPrice - (bestAlternative.AveragePrice ?? 0);
        var savingsPercent = currentVendorAvgPrice > 0
            ? (savings / currentVendorAvgPrice) * 100
            : 0;

        // Only recommend if savings are significant (> 5%)
        if (savingsPercent < 5)
        {
            return null;
        }

        var isStrongRecommendation = savingsPercent > 15 && (bestAlternative.PriceStability ?? 100) < 15;

        var reason = BuildRecommendationReason(savingsPercent, bestAlternative.PriceStability ?? 0, bestAlternative.Trend);

        return new VendorSwitchRecommendation
        {
            IngredientId = ingredientId,
            IngredientName = report.IngredientName,
            CurrentVendor = report.CurrentVendor,
            CurrentPrice = currentVendorAvgPrice,
            RecommendedVendor = bestAlternative.VendorName,
            RecommendedVendorPrice = bestAlternative.AveragePrice ?? 0,
            PotentialSavings = savings,
            SavingsPercent = savingsPercent,
            Reason = reason,
            RecommendedVendorStability = bestAlternative.PriceStability ?? 0,
            IsStrongRecommendation = isStrongRecommendation
        };
    }

    private decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (!values.Any())
            return 0;

        var average = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow((double)(v - average), 2));
        return (decimal)Math.Sqrt(sumOfSquares / values.Count);
    }

    private PriceTrendDirection DeterminePriceTrend(List<VendorPricePoint> pricePoints)
    {
        if (pricePoints.Count < 2)
            return PriceTrendDirection.Unknown;

        var prices = pricePoints.Select(pp => pp.Price).ToList();
        var average = prices.Average();
        var stdDev = CalculateStandardDeviation(prices);
        var coefficientOfVariation = average > 0 ? (stdDev / average) * 100 : 0;

        if (coefficientOfVariation > 15)
            return PriceTrendDirection.Volatile;

        var oldestPrice = pricePoints.First().Price;
        var newestPrice = pricePoints.Last().Price;
        var changePercent = oldestPrice > 0
            ? ((newestPrice - oldestPrice) / oldestPrice) * 100
            : 0;

        if (Math.Abs(changePercent) < 5)
            return PriceTrendDirection.Stable;

        return changePercent > 0
            ? PriceTrendDirection.Increasing
            : PriceTrendDirection.Decreasing;
    }

    private VendorReliability DetermineVendorReliability(VendorPriceTrend trend)
    {
        if (trend.IngredientsTracked == 0)
            return VendorReliability.Unknown;

        var stablePercent = (trend.StablePriceCount / (decimal)trend.IngredientsTracked) * 100;
        var increasePercent = (trend.PriceIncreaseCount / (decimal)trend.IngredientsTracked) * 100;

        if (stablePercent >= 70 && trend.AveragePriceIncrease < 5)
            return VendorReliability.Excellent;

        if (stablePercent >= 50 && trend.AveragePriceIncrease < 10)
            return VendorReliability.Good;

        if (stablePercent >= 30)
            return VendorReliability.Fair;

        return VendorReliability.Poor;
    }

    private string BuildRecommendationReason(decimal savingsPercent, decimal stability, PriceTrendDirection trend)
    {
        var reasons = new List<string>();

        if (savingsPercent > 20)
            reasons.Add($"significant cost savings ({savingsPercent:F1}%)");
        else if (savingsPercent > 10)
            reasons.Add($"moderate cost savings ({savingsPercent:F1}%)");
        else
            reasons.Add($"cost savings ({savingsPercent:F1}%)");

        if (stability < 10)
            reasons.Add("excellent price stability");
        else if (stability < 15)
            reasons.Add("good price stability");

        if (trend == PriceTrendDirection.Decreasing)
            reasons.Add("decreasing price trend");
        else if (trend == PriceTrendDirection.Stable)
            reasons.Add("stable pricing");

        return string.Join(", ", reasons);
    }
}
