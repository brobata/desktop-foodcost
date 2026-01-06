using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class RecipeCostAnalysisViewModel : ViewModelBase
{
    private readonly Recipe _recipe;

    [ObservableProperty]
    private string _recipeName = string.Empty;

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private decimal _yield;

    [ObservableProperty]
    private decimal _costPerServing;

    [ObservableProperty]
    private string _targetFoodCostPercent = "30";

    [ObservableProperty]
    private decimal _suggestedSellingPrice;

    [ObservableProperty]
    private string _sellingPrice = string.Empty;

    [ObservableProperty]
    private decimal _actualFoodCostPercent;

    [ObservableProperty]
    private decimal _grossProfit;

    [ObservableProperty]
    private decimal _profitMargin;

    [ObservableProperty]
    private decimal _markup;

    [ObservableProperty]
    private decimal _top3Percentage;

    [ObservableProperty]
    private ObservableCollection<IngredientCostItem> _ingredientCosts = new();

    [ObservableProperty]
    private ObservableCollection<string> _topExpenses = new();

    public bool HasSellingPrice => !string.IsNullOrWhiteSpace(SellingPrice) && decimal.TryParse(SellingPrice, out _);

    public string FoodCostColor
    {
        get
        {
            if (ActualFoodCostPercent <= 30) return "#4CAF50"; // Green - Good
            if (ActualFoodCostPercent <= 35) return "#FF9800"; // Orange - Warning
            return "#F44336"; // Red - High
        }
    }

    public RecipeCostAnalysisViewModel(Recipe recipe)
    {
        _recipe = recipe;
        RecipeName = recipe.Name;
        TotalCost = recipe.TotalCost;
        Yield = recipe.Yield;
        CostPerServing = recipe.Yield > 0 ? recipe.TotalCost / recipe.Yield : 0;

        CalculateSuggestedPrice();
        LoadIngredientBreakdown();
    }

    private void CalculateSuggestedPrice()
    {
        if (decimal.TryParse(TargetFoodCostPercent, out var targetPercent) && targetPercent > 0 && CostPerServing > 0)
        {
            // Suggested price = Cost / (Target Food Cost % / 100)
            SuggestedSellingPrice = CostPerServing / (targetPercent / 100);
        }
    }

    private void LoadIngredientBreakdown()
    {
        if (_recipe.RecipeIngredients == null || !_recipe.RecipeIngredients.Any())
            return;

        foreach (var ri in _recipe.RecipeIngredients.Where(ri => ri.Ingredient != null))
        {
            var cost = ri.CalculatedCost;
            var percentage = TotalCost > 0 ? (cost / TotalCost) * 100 : 0;

            IngredientCosts.Add(new IngredientCostItem
            {
                Name = ri.Ingredient!.Name,
                Quantity = ri.Quantity,
                Unit = ri.Unit.ToString(),
                Cost = cost,
                Percentage = percentage
            });
        }

        // Sort by cost descending
        var sorted = IngredientCosts.OrderByDescending(i => i.Cost).ToList();
        IngredientCosts.Clear();
        foreach (var item in sorted)
        {
            IngredientCosts.Add(item);
        }

        // Calculate top 3 percentage
        var top3 = IngredientCosts.Take(3).Sum(i => i.Percentage);
        Top3Percentage = top3;

        // Get top expenses
        TopExpenses.Clear();
        foreach (var item in IngredientCosts.Take(3))
        {
            TopExpenses.Add($"{item.Name}: {item.Cost:C2} ({item.Percentage:F1}%)");
        }
    }

    partial void OnTargetFoodCostPercentChanged(string value)
    {
        CalculateSuggestedPrice();
        CalculateProfitability();
    }

    partial void OnSellingPriceChanged(string value)
    {
        CalculateProfitability();
        OnPropertyChanged(nameof(HasSellingPrice));
    }

    private void CalculateProfitability()
    {
        if (decimal.TryParse(SellingPrice, out var price) && price > 0 && CostPerServing > 0)
        {
            // Actual food cost %
            ActualFoodCostPercent = (CostPerServing / price) * 100;

            // Gross profit
            GrossProfit = price - CostPerServing;

            // Profit margin = (Profit / Selling Price) * 100
            ProfitMargin = (GrossProfit / price) * 100;

            // Markup = (Profit / Cost) * 100
            Markup = (GrossProfit / CostPerServing) * 100;

            OnPropertyChanged(nameof(FoodCostColor));
        }
        else
        {
            ActualFoodCostPercent = 0;
            GrossProfit = 0;
            ProfitMargin = 0;
            Markup = 0;
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        // TODO: Implement Excel export for cost analysis
        await Task.CompletedTask;
    }
}

public partial class IngredientCostItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private decimal _cost;

    [ObservableProperty]
    private decimal _percentage;
}
