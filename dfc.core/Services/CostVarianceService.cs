using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class CostVarianceService : ICostVarianceService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public CostVarianceService(
        IIngredientRepository ingredientRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _ingredientRepository = ingredientRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<CostVarianceReport> GenerateVarianceReportAsync(Guid locationId, int daysBack = 30)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        var report = new CostVarianceReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var variances = new List<IngredientVariance>();

        foreach (var ingredient in ingredients)
        {
            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredient.Id);
            var periodHistory = priceHistory
                .Where(ph => ph.RecordedDate >= startDate && ph.RecordedDate <= endDate)
                .OrderBy(ph => ph.RecordedDate)
                .ToList();

            if (!periodHistory.Any())
                continue;

            // Expected cost is the average at the start of the period
            var startOfPeriodPrices = priceHistory
                .Where(ph => ph.RecordedDate <= startDate)
                .OrderByDescending(ph => ph.RecordedDate)
                .Take(3)
                .ToList();

            decimal expectedCost;
            if (startOfPeriodPrices.Any())
            {
                expectedCost = startOfPeriodPrices.Average(p => p.Price);
            }
            else
            {
                // If no history before period, use first price in period
                expectedCost = periodHistory.First().Price;
            }

            // Actual cost is current price
            var actualCost = ingredient.CurrentPrice;

            var variance = actualCost - expectedCost;
            var variancePercent = expectedCost > 0
                ? (variance / expectedCost) * 100
                : 0;

            var varianceType = Math.Abs(variancePercent) <= 2
                ? VarianceType.OnTarget
                : variance < 0
                    ? VarianceType.Favorable
                    : VarianceType.Unfavorable;

            var lastPriceChange = periodHistory.Any()
                ? periodHistory.Last().RecordedDate
                : ingredient.ModifiedAt;

            var analysis = GenerateVarianceAnalysis(variancePercent, varianceType, periodHistory.Count);

            variances.Add(new IngredientVariance
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Category = ingredient.Category ?? "Uncategorized",
                ExpectedCost = expectedCost,
                ActualCost = actualCost,
                Variance = variance,
                VariancePercent = variancePercent,
                VarianceType = varianceType,
                VendorName = ingredient.VendorName ?? "Unknown",
                Analysis = analysis,
                LastPriceChange = lastPriceChange
            });
        }

        report.IngredientsAnalyzed = variances.Count;
        report.TotalExpectedCost = variances.Sum(v => v.ExpectedCost);
        report.TotalActualCost = variances.Sum(v => v.ActualCost);
        report.TotalVariance = variances.Sum(v => v.Variance);
        report.TotalVariancePercent = report.TotalExpectedCost > 0
            ? (report.TotalVariance / report.TotalExpectedCost) * 100
            : 0;
        report.FavorableCount = variances.Count(v => v.VarianceType == VarianceType.Favorable);
        report.UnfavorableCount = variances.Count(v => v.VarianceType == VarianceType.Unfavorable);

        // Sort by largest unfavorable variance first
        report.Variances = variances
            .OrderBy(v => v.VarianceType)
            .ThenByDescending(v => Math.Abs(v.Variance))
            .ToList();

        return report;
    }

    public async Task<List<IngredientVariance>> GetTopVariancesAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateVarianceReportAsync(locationId);
        return report.Variances
            .Where(v => v.VarianceType == VarianceType.Unfavorable)
            .OrderByDescending(v => Math.Abs(v.Variance))
            .Take(count)
            .ToList();
    }

    private string GenerateVarianceAnalysis(decimal variancePercent, VarianceType varianceType, int priceChangeCount)
    {
        return varianceType switch
        {
            VarianceType.Favorable =>
                $"💚 Favorable variance of {Math.Abs(variancePercent):F1}%. Cost decreased from expected. {GetPriceChangeMessage(priceChangeCount)}",

            VarianceType.Unfavorable =>
                $"❤️ Unfavorable variance of {variancePercent:F1}%. Cost increased from expected. {GetUnfavorableRecommendation(variancePercent)} {GetPriceChangeMessage(priceChangeCount)}",

            VarianceType.OnTarget =>
                $"✓ On target. Cost within 2% of expected. {GetPriceChangeMessage(priceChangeCount)}",

            _ => "No variance analysis available."
        };
    }

    private string GetUnfavorableRecommendation(decimal variancePercent)
    {
        if (variancePercent > 20)
            return "Consider switching vendors or renegotiating prices immediately.";
        if (variancePercent > 10)
            return "Investigate alternative vendors or suppliers.";
        return "Monitor closely for continued increases.";
    }

    private string GetPriceChangeMessage(int priceChangeCount)
    {
        return priceChangeCount switch
        {
            0 => "No price changes recorded in period.",
            1 => "1 price change in period.",
            _ => $"{priceChangeCount} price changes in period."
        };
    }
}
