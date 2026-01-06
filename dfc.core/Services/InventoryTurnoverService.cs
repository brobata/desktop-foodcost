using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class InventoryTurnoverService : IInventoryTurnoverService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IEntreeRepository _entreeRepository;

    public InventoryTurnoverService(
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IEntreeRepository entreeRepository)
    {
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _entreeRepository = entreeRepository;
    }

    public async Task<InventoryTurnoverReport> GenerateTurnoverReportAsync(Guid locationId, int daysBack = 90)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        var report = new InventoryTurnoverReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var recipes = (await _recipeRepository.GetAllRecipesAsync(locationId)).ToList();
        var entrees = await _entreeRepository.GetAllAsync(locationId);

        var turnoverMetrics = new List<InventoryTurnoverMetrics>();

        foreach (var ingredient in ingredients)
        {
            // Count recipe usages
            var recipeUsageCount = recipes.Count(r =>
                r.RecipeIngredients != null &&
                r.RecipeIngredients.Any(ri => ri.IngredientId == ingredient.Id));

            // Count entree direct usages
            var entreeUsageCount = entrees.Count(e =>
                e.EntreeIngredients != null &&
                e.EntreeIngredients.Any(ei => ei.IngredientId == ingredient.Id));

            // Count entree indirect usages (through recipes)
            var entreeIndirectUsageCount = entrees.Count(e =>
                e.EntreeRecipes != null &&
                e.EntreeRecipes.Any(er =>
                    er.Recipe.RecipeIngredients != null &&
                    er.Recipe.RecipeIngredients.Any(ri => ri.IngredientId == ingredient.Id)));

            var totalUsageCount = recipeUsageCount + entreeUsageCount + entreeIndirectUsageCount;

            // Estimate quantity used (simplified - assumes average usage per recipe/entree)
            var estimatedQuantityUsed = totalUsageCount * 1.0m; // Placeholder calculation

            var estimatedValueUsed = estimatedQuantityUsed * ingredient.CurrentPrice;

            var turnoverRate = ClassifyTurnoverRate(totalUsageCount);
            var analysis = GenerateAnalysis(turnoverRate, totalUsageCount, recipeUsageCount, entreeUsageCount);
            var recommendation = GetRecommendation(turnoverRate, ingredient.CurrentPrice);

            turnoverMetrics.Add(new InventoryTurnoverMetrics
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Category = ingredient.Category ?? "Uncategorized",
                RecipeUsageCount = recipeUsageCount,
                EntreeUsageCount = entreeUsageCount + entreeIndirectUsageCount,
                TotalUsageCount = totalUsageCount,
                EstimatedQuantityUsed = estimatedQuantityUsed,
                CurrentPrice = ingredient.CurrentPrice,
                EstimatedValueUsed = estimatedValueUsed,
                TurnoverRate = turnoverRate,
                Analysis = analysis,
                Recommendation = recommendation
            });
        }

        report.TotalIngredients = turnoverMetrics.Count;
        report.AverageTurnoverRate = turnoverMetrics.Any()
            ? (decimal)turnoverMetrics.Average(tm => tm.TotalUsageCount)
            : 0;

        report.Items = turnoverMetrics
            .OrderByDescending(tm => tm.TotalUsageCount)
            .ToList();

        // Generate summary
        var summary = new InventoryTurnoverSummary
        {
            FastMovingCount = turnoverMetrics.Count(tm => tm.TurnoverRate == TurnoverRate.Fast),
            ModerateMovingCount = turnoverMetrics.Count(tm => tm.TurnoverRate == TurnoverRate.Moderate),
            SlowMovingCount = turnoverMetrics.Count(tm => tm.TurnoverRate == TurnoverRate.Slow),
            NotUsedCount = turnoverMetrics.Count(tm => tm.TurnoverRate == TurnoverRate.NotUsed),
            TotalEstimatedValue = turnoverMetrics.Sum(tm => tm.EstimatedValueUsed),
            TopCategories = turnoverMetrics
                .GroupBy(tm => tm.Category)
                .OrderByDescending(g => g.Sum(tm => tm.TotalUsageCount))
                .Take(5)
                .Select(g => g.Key)
                .ToList()
        };

        report.Summary = summary;

        return report;
    }

    public async Task<List<InventoryTurnoverMetrics>> GetSlowMovingItemsAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateTurnoverReportAsync(locationId);
        return report.Items
            .Where(i => i.TurnoverRate == TurnoverRate.Slow || i.TurnoverRate == TurnoverRate.NotUsed)
            .OrderBy(i => i.TotalUsageCount)
            .Take(count)
            .ToList();
    }

    public async Task<List<InventoryTurnoverMetrics>> GetFastMovingItemsAsync(Guid locationId, int count = 20)
    {
        var report = await GenerateTurnoverReportAsync(locationId);
        return report.Items
            .Where(i => i.TurnoverRate == TurnoverRate.Fast)
            .OrderByDescending(i => i.TotalUsageCount)
            .Take(count)
            .ToList();
    }

    private TurnoverRate ClassifyTurnoverRate(int usageCount)
    {
        return usageCount switch
        {
            0 => TurnoverRate.NotUsed,
            <= 5 => TurnoverRate.Slow,
            <= 15 => TurnoverRate.Moderate,
            _ => TurnoverRate.Fast
        };
    }

    private string GenerateAnalysis(TurnoverRate rate, int totalUsage, int recipeUsage, int entreeUsage)
    {
        return rate switch
        {
            TurnoverRate.Fast =>
                $"🔥 High demand item used in {totalUsage} recipes/entrees. Critical for operations.",

            TurnoverRate.Moderate =>
                $"✓ Moderate usage across {totalUsage} items ({recipeUsage} recipes, {entreeUsage} entrees). Regular stock item.",

            TurnoverRate.Slow =>
                $"⚠️ Low usage - only {totalUsage} recipes/entrees. Consider for review.",

            TurnoverRate.NotUsed =>
                "❌ Not currently used in any recipes or entrees. Candidate for removal.",

            _ => "No analysis available."
        };
    }

    private string GetRecommendation(TurnoverRate rate, decimal currentPrice)
    {
        return rate switch
        {
            TurnoverRate.Fast =>
                "Maintain adequate stock levels. Consider bulk purchasing for cost savings. Monitor for shortages.",

            TurnoverRate.Moderate =>
                "Standard inventory management. Review quarterly for usage changes.",

            TurnoverRate.Slow =>
                "Reduce stock levels. Consider substituting with more versatile ingredients. Review for elimination.",

            TurnoverRate.NotUsed =>
                "Remove from inventory. Update recipes if this was recently replaced. Archive for historical records.",

            _ => "Review usage patterns."
        };
    }
}
