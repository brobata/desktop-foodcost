using Avalonia.Controls;
using Dfc.Core.Models;
using Dfc.Desktop.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Desktop.Views;

public partial class CostTrendsWindow : Window
{
    private AvaPlot? _chart;

    public CostTrendsWindow()
    {
        InitializeComponent();

        Opened += OnWindowOpened;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        _chart = this.FindControl<AvaPlot>("CostChart");
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CostTrendsViewModel viewModel)
        {
            viewModel.DataFiltered += OnDataFiltered;
        }
    }

    private void OnDataFiltered(object? sender, List<PriceHistory> priceHistory)
    {
        if (_chart == null || !priceHistory.Any())
            return;

        try
        {
            // Clear existing plots
            _chart.Plot.Clear();

            // Prepare data for ScottPlot
            var dates = priceHistory.Select(ph => ph.RecordedDate.ToOADate()).ToArray();
            var prices = priceHistory.Select(ph => (double)ph.Price).ToArray();

            // Add line plot
            var linePlot = _chart.Plot.Add.ScatterLine(dates, prices);
            linePlot.Color = ScottPlot.Color.FromHex("#7AB51D");
            linePlot.LineWidth = 2;
            linePlot.MarkerSize = 6;

            // Add markers
            var scatterPlot = _chart.Plot.Add.Scatter(dates, prices);
            scatterPlot.Color = ScottPlot.Color.FromHex("#7AB51D");
            scatterPlot.MarkerSize = 8;
            scatterPlot.LineWidth = 0; // No line, just markers

            // Configure axes
            _chart.Plot.Axes.DateTimeTicksBottom();
            _chart.Plot.Axes.Left.Label.Text = "Price ($)";
            _chart.Plot.Axes.Bottom.Label.Text = "Date";

            // Style
            _chart.Plot.Axes.Left.Label.FontSize = 14;
            _chart.Plot.Axes.Bottom.Label.FontSize = 14;
            _chart.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E0E0E0");

            // Add average price line
            if (DataContext is CostTrendsViewModel vm && prices.Any())
            {
                var avgPrice = (double)vm.AveragePrice;
                var avgLine = _chart.Plot.Add.HorizontalLine(avgPrice);
                avgLine.Color = ScottPlot.Color.FromHex("#FF9800");
                avgLine.LineWidth = 2;
                avgLine.LinePattern = LinePattern.Dashed;
                avgLine.LegendText = $"Average: ${vm.AveragePrice:F2}";
            }

            // Auto-scale
            _chart.Plot.Axes.AutoScale();

            // Add some padding
            _chart.Plot.Axes.Margins(0.1, 0.1);

            // Refresh the chart
            _chart.Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rendering chart: {ex.Message}");
        }
    }
}
