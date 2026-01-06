using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class VendorComparisonService : IVendorComparisonService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public VendorComparisonService(
        IIngredientRepository ingredientRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _ingredientRepository = ingredientRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<VendorComparisonReport> CompareVendorsAsync(Guid locationId)
    {
        var report = new VendorComparisonReport();
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);

        report.TotalIngredients = ingredients.Count;

        // Group ingredients by vendor to create vendor summaries
        var vendorGroups = ingredients
            .Where(i => !string.IsNullOrWhiteSpace(i.VendorName))
            .GroupBy(i => i.VendorName!)
            .ToList();

        foreach (var vendorGroup in vendorGroups)
        {
            var vendorIngredients = vendorGroup.ToList();
            var totalSpend = vendorIngredients.Sum(i => i.CurrentPrice);
            var avgPrice = vendorIngredients.Any() ? vendorIngredients.Average(i => i.CurrentPrice) : 0;

            report.VendorSummaries.Add(new VendorSummary
            {
                VendorName = vendorGroup.Key,
                IngredientCount = vendorIngredients.Count,
                TotalSpend = totalSpend,
                AveragePrice = avgPrice
            });
        }

        // Rank vendors by average price
        var sortedByPrice = report.VendorSummaries.OrderBy(v => v.AveragePrice).ToList();
        for (int i = 0; i < sortedByPrice.Count; i++)
        {
            if (i == 0)
                sortedByPrice[i].PriceRanking = "Lowest";
            else if (i == sortedByPrice.Count - 1)
                sortedByPrice[i].PriceRanking = "Highest";
            else
                sortedByPrice[i].PriceRanking = "Competitive";
        }

        report.TotalCurrentCost = report.VendorSummaries.Sum(v => v.TotalSpend);

        // Find savings opportunities
        // For this simplified version, we'll identify ingredients where price history shows
        // other vendors had lower prices
        var savingsOpportunities = new List<SavingsOpportunity>();

        foreach (var ingredient in ingredients.Where(i => !string.IsNullOrWhiteSpace(i.VendorName)))
        {
            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredient.Id);

            // Look for recent price records from other vendors (if we had vendor info in price history)
            // For now, simulate by checking if ingredient has historically had lower prices
            var historicalPrices = priceHistory
                .Where(ph => ph.RecordedDate >= DateTime.UtcNow.AddDays(-90))
                .OrderByDescending(ph => ph.RecordedDate)
                .ToList();

            if (historicalPrices.Any())
            {
                var lowestHistoricalPrice = historicalPrices.Min(ph => ph.Price);

                if (lowestHistoricalPrice < ingredient.CurrentPrice &&
                    ingredient.CurrentPrice > 0)
                {
                    var savingsAmount = ingredient.CurrentPrice - lowestHistoricalPrice;
                    var savingsPercent = (savingsAmount / ingredient.CurrentPrice) * 100;

                    if (savingsPercent > 5) // Only show if savings > 5%
                    {
                        savingsOpportunities.Add(new SavingsOpportunity
                        {
                            IngredientId = ingredient.Id,
                            IngredientName = ingredient.Name,
                            CurrentVendor = ingredient.VendorName ?? "Unknown",
                            CurrentPrice = ingredient.CurrentPrice,
                            RecommendedVendor = "Historical Low Price",
                            RecommendedPrice = lowestHistoricalPrice,
                            SavingsAmount = savingsAmount,
                            SavingsPercent = savingsPercent,
                            Unit = ingredient.Unit.ToString()
                        });
                    }
                }
            }
        }

        report.TopSavingsOpportunities = savingsOpportunities
            .OrderByDescending(s => s.SavingsAmount)
            .Take(20)
            .ToList();

        report.IngredientsWithMultipleVendors = savingsOpportunities.Count;
        report.PotentialSavings = report.TopSavingsOpportunities.Sum(s => s.SavingsAmount);

        if (report.TotalCurrentCost > 0)
        {
            report.SavingsPercent = (report.PotentialSavings / report.TotalCurrentCost) * 100;
        }

        return report;
    }

    public async Task<List<IngredientVendorComparison>> GetIngredientVendorOptionsAsync(Guid ingredientId)
    {
        var comparisons = new List<IngredientVendorComparison>();
        var ingredient = await _ingredientRepository.GetByIdAsync(ingredientId);

        if (ingredient == null)
        {
            return comparisons;
        }

        var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredientId);
        var recentPrices = priceHistory
            .Where(ph => ph.RecordedDate >= DateTime.UtcNow.AddDays(-90))
            .OrderByDescending(ph => ph.RecordedDate)
            .ToList();

        var alternativeOptions = new List<AlternativeVendorOption>();

        // Group by unique prices to simulate different vendor options
        var uniquePrices = recentPrices
            .Select(ph => ph.Price)
            .Distinct()
            .Where(p => p != ingredient.CurrentPrice)
            .OrderBy(p => p)
            .Take(5);

        foreach (var price in uniquePrices)
        {
            var priceDiff = price - ingredient.CurrentPrice;
            var priceDiffPercent = ingredient.CurrentPrice > 0
                ? (priceDiff / ingredient.CurrentPrice) * 100
                : 0;

            alternativeOptions.Add(new AlternativeVendorOption
            {
                VendorName = "Alternative Vendor", // In production, this would come from price history vendor field
                Price = price,
                PriceDifference = priceDiff,
                PriceDifferencePercent = priceDiffPercent,
                IsLowerPrice = price < ingredient.CurrentPrice,
                LastUpdated = recentPrices.First(ph => ph.Price == price).RecordedDate
            });
        }

        if (alternativeOptions.Any())
        {
            comparisons.Add(new IngredientVendorComparison
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                CurrentVendor = ingredient.VendorName ?? "Current Vendor",
                CurrentPrice = ingredient.CurrentPrice,
                AlternativeVendors = alternativeOptions
            });
        }

        return comparisons;
    }

    public async Task<decimal> CalculatePotentialSavingsAsync(Guid locationId)
    {
        var report = await CompareVendorsAsync(locationId);
        return report.PotentialSavings;
    }
}
