using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class RecipeCostTrendService : IRecipeCostTrendService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;
    private readonly IRecipeCostCalculator _costCalculator;

    public RecipeCostTrendService(
        IRecipeRepository recipeRepository,
        IPriceHistoryRepository priceHistoryRepository,
        IRecipeCostCalculator costCalculator)
    {
        _recipeRepository = recipeRepository;
        _priceHistoryRepository = priceHistoryRepository;
        _costCalculator = costCalculator;
    }

    public async Task<RecipeCostTrend> GetCostTrendAsync(Guid recipeId, int daysBack = 90)
    {
        var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);
        if (recipe == null)
        {
            throw new ArgumentException($"Recipe with ID {recipeId} not found");
        }

        var dataPoints = await GetHistoricalCostDataAsync(recipeId, daysBack);

        var trend = new RecipeCostTrend
        {
            RecipeId = recipeId,
            RecipeName = recipe.Name,
            CurrentCost = recipe.TotalCost,
            DataPoints = dataPoints
        };

        if (dataPoints.Any())
        {
            trend.MinCost = dataPoints.Min(dp => dp.Cost);
            trend.MaxCost = dataPoints.Max(dp => dp.Cost);
            trend.AverageCost = dataPoints.Average(dp => dp.Cost);

            // Calculate trend direction
            var oldestCost = dataPoints.First().Cost;
            var newestCost = dataPoints.Last().Cost;

            if (oldestCost > 0)
            {
                trend.CostChangePercent = ((newestCost - oldestCost) / oldestCost) * 100;
            }

            // Determine direction
            trend.Direction = DetermineTrendDirection(dataPoints);

            // Generate summary
            trend.Summary = GenerateSummary(trend);
        }

        return trend;
    }

    public async Task<List<RecipeCostDataPoint>> GetHistoricalCostDataAsync(Guid recipeId, int daysBack = 90)
    {
        var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);
        if (recipe == null || recipe.RecipeIngredients == null)
        {
            return new List<RecipeCostDataPoint>();
        }

        var startDate = DateTime.UtcNow.AddDays(-daysBack);
        var dataPoints = new List<RecipeCostDataPoint>();

        // Get all unique dates where any ingredient had a price change
        var allPriceChangeDates = new HashSet<DateTime>();

        foreach (var ri in recipe.RecipeIngredients)
        {
            // Skip ingredients that aren't matched to database yet
            if (!ri.IngredientId.HasValue) continue;

            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ri.IngredientId.Value);
            var relevantHistory = priceHistory
                .Where(ph => ph.RecordedDate >= startDate)
                .OrderBy(ph => ph.RecordedDate);

            foreach (var ph in relevantHistory)
            {
                allPriceChangeDates.Add(ph.RecordedDate.Date);
            }
        }

        // Calculate recipe cost at each price change date
        foreach (var date in allPriceChangeDates.OrderBy(d => d))
        {
            var recipeCostAtDate = await CalculateRecipeCostAtDate(recipe, date);
            dataPoints.Add(new RecipeCostDataPoint
            {
                Date = date,
                Cost = recipeCostAtDate
            });
        }

        // Add current cost as final data point
        if (!dataPoints.Any() || dataPoints.Last().Date.Date != DateTime.UtcNow.Date)
        {
            await _costCalculator.CalculateRecipeTotalCostAsync(recipe);
            dataPoints.Add(new RecipeCostDataPoint
            {
                Date = DateTime.UtcNow,
                Cost = recipe.TotalCost
            });
        }

        return dataPoints;
    }

    private async Task<decimal> CalculateRecipeCostAtDate(Recipe recipe, DateTime date)
    {
        decimal totalCost = 0;

        foreach (var ri in recipe.RecipeIngredients ?? Enumerable.Empty<RecipeIngredient>())
        {
            // Skip ingredients that aren't matched to database yet
            if (!ri.IngredientId.HasValue) continue;

            // Get the price that was effective on the given date
            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ri.IngredientId.Value);
            var effectivePrice = priceHistory
                .Where(ph => ph.RecordedDate <= date)
                .OrderByDescending(ph => ph.RecordedDate)
                .FirstOrDefault();

            if (effectivePrice != null && ri.Ingredient != null)
            {
                // Temporarily set ingredient price to historical value
                var originalPrice = ri.Ingredient.CurrentPrice;
                ri.Ingredient.CurrentPrice = effectivePrice.Price;

                // Calculate cost for this ingredient
                var cost = await _costCalculator.CalculateIngredientCostAsync(ri);
                totalCost += cost;

                // Restore original price
                ri.Ingredient.CurrentPrice = originalPrice;
            }
        }

        return totalCost;
    }

    private TrendDirection DetermineTrendDirection(List<RecipeCostDataPoint> dataPoints)
    {
        if (dataPoints.Count < 2)
        {
            return TrendDirection.Stable;
        }

        var oldestCost = dataPoints.First().Cost;
        var newestCost = dataPoints.Last().Cost;
        var changePercent = oldestCost > 0 ? ((newestCost - oldestCost) / oldestCost) * 100 : 0;

        // Check volatility (if there are large swings)
        var costs = dataPoints.Select(dp => dp.Cost).ToList();
        var average = costs.Average();
        var variance = costs.Select(c => Math.Pow((double)(c - average), 2)).Average();
        var stdDev = (decimal)Math.Sqrt(variance);
        var coefficientOfVariation = average > 0 ? (stdDev / average) * 100 : 0;

        if (coefficientOfVariation > 15) // More than 15% variation
        {
            return TrendDirection.Volatile;
        }

        if (Math.Abs(changePercent) < 5) // Less than 5% change
        {
            return TrendDirection.Stable;
        }

        return changePercent > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing;
    }

    private string GenerateSummary(RecipeCostTrend trend)
    {
        return trend.Direction switch
        {
            TrendDirection.Increasing => $"Cost has increased {trend.CostChangePercent:F1}% over the period",
            TrendDirection.Decreasing => $"Cost has decreased {Math.Abs(trend.CostChangePercent ?? 0):F1}% over the period",
            TrendDirection.Volatile => $"Cost has been volatile, ranging from {trend.MinCost:C2} to {trend.MaxCost:C2}",
            _ => "Cost has remained stable"
        };
    }
}
