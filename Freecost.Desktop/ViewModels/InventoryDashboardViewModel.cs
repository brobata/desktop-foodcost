using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class InventoryDashboardViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly Guid _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty]
    private int _totalIngredients;

    [ObservableProperty]
    private int _priceIncreasesCount;

    [ObservableProperty]
    private int _priceDecreasesCount;

    [ObservableProperty]
    private int _volatilePricesCount;

    [ObservableProperty]
    private ObservableCollection<InventoryAlert> _alerts = new();

    [ObservableProperty]
    private ObservableCollection<TopCostIngredient> _topCostIngredients = new();

    public bool HasAlerts => Alerts.Any();

    public InventoryDashboardViewModel(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService;
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationId);
            TotalIngredients = ingredients.Count;

            // Analyze price changes (last 30 days)
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var priceChanges = new List<(Ingredient ingredient, decimal change)>();

            foreach (var ingredient in ingredients)
            {
                if (ingredient.PriceHistory.Any())
                {
                    var recentHistory = ingredient.PriceHistory
                        .Where(ph => ph.RecordedDate >= cutoffDate)
                        .OrderBy(ph => ph.RecordedDate)
                        .ToList();

                    if (recentHistory.Count >= 2)
                    {
                        var oldPrice = recentHistory.First().Price;
                        var newPrice = recentHistory.Last().Price;
                        var change = newPrice - oldPrice;
                        priceChanges.Add((ingredient, change));

                        if (change > 0)
                            PriceIncreasesCount++;
                        else if (change < 0)
                            PriceDecreasesCount++;

                        // Check volatility (>20% change)
                        if (oldPrice > 0 && Math.Abs(change / oldPrice) > 0.2m)
                            VolatilePricesCount++;
                    }
                }
            }

            // Generate alerts
            Alerts.Clear();

            // Alert for significant price increases
            var majorIncreases = priceChanges
                .Where(pc => pc.change > 0 && pc.ingredient.CurrentPrice > 0 && (pc.change / pc.ingredient.CurrentPrice) > 0.15m)
                .OrderByDescending(pc => pc.change)
                .Take(5);

            foreach (var (ingredient, change) in majorIncreases)
            {
                var percentChange = (change / ingredient.CurrentPrice) * 100;
                Alerts.Add(new InventoryAlert
                {
                    Icon = "📈",
                    Title = $"{ingredient.Name} - Significant Price Increase",
                    Description = $"Price increased by ${change:F2} ({percentChange:F1}%) in the last 30 days",
                    BackgroundColor = "#FFEBEE",
                    BorderColor = "#F44336",
                    IngredientId = ingredient.Id
                });
            }

            // Alert for volatile prices
            var volatileItems = priceChanges
                .Where(pc => {
                    var oldPrice = pc.ingredient.CurrentPrice - pc.change;
                    return oldPrice > 0 && Math.Abs(pc.change / oldPrice) > 0.25m;
                })
                .OrderByDescending(pc => Math.Abs(pc.change))
                .Take(3);

            foreach (var (ingredient, change) in volatileItems)
            {
                Alerts.Add(new InventoryAlert
                {
                    Icon = "⚠️",
                    Title = $"{ingredient.Name} - Volatile Pricing",
                    Description = "Price has fluctuated significantly. Consider finding alternative suppliers or adjusting menu prices.",
                    BackgroundColor = "#FFF3E0",
                    BorderColor = "#FF9800",
                    IngredientId = ingredient.Id
                });
            }

            OnPropertyChanged(nameof(HasAlerts));

            // Top cost contributors
            TopCostIngredients.Clear();
            var topCost = ingredients
                .OrderByDescending(i => i.CurrentPrice)
                .Take(10)
                .Select((i, index) => new TopCostIngredient
                {
                    Rank = $"#{index + 1}",
                    Name = i.Name,
                    Category = i.Category ?? "Uncategorized",
                    PriceDisplay = $"${i.CurrentPrice:F2}/{i.Unit}",
                    IngredientId = i.Id
                });

            foreach (var item in topCost)
            {
                TopCostIngredients.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ViewAlertDetails(InventoryAlert alert)
    {
        // TODO: Navigate to ingredient detail or cost trends
        System.Diagnostics.Debug.WriteLine($"Viewing alert for ingredient: {alert.IngredientId}");
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadDashboardDataAsync();
    }
}

public class InventoryAlert
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public Guid IngredientId { get; set; }
}

public class TopCostIngredient
{
    public string Rank { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PriceDisplay { get; set; } = string.Empty;
    public Guid IngredientId { get; set; }
}
