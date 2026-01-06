using CommunityToolkit.Mvvm.ComponentModel;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class PriceHistoryViewModel : ViewModelBase
{
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly Guid _ingredientId;

    [ObservableProperty]
    private string _ingredientName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PriceHistoryDisplayModel> _priceHistory = new();

    [ObservableProperty]
    private decimal _currentPrice = 0;

    [ObservableProperty]
    private decimal _averagePrice = 0;

    [ObservableProperty]
    private decimal _highestPrice = 0;

    [ObservableProperty]
    private decimal _lowestPrice = 0;

    [ObservableProperty]
    private bool _isLoading = false;

    public PriceHistoryViewModel(IPriceHistoryService priceHistoryService, Ingredient ingredient)
    {
        _priceHistoryService = priceHistoryService;
        _ingredientId = ingredient.Id;
        _ingredientName = ingredient.Name;
        _currentPrice = ingredient.CurrentPrice;

        _ = LoadPriceHistoryAsync();
    }

    private async Task LoadPriceHistoryAsync()
    {
        IsLoading = true;
        try
        {
            var history = await _priceHistoryService.GetPriceHistoryAsync(_ingredientId);

            PriceHistory.Clear();
            foreach (var record in history.OrderByDescending(h => h.RecordedDate))
            {
                PriceHistory.Add(new PriceHistoryDisplayModel
                {
                    Price = record.Price,
                    Date = record.RecordedDate,
                    AggregationType = record.AggregationType.ToString(),
                    IsAggregated = record.IsAggregated
                });
            }

            if (PriceHistory.Any())
            {
                AveragePrice = PriceHistory.Average(p => p.Price);

                // Include current price in highest/lowest calculations
                var allPrices = PriceHistory.Select(p => p.Price).Append(CurrentPrice);
                HighestPrice = allPrices.Max();
                LowestPrice = allPrices.Min();
            }
            else if (CurrentPrice > 0)
            {
                // If no history, current price is both highest and lowest
                HighestPrice = CurrentPrice;
                LowestPrice = CurrentPrice;
                AveragePrice = CurrentPrice;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class PriceHistoryDisplayModel
{
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string AggregationType { get; set; } = string.Empty;
    public bool IsAggregated { get; set; }

    public string DisplayDate => Date.ToString("MMM dd, yyyy");
    public string DisplayPrice => Price.ToString("C2");
    public string DisplayType => IsAggregated ? $"({AggregationType})" : "Daily";
}
