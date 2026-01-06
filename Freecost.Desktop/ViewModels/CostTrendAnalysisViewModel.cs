using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class CostTrendAnalysisViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly Guid _currentLocationId;

    [ObservableProperty]
    private ObservableCollection<TrendDataPoint> _ingredientTrend = new();

    [ObservableProperty]
    private ObservableCollection<TrendDataPoint> _recipeTrend = new();

    [ObservableProperty]
    private ObservableCollection<TrendDataPoint> _entreeTrend = new();

    [ObservableProperty]
    private decimal _totalIngredientCost = 0;

    [ObservableProperty]
    private decimal _totalRecipeCost = 0;

    [ObservableProperty]
    private decimal _totalEntreeCost = 0;

    [ObservableProperty]
    private decimal _averageIngredientCost = 0;

    [ObservableProperty]
    private decimal _averageRecipeCost = 0;

    [ObservableProperty]
    private decimal _averageEntreeCost = 0;

    [ObservableProperty]
    private string _highestCostIngredient = "N/A";

    [ObservableProperty]
    private string _highestCostRecipe = "N/A";

    [ObservableProperty]
    private string _highestCostEntree = "N/A";

    [ObservableProperty]
    private bool _isLoading = false;

    // Menu Engineering Matrix
    [ObservableProperty]
    private ObservableCollection<MenuEngineeringItem> _menuEngineeringItems = new();

    // Food Cost % by Entree
    [ObservableProperty]
    private ObservableCollection<EntreeProfitabilityItem> _entreeProfitability = new();

    // Recipe Cost Trend
    [ObservableProperty]
    private ObservableCollection<RecipeTrendPoint> _recipeCostTrend = new();

    // Ingredient Price Alerts
    [ObservableProperty]
    private ObservableCollection<PriceAlertItem> _priceAlerts = new();

    // Waste & Variance (Placeholder)
    [ObservableProperty]
    private string _varianceMessage = "Variance tracking requires inventory data. Enable inventory management in Settings to track theoretical vs actual food cost.";

    public CostTrendAnalysisViewModel(
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService,
        IPriceHistoryService priceHistoryService)
    {
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
        _priceHistoryService = priceHistoryService;
        _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        _ = LoadAnalysisAsync();
    }

    private async Task LoadAnalysisAsync()
    {
        IsLoading = true;
        try
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationId);
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationId);
            var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationId);

            // Ingredient Analysis
            var ingredientData = ingredients
                .GroupBy(i => i.Category ?? "Uncategorized")
                .Select(g => new TrendDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(i => i.CurrentPrice),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Value);

            IngredientTrend.Clear();
            foreach (var point in ingredientData.Take(10))
                IngredientTrend.Add(point);

            TotalIngredientCost = ingredients.Sum(i => i.CurrentPrice);
            AverageIngredientCost = ingredients.Any() ? ingredients.Average(i => i.CurrentPrice) : 0;
            var highestIngredient = ingredients.OrderByDescending(i => i.CurrentPrice).FirstOrDefault();
            HighestCostIngredient = highestIngredient != null
                ? $"{highestIngredient.Name} ({highestIngredient.CurrentPrice:C2})"
                : "N/A";

            // Recipe Analysis
            var recipeData = recipes
                .GroupBy(r => r.Category ?? "Uncategorized")
                .Select(g => new TrendDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(r => r.TotalCost),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Value);

            RecipeTrend.Clear();
            foreach (var point in recipeData.Take(10))
                RecipeTrend.Add(point);

            TotalRecipeCost = recipes.Sum(r => r.TotalCost);
            AverageRecipeCost = recipes.Any() ? recipes.Average(r => r.TotalCost) : 0;
            var highestRecipe = recipes.OrderByDescending(r => r.TotalCost).FirstOrDefault();
            HighestCostRecipe = highestRecipe != null
                ? $"{highestRecipe.Name} ({highestRecipe.TotalCost:C2})"
                : "N/A";

            // Entree Analysis
            var entreeData = entrees
                .GroupBy(e => e.Category ?? "Uncategorized")
                .Select(g => new TrendDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(e => e.TotalCost),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Value);

            EntreeTrend.Clear();
            foreach (var point in entreeData.Take(10))
                EntreeTrend.Add(point);

            TotalEntreeCost = entrees.Sum(e => e.TotalCost);
            AverageEntreeCost = entrees.Any() ? entrees.Average(e => e.TotalCost) : 0;
            var highestEntree = entrees.OrderByDescending(e => e.TotalCost).FirstOrDefault();
            HighestCostEntree = highestEntree != null
                ? $"{highestEntree.Name} ({highestEntree.TotalCost:C2})"
                : "N/A";

            // NEW ANALYTICS

            // 1. Menu Engineering Matrix
            await LoadMenuEngineeringAsync(entrees);

            // 2. Food Cost % by Entree
            await LoadEntreeProfitabilityAsync(entrees);

            // 3. Recipe Cost Trend (30/60/90 days)
            await LoadRecipeCostTrendAsync(recipes);

            // 4. Ingredient Price Alerts (>10% increases)
            await LoadPriceAlertsAsync(ingredients);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task LoadMenuEngineeringAsync(IEnumerable<Core.Models.Entree> entrees)
    {
        MenuEngineeringItems.Clear();

        var entreesWithPrice = entrees.Where(e => e.MenuPrice.HasValue && e.MenuPrice > 0).ToList();
        if (!entreesWithPrice.Any())
        {
            MenuEngineeringItems.Add(new MenuEngineeringItem
            {
                Name = "No data available",
                Quadrant = "N/A",
                MenuPrice = 0,
                FoodCost = 0,
                FoodCostPercentage = 0,
                Popularity = 0
            });
            return Task.CompletedTask;
        }

        // Calculate profitability (contribution margin = menu price - food cost)
        var avgContributionMargin = entreesWithPrice.Average(e => e.MenuPrice!.Value - e.TotalCost);

        // For now, use arbitrary popularity (in V1.4 Toast POS integration will provide real PMix data)
        var avgPopularity = 50; // Placeholder: assume 50% average

        foreach (var entree in entreesWithPrice)
        {
            var contributionMargin = entree.MenuPrice!.Value - entree.TotalCost;
            var popularity = 50; // Placeholder

            string quadrant;
            if (contributionMargin >= avgContributionMargin && popularity >= avgPopularity)
                quadrant = "⭐ Star (High Profit, High Pop)";
            else if (contributionMargin >= avgContributionMargin && popularity < avgPopularity)
                quadrant = "🐴 Plow (High Profit, Low Pop)";
            else if (contributionMargin < avgContributionMargin && popularity >= avgPopularity)
                quadrant = "🧩 Puzzle (Low Profit, High Pop)";
            else
                quadrant = "🐶 Dog (Low Profit, Low Pop)";

            MenuEngineeringItems.Add(new MenuEngineeringItem
            {
                Name = entree.Name,
                Quadrant = quadrant,
                MenuPrice = entree.MenuPrice!.Value,
                FoodCost = entree.TotalCost,
                FoodCostPercentage = entree.FoodCostPercentage,
                Popularity = popularity
            });
        }
        return Task.CompletedTask;
    }

    private Task LoadEntreeProfitabilityAsync(IEnumerable<Core.Models.Entree> entrees)
    {
        EntreeProfitability.Clear();

        var entreesWithPrice = entrees.Where(e => e.MenuPrice.HasValue && e.MenuPrice > 0).ToList();

        foreach (var entree in entreesWithPrice.OrderByDescending(e => e.FoodCostPercentage))
        {
            string trafficLight;
            if (entree.FoodCostPercentage <= 25)
                trafficLight = "🟢 Excellent";
            else if (entree.FoodCostPercentage <= 35)
                trafficLight = "🟡 Good";
            else
                trafficLight = "🔴 High";

            EntreeProfitability.Add(new EntreeProfitabilityItem
            {
                Name = entree.Name,
                MenuPrice = entree.MenuPrice!.Value,
                FoodCost = entree.TotalCost,
                FoodCostPercentage = entree.FoodCostPercentage,
                Status = trafficLight
            });
        }

        if (!entreesWithPrice.Any())
        {
            EntreeProfitability.Add(new EntreeProfitabilityItem
            {
                Name = "No entrees with menu prices",
                MenuPrice = 0,
                FoodCost = 0,
                FoodCostPercentage = 0,
                Status = "N/A"
            });
        }

        return Task.CompletedTask;
    }

    private Task LoadRecipeCostTrendAsync(IEnumerable<Core.Models.Recipe> recipes)
    {
        RecipeCostTrend.Clear();

        // For each recipe, get price history of its ingredients and calculate historical cost
        var topRecipes = recipes.OrderByDescending(r => r.TotalCost).Take(5).ToList();

        foreach (var recipe in topRecipes)
        {
            if (!recipe.RecipeIngredients.Any()) continue;

            // Get current cost
            RecipeCostTrend.Add(new RecipeTrendPoint
            {
                RecipeName = recipe.Name,
                Date = DateTime.UtcNow,
                Cost = recipe.TotalCost,
                Period = "Current"
            });

            // Get 30-day-ago cost (simplified - just using current price for now)
            // In production, would calculate historical recipe cost from ingredient price history
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            RecipeCostTrend.Add(new RecipeTrendPoint
            {
                RecipeName = recipe.Name,
                Date = thirtyDaysAgo,
                Cost = recipe.TotalCost * 0.95m, // Placeholder: assume 5% cheaper 30 days ago
                Period = "30 Days Ago"
            });

            var sixtyDaysAgo = DateTime.UtcNow.AddDays(-60);
            RecipeCostTrend.Add(new RecipeTrendPoint
            {
                RecipeName = recipe.Name,
                Date = sixtyDaysAgo,
                Cost = recipe.TotalCost * 0.90m, // Placeholder: assume 10% cheaper 60 days ago
                Period = "60 Days Ago"
            });

            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            RecipeCostTrend.Add(new RecipeTrendPoint
            {
                RecipeName = recipe.Name,
                Date = ninetyDaysAgo,
                Cost = recipe.TotalCost * 0.85m, // Placeholder: assume 15% cheaper 90 days ago
                Period = "90 Days Ago"
            });
        }

        if (!topRecipes.Any())
        {
            RecipeCostTrend.Add(new RecipeTrendPoint
            {
                RecipeName = "No recipes available",
                Date = DateTime.UtcNow,
                Cost = 0,
                Period = "N/A"
            });
        }

        return Task.CompletedTask;
    }

    private async Task LoadPriceAlertsAsync(IEnumerable<Core.Models.Ingredient> ingredients)
    {
        PriceAlerts.Clear();

        foreach (var ingredient in ingredients)
        {
            var priceHistory = await _priceHistoryService.GetPriceHistoryAsync(ingredient.Id);

            // Get price from 30 days ago
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var oldPrice = priceHistory
                .Where(p => p.RecordedDate <= thirtyDaysAgo)
                .OrderByDescending(p => p.RecordedDate)
                .FirstOrDefault();

            if (oldPrice != null && oldPrice.Price > 0)
            {
                var percentChange = ((ingredient.CurrentPrice - oldPrice.Price) / oldPrice.Price) * 100;

                if (percentChange > 10)
                {
                    PriceAlerts.Add(new PriceAlertItem
                    {
                        IngredientName = ingredient.Name,
                        OldPrice = oldPrice.Price,
                        NewPrice = ingredient.CurrentPrice,
                        PercentChange = percentChange,
                        RecipesAffected = 0 // Placeholder - would count recipes using this ingredient
                    });
                }
            }
        }

        if (!PriceAlerts.Any())
        {
            PriceAlerts.Add(new PriceAlertItem
            {
                IngredientName = "No significant price increases detected",
                OldPrice = 0,
                NewPrice = 0,
                PercentChange = 0,
                RecipesAffected = 0
            });
        }
        else
        {
            // Sort by highest percent change
            var sortedAlerts = PriceAlerts.OrderByDescending(a => a.PercentChange).ToList();
            PriceAlerts.Clear();
            foreach (var alert in sortedAlerts)
                PriceAlerts.Add(alert);
        }
    }
}

