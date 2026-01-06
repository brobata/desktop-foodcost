using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class RecipeProfitabilityService : IRecipeProfitabilityService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IRecipeCostCalculator _costCalculator;

    // Standard food cost targets
    private const decimal TargetFoodCostPercent = 30.0m;
    private const decimal LowProfitThreshold = 35.0m;
    private const decimal HighProfitThreshold = 25.0m;

    public RecipeProfitabilityService(
        IRecipeRepository recipeRepository,
        IRecipeCostCalculator costCalculator)
    {
        _recipeRepository = recipeRepository;
        _costCalculator = costCalculator;
    }

    public async Task<RecipeProfitabilityReport> GenerateProfitabilityReportAsync(Guid locationId)
    {
        var report = new RecipeProfitabilityReport();
        var recipes = (await _recipeRepository.GetAllRecipesAsync(locationId)).ToList();
        var profitabilityMetrics = new List<RecipeProfitabilityMetrics>();

        report.TotalRecipes = recipes.Count;

        foreach (var recipe in recipes)
        {
            // Calculate total cost
            await _costCalculator.CalculateRecipeTotalCostAsync(recipe);

            var costPerServing = recipe.Yield > 0 ? recipe.TotalCost / recipe.Yield : recipe.TotalCost;

            var metrics = new RecipeProfitabilityMetrics
            {
                RecipeId = recipe.Id,
                RecipeName = recipe.Name,
                TotalCost = recipe.TotalCost,
                CostPerServing = costPerServing,
                Category = recipe.Category ?? "Uncategorized"
            };

            // Calculate profitability if we have a suggested menu price
            if (recipe.SuggestedMenuPrice.HasValue && recipe.SuggestedMenuPrice.Value > 0)
            {
                metrics.SuggestedPrice = recipe.SuggestedMenuPrice.Value;

                // Food cost percentage = (Cost / Price) * 100
                metrics.FoodCostPercent = (costPerServing / recipe.SuggestedMenuPrice.Value) * 100;

                // Profit margin = Price - Cost
                metrics.ContributionMargin = recipe.SuggestedMenuPrice.Value - costPerServing;

                // Profit margin percentage = ((Price - Cost) / Price) * 100
                metrics.ProfitMargin = (metrics.ContributionMargin / recipe.SuggestedMenuPrice.Value) * 100;

                // Classify profitability level
                metrics.ProfitabilityLevel = metrics.FoodCostPercent switch
                {
                    < HighProfitThreshold => ProfitabilityLevel.High,
                    <= LowProfitThreshold => ProfitabilityLevel.Moderate,
                    _ => ProfitabilityLevel.Low
                };
            }
            else
            {
                metrics.ProfitabilityLevel = ProfitabilityLevel.NoPricingData;
                metrics.FoodCostPercent = 0;
            }

            metrics.Recommendation = GetRecommendation(metrics);
            profitabilityMetrics.Add(metrics);
        }

        // Rank recipes by profitability (best profit margin first)
        var rankedRecipes = profitabilityMetrics
            .Where(m => m.ProfitabilityLevel != ProfitabilityLevel.NoPricingData)
            .OrderByDescending(m => m.ProfitMargin ?? 0)
            .ThenBy(m => m.FoodCostPercent)
            .ToList();

        for (int i = 0; i < rankedRecipes.Count; i++)
        {
            rankedRecipes[i].Rank = i + 1;
        }

        // Add recipes without pricing data at the end
        var noPricingRecipes = profitabilityMetrics
            .Where(m => m.ProfitabilityLevel == ProfitabilityLevel.NoPricingData)
            .ToList();

        report.Recipes = rankedRecipes.Concat(noPricingRecipes).ToList();

        // Calculate averages (only for recipes with pricing)
        var recipesWithPricing = rankedRecipes.ToList();
        if (recipesWithPricing.Any())
        {
            report.AverageFoodCostPercent = recipesWithPricing.Average(r => r.FoodCostPercent);
            report.AverageContributionMargin = recipesWithPricing.Average(r => r.ContributionMargin ?? 0);
        }

        // Generate summary
        report.Summary = new RecipeProfitabilitySummary
        {
            HighProfitabilityCount = profitabilityMetrics.Count(r => r.ProfitabilityLevel == ProfitabilityLevel.High),
            ModerateProfitabilityCount = profitabilityMetrics.Count(r => r.ProfitabilityLevel == ProfitabilityLevel.Moderate),
            LowProfitabilityCount = profitabilityMetrics.Count(r => r.ProfitabilityLevel == ProfitabilityLevel.Low),
            NoPricingDataCount = profitabilityMetrics.Count(r => r.ProfitabilityLevel == ProfitabilityLevel.NoPricingData),
            TotalRecipeCosts = profitabilityMetrics.Sum(r => r.TotalCost),
            TotalPotentialRevenue = recipesWithPricing.Sum(r => r.SuggestedPrice ?? 0),
            TotalProfitMargin = recipesWithPricing.Sum(r => r.ContributionMargin ?? 0)
        };

        return report;
    }

    public async Task<List<RecipeProfitabilityMetrics>> GetTopProfitableRecipesAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateProfitabilityReportAsync(locationId);
        return report.Recipes
            .Where(r => r.ProfitabilityLevel != ProfitabilityLevel.NoPricingData)
            .OrderByDescending(r => r.ProfitMargin ?? 0)
            .Take(count)
            .ToList();
    }

    public async Task<List<RecipeProfitabilityMetrics>> GetLeastProfitableRecipesAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateProfitabilityReportAsync(locationId);
        return report.Recipes
            .Where(r => r.ProfitabilityLevel != ProfitabilityLevel.NoPricingData)
            .OrderBy(r => r.ProfitMargin ?? 0)
            .Take(count)
            .ToList();
    }

    private string GetRecommendation(RecipeProfitabilityMetrics metrics)
    {
        return metrics.ProfitabilityLevel switch
        {
            ProfitabilityLevel.High =>
                $"💚 Excellent profitability! Food cost of {metrics.FoodCostPercent:F1}% is below target. Consider featuring this recipe prominently.",

            ProfitabilityLevel.Moderate =>
                $"💛 Good profitability. Food cost of {metrics.FoodCostPercent:F1}% is within acceptable range. Consider small price increases or cost reductions.",

            ProfitabilityLevel.Low =>
                $"❤️ Low profitability. Food cost of {metrics.FoodCostPercent:F1}% is above target. Reduce ingredient costs, increase price, or consider removing from menu.",

            ProfitabilityLevel.NoPricingData =>
                "⚠️ No pricing data available. Add a suggested menu price to analyze profitability.",

            _ => "Analyze further"
        };
    }
}
