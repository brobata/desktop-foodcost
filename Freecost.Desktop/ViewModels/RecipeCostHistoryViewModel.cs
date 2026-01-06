using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class RecipeCostHistoryViewModel : ViewModelBase
{
    private readonly ILogger<RecipeCostHistoryViewModel>? _logger;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly Recipe _recipe;

    [ObservableProperty]
    private string _recipeName = string.Empty;

    [ObservableProperty]
    private string _currentCost = string.Empty;

    [ObservableProperty]
    private string _dateRange = string.Empty;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasData = false;

    public List<DateTime> ChartDates { get; private set; } = new();
    public List<double> ChartCosts { get; private set; } = new();

    public RecipeCostHistoryViewModel(Recipe recipe, IPriceHistoryService priceHistoryService, ILogger<RecipeCostHistoryViewModel>? logger = null)
    {
        _logger = logger;
        _recipe = recipe;
        _priceHistoryService = priceHistoryService;
        RecipeName = recipe.Name;
        CurrentCost = recipe.TotalCost.ToString("C2");
    }

    public async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            HasData = false;

            if (_recipe.RecipeIngredients == null || !_recipe.RecipeIngredients.Any())
            {
                return;
            }

            // Get price history for all ingredients
            var allPriceHistories = new Dictionary<Guid, List<PriceHistory>>();
            foreach (var recipeIngredient in _recipe.RecipeIngredients)
            {
                if (recipeIngredient.Ingredient == null) continue;

                var history = await _priceHistoryService.GetPriceHistoryAsync(recipeIngredient.Ingredient.Id);
                if (history.Any())
                {
                    allPriceHistories[recipeIngredient.Ingredient.Id] = history;
                }
            }

            if (!allPriceHistories.Any())
            {
                return;
            }

            // Collect all unique dates from all ingredients
            var allDates = allPriceHistories.Values
                .SelectMany(histories => histories.Select(h => h.RecordedDate.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (!allDates.Any())
            {
                return;
            }

            // Calculate recipe cost at each date
            var costsByDate = new Dictionary<DateTime, decimal>();

            foreach (var date in allDates)
            {
                decimal totalCost = 0;
                bool canCalculate = true;

                foreach (var recipeIngredient in _recipe.RecipeIngredients)
                {
                    if (recipeIngredient.Ingredient == null)
                    {
                        canCalculate = false;
                        break;
                    }

                    // Find the most recent price for this ingredient at or before this date
                    if (allPriceHistories.TryGetValue(recipeIngredient.Ingredient.Id, out var histories))
                    {
                        var priceAtDate = histories
                            .Where(h => h.RecordedDate.Date <= date)
                            .OrderByDescending(h => h.RecordedDate)
                            .FirstOrDefault();

                        if (priceAtDate != null)
                        {
                            // Calculate cost for this ingredient
                            var ingredientCost = priceAtDate.Price * recipeIngredient.Quantity;
                            totalCost += ingredientCost;
                        }
                        else
                        {
                            canCalculate = false;
                            break;
                        }
                    }
                    else
                    {
                        canCalculate = false;
                        break;
                    }
                }

                if (canCalculate)
                {
                    costsByDate[date] = totalCost;
                }
            }

            if (costsByDate.Any())
            {
                ChartDates = costsByDate.Keys.OrderBy(d => d).ToList();
                ChartCosts = ChartDates.Select(d => (double)costsByDate[d]).ToList();

                var minDate = ChartDates.Min();
                var maxDate = ChartDates.Max();
                DateRange = $"{minDate:MMM d, yyyy} - {maxDate:MMM d, yyyy}";

                HasData = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading recipe cost history");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
