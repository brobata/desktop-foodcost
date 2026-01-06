using Freecost.Core.Models;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public class MenuEngineeringService : IMenuEngineeringService
{
    private readonly IEntreeRepository _entreeRepository;
    private readonly IRecipeCostCalculator _costCalculator;

    public MenuEngineeringService(
        IEntreeRepository entreeRepository,
        IRecipeCostCalculator costCalculator)
    {
        _entreeRepository = entreeRepository;
        _costCalculator = costCalculator;
    }

    public async Task<MenuEngineeringAnalysis> AnalyzeMenuAsync(Guid locationId)
    {
        var entrees = await _entreeRepository.GetAllAsync(locationId);
        var analysis = new MenuEngineeringAnalysis();

        // Calculate contribution margin for each item
        var itemAnalyses = new List<MenuItemAnalysis>();

        foreach (var entree in entrees)
        {
            if (!entree.MenuPrice.HasValue || entree.MenuPrice.Value == 0)
            {
                continue; // Skip items without menu price
            }

            // Calculate total food cost
            decimal foodCost = 0;

            // Cost from recipes
            if (entree.EntreeRecipes != null)
            {
                foreach (var er in entree.EntreeRecipes)
                {
                    await _costCalculator.CalculateRecipeTotalCostAsync(er.Recipe);
                    foodCost += er.Recipe.TotalCost;
                }
            }

            // Cost from direct ingredients
            if (entree.EntreeIngredients != null)
            {
                foreach (var ei in entree.EntreeIngredients)
                {
                    var ingredientCost = await _costCalculator.CalculateIngredientCostAsync(
                        new RecipeIngredient
                        {
                            Ingredient = ei.Ingredient,
                            IngredientId = ei.IngredientId,
                            Quantity = ei.Quantity,
                            Unit = ei.Unit
                        });
                    foodCost += ingredientCost;
                }
            }

            var contributionMargin = entree.MenuPrice.Value - foodCost;
            var foodCostPercent = entree.MenuPrice.Value > 0 ? (foodCost / entree.MenuPrice.Value) * 100 : 0;

            // TODO: In production, popularity would come from sales data
            // For now, use a simulated popularity score based on food cost %
            // Lower food cost % = potentially more popular (simplified assumption)
            var popularity = foodCostPercent < 30 ? 75 : foodCostPercent < 35 ? 50 : 25;

            itemAnalyses.Add(new MenuItemAnalysis
            {
                EntreeId = entree.Id,
                EntreeName = entree.Name,
                MenuPrice = entree.MenuPrice.Value,
                FoodCost = foodCost,
                ContributionMargin = contributionMargin,
                FoodCostPercent = foodCostPercent,
                Popularity = popularity
            });
        }

        if (!itemAnalyses.Any())
        {
            return analysis;
        }

        // Calculate averages
        analysis.AverageContributionMargin = itemAnalyses.Average(i => i.ContributionMargin);
        analysis.AveragePopularity = itemAnalyses.Average(i => i.Popularity);

        // Classify each item
        foreach (var item in itemAnalyses)
        {
            item.Classification = ClassifyMenuItem(
                item.ContributionMargin,
                item.Popularity,
                analysis.AverageContributionMargin,
                analysis.AveragePopularity);

            item.Recommendation = GetRecommendation(item);
        }

        analysis.Items = itemAnalyses;

        // Generate summary
        analysis.Summary = new MenuEngineeringSummary
        {
            StarsCount = itemAnalyses.Count(i => i.Classification == MenuItemClassification.Star),
            PlowHorsesCount = itemAnalyses.Count(i => i.Classification == MenuItemClassification.PlowHorse),
            PuzzlesCount = itemAnalyses.Count(i => i.Classification == MenuItemClassification.Puzzle),
            DogsCount = itemAnalyses.Count(i => i.Classification == MenuItemClassification.Dog)
        };

        analysis.Summary.OverallHealth = CalculateOverallHealth(analysis.Summary);

        return analysis;
    }

    public MenuItemClassification ClassifyMenuItem(
        decimal contributionMargin,
        decimal popularity,
        decimal avgMargin,
        decimal avgPopularity)
    {
        var highProfitability = contributionMargin >= avgMargin;
        var highPopularity = popularity >= avgPopularity;

        return (highProfitability, highPopularity) switch
        {
            (true, true) => MenuItemClassification.Star,
            (false, true) => MenuItemClassification.PlowHorse,
            (true, false) => MenuItemClassification.Puzzle,
            (false, false) => MenuItemClassification.Dog
        };
    }

    private string GetRecommendation(MenuItemAnalysis item)
    {
        return item.Classification switch
        {
            MenuItemClassification.Star => "✨ Maintain quality, promote heavily, feature prominently",
            MenuItemClassification.PlowHorse => "🐴 Increase price slightly or reduce portion/cost to improve margins",
            MenuItemClassification.Puzzle => "🧩 Reposition on menu, add marketing, rename, or reduce price to boost sales",
            MenuItemClassification.Dog => "🐕 Consider removal or complete redesign with better cost structure",
            _ => "Analyze further"
        };
    }

    private string CalculateOverallHealth(MenuEngineeringSummary summary)
    {
        var totalItems = summary.StarsCount + summary.PlowHorsesCount +
                        summary.PuzzlesCount + summary.DogsCount;

        if (totalItems == 0)
        {
            return "No data";
        }

        var starsPercent = (summary.StarsCount / (decimal)totalItems) * 100;
        var dogsPercent = (summary.DogsCount / (decimal)totalItems) * 100;

        if (starsPercent >= 40 && dogsPercent <= 20)
        {
            return "Excellent - Strong menu with many stars";
        }
        else if (starsPercent >= 25 && dogsPercent <= 30)
        {
            return "Good - Balanced menu with optimization opportunities";
        }
        else if (dogsPercent >= 40)
        {
            return "Needs attention - Too many underperforming items";
        }
        else
        {
            return "Fair - Consider menu reengineering";
        }
    }
}
