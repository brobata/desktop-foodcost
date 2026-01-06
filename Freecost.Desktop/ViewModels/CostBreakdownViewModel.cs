using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class CostBreakdownViewModel : ViewModelBase
{
    private readonly ILogger<CostBreakdownViewModel>? _logger;
    private readonly Recipe? _recipe;
    private readonly Entree? _entree;

    [ObservableProperty]
    private string _itemName = string.Empty;

    [ObservableProperty]
    private string _itemType = string.Empty;

    [ObservableProperty]
    private string _totalCost = string.Empty;

    [ObservableProperty]
    private bool _hasData = false;

    public List<CostBreakdownItem> BreakdownItems { get; private set; } = new();

    public CostBreakdownViewModel(Recipe recipe, ILogger<CostBreakdownViewModel>? logger = null)
    {
        _logger = logger;
        _recipe = recipe;
        ItemName = recipe.Name;
        ItemType = "Recipe";
        TotalCost = recipe.TotalCost.ToString("C2");
        LoadBreakdown();
    }

    public CostBreakdownViewModel(Entree entree, ILogger<CostBreakdownViewModel>? logger = null)
    {
        _logger = logger;
        _entree = entree;
        ItemName = entree.Name;
        ItemType = "Menu Item";
        TotalCost = entree.TotalCost.ToString("C2");
        LoadBreakdown();
    }

    private void LoadBreakdown()
    {
        try
        {
            if (_recipe != null)
            {
                LoadRecipeBreakdown();
            }
            else if (_entree != null)
            {
                LoadEntreeBreakdown();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading cost breakdown");
        }
    }

    private void LoadRecipeBreakdown()
    {
        if (_recipe == null || _recipe.RecipeIngredients == null || !_recipe.RecipeIngredients.Any())
        {
            HasData = false;
            return;
        }

        var items = new List<CostBreakdownItem>();

        foreach (var recipeIngredient in _recipe.RecipeIngredients.OrderByDescending(ri => ri.CalculatedCost))
        {
            if (recipeIngredient.Ingredient == null) continue;

            var percentage = _recipe.TotalCost > 0
                ? (recipeIngredient.CalculatedCost / _recipe.TotalCost) * 100
                : 0;

            items.Add(new CostBreakdownItem
            {
                Name = recipeIngredient.Ingredient.Name,
                Cost = recipeIngredient.CalculatedCost,
                Percentage = percentage
            });
        }

        BreakdownItems = items;
        HasData = items.Any();
    }

    private void LoadEntreeBreakdown()
    {
        if (_entree == null)
        {
            HasData = false;
            return;
        }

        var items = new List<CostBreakdownItem>();

        // Add recipe components
        if (_entree.EntreeRecipes != null)
        {
            foreach (var entreeRecipe in _entree.EntreeRecipes)
            {
                if (entreeRecipe.Recipe == null) continue;

                // Calculate cost: Recipe cost per yield unit * quantity
                var costPerUnit = entreeRecipe.Recipe.Yield > 0
                    ? entreeRecipe.Recipe.TotalCost / entreeRecipe.Recipe.Yield
                    : 0;
                var componentCost = costPerUnit * entreeRecipe.Quantity;

                var percentage = _entree.TotalCost > 0
                    ? (componentCost / _entree.TotalCost) * 100
                    : 0;

                items.Add(new CostBreakdownItem
                {
                    Name = $"{entreeRecipe.Recipe.Name} (Recipe)",
                    Cost = componentCost,
                    Percentage = percentage
                });
            }
        }

        // Add ingredient components
        if (_entree.EntreeIngredients != null)
        {
            foreach (var entreeIngredient in _entree.EntreeIngredients)
            {
                if (entreeIngredient.Ingredient == null) continue;

                // Calculate cost: Ingredient price * quantity
                var componentCost = entreeIngredient.Ingredient.CurrentPrice * entreeIngredient.Quantity;

                var percentage = _entree.TotalCost > 0
                    ? (componentCost / _entree.TotalCost) * 100
                    : 0;

                items.Add(new CostBreakdownItem
                {
                    Name = entreeIngredient.Ingredient.Name,
                    Cost = componentCost,
                    Percentage = percentage
                });
            }
        }

        BreakdownItems = items.OrderByDescending(i => i.Cost).ToList();
        HasData = items.Any();
    }
}

public class CostBreakdownItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal Percentage { get; set; }

    public string CostDisplay => Cost.ToString("C2");
    public string PercentageDisplay => $"{Percentage:F1}%";
}
