// Location: Dfc.Desktop/ViewModels/DashboardViewModel.cs
// Action: CREATE - Dashboard overview with key metrics and quick actions

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly ILogger<DashboardViewModel>? _logger;

    public DashboardViewModel(
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService,
        ICurrentLocationService currentLocationService,
        ILogger<DashboardViewModel>? logger = null)
    {
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
        _currentLocationService = currentLocationService;
        _logger = logger;

        RecentItems = new ObservableCollection<RecentItemModel>();
        TopCostItems = new ObservableCollection<TopCostItemModel>();
    }

    [ObservableProperty]
    private bool _isLoading = true;

    // Version and Build Information
    public string VersionInfo => "Version 1.0";


    // Overall Statistics
    [ObservableProperty]
    private int _totalIngredients;

    [ObservableProperty]
    private int _totalRecipes;

    [ObservableProperty]
    private int _totalEntrees;

    // Validation Warnings
    [ObservableProperty]
    private int _validationWarningCount;

    [ObservableProperty]
    private int _missingPrices;

    [ObservableProperty]
    private int _missingVendors;

    [ObservableProperty]
    private int _missingCategories;

    [ObservableProperty]
    private int _missingNutritionalData;

    // Recent Items
    [ObservableProperty]
    private ObservableCollection<RecentItemModel> _recentItems;

    // Top Cost Items
    [ObservableProperty]
    private ObservableCollection<TopCostItemModel> _topCostItems;

    // Profitability Zones
    [ObservableProperty]
    private int _greenZoneCount;

    [ObservableProperty]
    private int _yellowZoneCount;

    [ObservableProperty]
    private int _redZoneCount;

    // Price Alerts
    [ObservableProperty]
    private ObservableCollection<PriceAlertModel> _priceAlerts = new();

    // Chart Data - exposed for view to access (deprecated, keeping for compatibility)
    public List<Core.Models.Ingredient>? Ingredients { get; private set; }
    public List<Core.Models.Entree>? Entrees { get; private set; }

    public async Task LoadDashboardAsync()
    {
        try
        {
            IsLoading = true;

            // Load all data in parallel
            var ingredientsTask = _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
            var recipesTask = _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);
            var entreesTask = _entreeService.GetAllEntreesAsync(_currentLocationService.CurrentLocationId);

            await Task.WhenAll(ingredientsTask, recipesTask, entreesTask);

            var ingredients = (await ingredientsTask).ToList();
            var recipes = (await recipesTask).ToList();
            var entrees = (await entreesTask).ToList();

            // Store for chart generation in view
            Ingredients = ingredients;
            Entrees = entrees;

            // Calculate statistics
            TotalIngredients = ingredients.Count;
            TotalRecipes = recipes.Count;
            TotalEntrees = entrees.Count;

            // Validation warnings
            MissingPrices = ingredients.Count(i => i.CurrentPrice == 0);
            MissingVendors = ingredients.Count(i => string.IsNullOrWhiteSpace(i.VendorName));
            MissingCategories = ingredients.Count(i => string.IsNullOrWhiteSpace(i.Category));
            MissingNutritionalData = ingredients.Count(i =>
                !i.CaloriesPerUnit.HasValue &&
                !i.ProteinPerUnit.HasValue &&
                !i.CarbohydratesPerUnit.HasValue &&
                !i.FatPerUnit.HasValue);
            ValidationWarningCount = MissingPrices + MissingVendors + MissingCategories;

            // Recent items (last 5 modified)
            RecentItems.Clear();
            var recentIngredients = ingredients
                .OrderByDescending(i => i.ModifiedAt)
                .Take(3)
                .Select(i => new RecentItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = "Ingredient",
                    Icon = "🥕",
                    Date = i.ModifiedAt.ToString("g"),
                    Cost = i.CurrentPrice.ToString("C2")
                });

            var recentRecipes = recipes
                .OrderByDescending(r => r.ModifiedAt)
                .Take(2)
                .Select(r => new RecentItemModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Type = "Recipe",
                    Icon = "📝",
                    Date = r.ModifiedAt.ToString("g"),
                    Cost = r.TotalCost.ToString("C2")
                });

            foreach (var item in recentIngredients.Concat(recentRecipes).OrderByDescending(i => i.Date).Take(5))
            {
                RecentItems.Add(item);
            }

            // Top cost items (top 5 entrees)
            TopCostItems.Clear();
            var topEntrees = entrees
                .OrderByDescending(e => e.TotalCost)
                .Take(5)
                .Select(e => new TopCostItemModel
                {
                    Name = e.Name,
                    Cost = e.TotalCost,
                    Percentage = entrees.Max(x => x.TotalCost) > 0
                        ? (double)((e.TotalCost / entrees.Max(x => x.TotalCost)) * 100)
                        : 0
                });

            foreach (var item in topEntrees)
            {
                TopCostItems.Add(item);
            }

            // Calculate profitability zones
            GreenZoneCount = entrees.Count(e =>
            {
                if (!e.MenuPrice.HasValue || e.MenuPrice.Value == 0) return false;
                var foodCostPercent = (e.TotalCost / e.MenuPrice.Value) * 100;
                return foodCostPercent < 30;
            });

            YellowZoneCount = entrees.Count(e =>
            {
                if (!e.MenuPrice.HasValue || e.MenuPrice.Value == 0) return false;
                var foodCostPercent = (e.TotalCost / e.MenuPrice.Value) * 100;
                return foodCostPercent >= 30 && foodCostPercent <= 40;
            });

            RedZoneCount = entrees.Count(e =>
            {
                if (!e.MenuPrice.HasValue || e.MenuPrice.Value == 0) return false;
                var foodCostPercent = (e.TotalCost / e.MenuPrice.Value) * 100;
                return foodCostPercent > 40;
            });

            // Generate price alerts (mock data for now - in real app would track price history)
            PriceAlerts.Clear();
            var alertCount = 0;
            foreach (var ingredient in ingredients.OrderByDescending(i => i.ModifiedAt).Take(10))
            {
                // Simulate recent price increases for ingredients with price > 0
                if (ingredient.CurrentPrice > 0 && alertCount < 5)
                {
                    var oldPrice = ingredient.CurrentPrice * 0.9m; // Simulate 10% increase
                    var percentChange = ((ingredient.CurrentPrice - oldPrice) / oldPrice) * 100;

                    if (percentChange > 3) // Only show increases > 3%
                    {
                        PriceAlerts.Add(new PriceAlertModel
                        {
                            IngredientName = ingredient.Name,
                            OldPrice = oldPrice,
                            NewPrice = ingredient.CurrentPrice,
                            PercentChange = (double)percentChange
                        });
                        alertCount++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading dashboard data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadDashboardAsync();
    }
}

public partial class RecentItemModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _date = string.Empty;

    [ObservableProperty]
    private string _cost = string.Empty;
}

public partial class TopCostItemModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _cost;

    [ObservableProperty]
    private double _percentage;
}

public partial class PriceAlertModel : ObservableObject
{
    [ObservableProperty]
    private string _ingredientName = string.Empty;

    [ObservableProperty]
    private decimal _oldPrice;

    [ObservableProperty]
    private decimal _newPrice;

    [ObservableProperty]
    private double _percentChange;
}