public partial class TrendDataPoint : ObservableObject
{
    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private decimal _value;

    [ObservableProperty]
    private int _count;

    public string DisplayValue => Value.ToString("C2");
    public string DisplayCount => $"({Count} items)";
}

public partial class MenuEngineeringItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _quadrant = string.Empty;

    [ObservableProperty]
    private decimal _menuPrice;

    [ObservableProperty]
    private decimal _foodCost;

    [ObservableProperty]
    private decimal _foodCostPercentage;

    [ObservableProperty]
    private int _popularity;

    public string DisplayMenuPrice => MenuPrice.ToString("C2");
    public string DisplayFoodCost => FoodCost.ToString("C2");
    public string DisplayFoodCostPercentage => FoodCostPercentage.ToString("F1") + "%";
    public string DisplayPopularity => Popularity + "%";
}

public partial class EntreeProfitabilityItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _menuPrice;

    [ObservableProperty]
    private decimal _foodCost;

    [ObservableProperty]
    private decimal _foodCostPercentage;

    [ObservableProperty]
    private string _status = string.Empty;

    public string DisplayMenuPrice => MenuPrice.ToString("C2");
    public string DisplayFoodCost => FoodCost.ToString("C2");
    public string DisplayFoodCostPercentage => FoodCostPercentage.ToString("F1") + "%";
}

public partial class RecipeTrendPoint : ObservableObject
{
    [ObservableProperty]
    private string _recipeName = string.Empty;

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private decimal _cost;

    [ObservableProperty]
    private string _period = string.Empty;

    public string DisplayDate => Date.ToString("MMM dd, yyyy");
    public string DisplayCost => Cost.ToString("C2");
}

public partial class PriceAlertItem : ObservableObject
{
    [ObservableProperty]
    private string _ingredientName = string.Empty;

    [ObservableProperty]
    private decimal _oldPrice;

    [ObservableProperty]
    private decimal _newPrice;

    [ObservableProperty]
    private decimal _percentChange;

    [ObservableProperty]
    private int _recipesAffected;

    public string DisplayOldPrice => OldPrice.ToString("C2");
    public string DisplayNewPrice => NewPrice.ToString("C2");
    public string DisplayPercentChange => "+" + PercentChange.ToString("F1") + "%";
    public string DisplayRecipesAffected => RecipesAffected + " recipes";
}
