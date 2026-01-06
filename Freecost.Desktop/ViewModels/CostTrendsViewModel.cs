using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class CostTrendsViewModel : ViewModelBase
{
    private readonly List<PriceHistory> _allPriceHistory;
    private List<PriceHistory> _filteredPriceHistory;

    [ObservableProperty]
    private string _ingredientName;

    [ObservableProperty]
    private decimal _averagePrice;

    [ObservableProperty]
    private decimal _currentPrice;

    [ObservableProperty]
    private string _priceChangeText = string.Empty;

    [ObservableProperty]
    private string _priceChangeColor = "#666666";

    [ObservableProperty]
    private int _dataPointCount;

    [ObservableProperty]
    private int _selectedDays = 30; // Default to 30 days

    public event EventHandler<List<PriceHistory>>? DataFiltered;

    public CostTrendsViewModel(string ingredientName, List<PriceHistory> priceHistory)
    {
        _ingredientName = ingredientName;
        _allPriceHistory = priceHistory.OrderBy(ph => ph.RecordedDate).ToList();
        _filteredPriceHistory = _allPriceHistory;

        // Calculate initial statistics for 30 days
        FilterData(30);
    }

    [RelayCommand]
    private void SetTimeRange(string daysStr)
    {
        if (int.TryParse(daysStr, out int days))
        {
            SelectedDays = days;
            FilterData(days);
        }
    }

    private void FilterData(int days)
    {
        if (days == 0)
        {
            // All time
            _filteredPriceHistory = _allPriceHistory;
        }
        else
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            _filteredPriceHistory = _allPriceHistory
                .Where(ph => ph.RecordedDate >= cutoffDate)
                .ToList();
        }

        // Calculate statistics
        if (_filteredPriceHistory.Any())
        {
            AveragePrice = _filteredPriceHistory.Average(ph => ph.Price);
            CurrentPrice = _filteredPriceHistory.Last().Price;
            DataPointCount = _filteredPriceHistory.Count;

            // Calculate price change
            if (_filteredPriceHistory.Count > 1)
            {
                var firstPrice = _filteredPriceHistory.First().Price;
                var lastPrice = _filteredPriceHistory.Last().Price;
                var change = lastPrice - firstPrice;
                var changePercent = firstPrice > 0 ? (change / firstPrice) * 100 : 0;

                if (change > 0)
                {
                    PriceChangeText = $"+${change:F2} (+{changePercent:F1}%)";
                    PriceChangeColor = "#F44336"; // Red for increase
                }
                else if (change < 0)
                {
                    PriceChangeText = $"${change:F2} ({changePercent:F1}%)";
                    PriceChangeColor = "#4CAF50"; // Green for decrease
                }
                else
                {
                    PriceChangeText = "No change";
                    PriceChangeColor = "#666666";
                }
            }
            else
            {
                PriceChangeText = "Insufficient data";
                PriceChangeColor = "#666666";
            }
        }
        else
        {
            AveragePrice = 0;
            CurrentPrice = 0;
            DataPointCount = 0;
            PriceChangeText = "No data";
            PriceChangeColor = "#666666";
        }

        // Notify listeners to update chart
        DataFiltered?.Invoke(this, _filteredPriceHistory);
    }
}
