using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class SeasonalTrendService : ISeasonalTrendService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public SeasonalTrendService(
        IIngredientRepository ingredientRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _ingredientRepository = ingredientRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<SeasonalTrendReport> AnalyzeSeasonalTrendsAsync(Guid locationId, int yearsBack = 2)
    {
        var report = new SeasonalTrendReport
        {
            YearsAnalyzed = yearsBack
        };

        var startDate = DateTime.UtcNow.AddYears(-yearsBack);
        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var ingredientAnalyses = new List<IngredientSeasonalAnalysis>();

        foreach (var ingredient in ingredients)
        {
            var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredient.Id);
            var relevantHistory = priceHistory
                .Where(ph => ph.RecordedDate >= startDate)
                .ToList();

            if (relevantHistory.Count < 8) // Need at least 8 data points for seasonal analysis
                continue;

            var analysis = AnalyzeIngredientSeasonality(ingredient, relevantHistory);
            ingredientAnalyses.Add(analysis);
        }

        report.Ingredients = ingredientAnalyses;

        // Calculate summary
        report.Summary = new SeasonalSummary
        {
            IngredientsWithStrongSeasonality = ingredientAnalyses.Count(i => i.HasStrongSeasonalPattern),
            IngredientsWithWeakSeasonality = ingredientAnalyses.Count(i => !i.HasStrongSeasonalPattern),
            AverageSeasonalVariance = ingredientAnalyses.Any()
                ? ingredientAnalyses.Average(i => i.SeasonalVariance)
                : 0
        };

        // Find overall most/least expensive seasons
        var seasonCosts = new Dictionary<Season, List<decimal>>();
        foreach (var ingredient in ingredientAnalyses)
        {
            foreach (var seasonData in ingredient.SeasonalData)
            {
                if (!seasonCosts.ContainsKey(seasonData.Season))
                    seasonCosts[seasonData.Season] = new List<decimal>();

                seasonCosts[seasonData.Season].Add(seasonData.AveragePrice);
            }
        }

        if (seasonCosts.Any())
        {
            var seasonAverages = seasonCosts.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Average()
            );

            report.Summary.MostExpensiveSeason = seasonAverages.OrderByDescending(kvp => kvp.Value).First().Key;
            report.Summary.LeastExpensiveSeason = seasonAverages.OrderBy(kvp => kvp.Value).First().Key;
        }

        return report;
    }

    public async Task<List<IngredientSeasonalPattern>> GetSeasonalPatternsAsync(Guid ingredientId)
    {
        var ingredient = await _ingredientRepository.GetByIdAsync(ingredientId);
        if (ingredient == null)
            return new List<IngredientSeasonalPattern>();

        var priceHistory = await _priceHistoryRepository.GetByIngredientIdAsync(ingredientId);
        var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
        var relevantHistory = priceHistory.Where(ph => ph.RecordedDate >= twoYearsAgo).ToList();

        if (relevantHistory.Count < 8)
            return new List<IngredientSeasonalPattern>();

        var analysis = AnalyzeIngredientSeasonality(ingredient, relevantHistory);

        var pattern = new IngredientSeasonalPattern
        {
            Pattern = analysis.HasStrongSeasonalPattern ? "Strong Seasonal Variation" : "Weak Seasonal Variation",
            Description = BuildPatternDescription(analysis),
            BestBuyingSeasons = analysis.SeasonalData
                .OrderBy(sd => sd.AveragePrice)
                .Take(2)
                .Select(sd => sd.Season)
                .ToList(),
            AvoidSeasons = analysis.SeasonalData
                .OrderByDescending(sd => sd.AveragePrice)
                .Take(2)
                .Select(sd => sd.Season)
                .ToList()
        };

        return new List<IngredientSeasonalPattern> { pattern };
    }

    public async Task<List<SeasonalRecommendation>> GetSeasonalRecommendationsAsync(Guid locationId)
    {
        var report = await AnalyzeSeasonalTrendsAsync(locationId);
        var currentSeason = GetCurrentSeason();
        var recommendations = new List<SeasonalRecommendation>();

        foreach (var ingredient in report.Ingredients.Where(i => i.HasStrongSeasonalPattern))
        {
            var currentSeasonData = ingredient.SeasonalData.FirstOrDefault(sd => sd.Season == currentSeason);
            var bestSeasonData = ingredient.SeasonalData.OrderBy(sd => sd.AveragePrice).First();

            if (currentSeasonData == null)
                continue;

            var potentialSavings = currentSeasonData.AveragePrice - bestSeasonData.AveragePrice;

            string recommendation;
            string actionItem;

            if (ingredient.LowestPriceSeason == currentSeason)
            {
                recommendation = $"✓ Currently in best buying season. Prices are typically {ingredient.SeasonalVariance:F0}% lower than peak season.";
                actionItem = "Consider stocking up if storage allows.";
            }
            else if (ingredient.HighestPriceSeason == currentSeason)
            {
                recommendation = $"⚠ Currently in most expensive season. Prices are typically {ingredient.SeasonalVariance:F0}% higher.";
                actionItem = $"Wait for {ingredient.LowestPriceSeason} if possible, or seek alternative suppliers.";
            }
            else
            {
                recommendation = $"Moderate pricing season. Best prices typically in {ingredient.LowestPriceSeason}.";
                actionItem = $"Plan ahead for purchasing in {ingredient.LowestPriceSeason}.";
            }

            recommendations.Add(new SeasonalRecommendation
            {
                IngredientId = ingredient.IngredientId,
                IngredientName = ingredient.IngredientName,
                Recommendation = recommendation,
                CurrentSeason = currentSeason,
                BestBuyingSeason = ingredient.LowestPriceSeason,
                PotentialSavings = Math.Max(0, potentialSavings),
                ActionItem = actionItem
            });
        }

        return recommendations.OrderByDescending(r => r.PotentialSavings).ToList();
    }

    private IngredientSeasonalAnalysis AnalyzeIngredientSeasonality(Ingredient ingredient, List<PriceHistory> priceHistory)
    {
        var analysis = new IngredientSeasonalAnalysis
        {
            IngredientId = ingredient.Id,
            IngredientName = ingredient.Name,
            Category = ingredient.Category ?? "Uncategorized"
        };

        // Group prices by season
        var seasonGroups = priceHistory.GroupBy(ph => GetSeason(ph.RecordedDate));

        var seasonalData = new List<SeasonalData>();

        foreach (var seasonGroup in seasonGroups)
        {
            var prices = seasonGroup.Select(ph => ph.Price).ToList();

            seasonalData.Add(new SeasonalData
            {
                Season = seasonGroup.Key,
                AveragePrice = prices.Average(),
                MinPrice = prices.Min(),
                MaxPrice = prices.Max(),
                DataPoints = prices.Count
            });
        }

        analysis.SeasonalData = seasonalData.OrderBy(sd => sd.Season).ToList();

        if (seasonalData.Any())
        {
            var lowestSeason = seasonalData.OrderBy(sd => sd.AveragePrice).First();
            var highestSeason = seasonalData.OrderByDescending(sd => sd.AveragePrice).First();

            analysis.LowestPriceSeason = lowestSeason.Season;
            analysis.HighestPriceSeason = highestSeason.Season;

            // Calculate seasonal variance percentage
            if (lowestSeason.AveragePrice > 0)
            {
                analysis.SeasonalVariance = ((highestSeason.AveragePrice - lowestSeason.AveragePrice) / lowestSeason.AveragePrice) * 100;
            }

            // Strong seasonal pattern if variance > 15%
            analysis.HasStrongSeasonalPattern = analysis.SeasonalVariance > 15;
        }

        return analysis;
    }

    private Season GetSeason(DateTime date)
    {
        var month = date.Month;
        return month switch
        {
            3 or 4 or 5 => Season.Spring,
            6 or 7 or 8 => Season.Summer,
            9 or 10 or 11 => Season.Fall,
            _ => Season.Winter // 12, 1, 2
        };
    }

    private Season GetCurrentSeason()
    {
        return GetSeason(DateTime.UtcNow);
    }

    private string BuildPatternDescription(IngredientSeasonalAnalysis analysis)
    {
        if (!analysis.HasStrongSeasonalPattern)
        {
            return $"Prices remain relatively stable throughout the year with only {analysis.SeasonalVariance:F0}% variance between seasons.";
        }

        return $"Prices vary significantly by season ({analysis.SeasonalVariance:F0}% difference). " +
               $"Lowest prices in {analysis.LowestPriceSeason}, highest in {analysis.HighestPriceSeason}.";
    }
}
