using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Freecost.Desktop.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class CostBreakdownWindow : Window
{
    public CostBreakdownWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CostBreakdownViewModel viewModel && viewModel.HasData)
        {
            CreatePieChart(viewModel);
        }
    }

    private void CreatePieChart(CostBreakdownViewModel viewModel)
    {
        var chartContainer = this.FindControl<Border>("PieChartContainer");
        if (chartContainer == null) return;

        // Create AvaPlot control
        var avaPlot = new AvaPlot();
        avaPlot.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        avaPlot.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        // Prepare data for pie chart
        var values = viewModel.BreakdownItems.Select(i => (double)i.Cost).ToArray();
        var labels = viewModel.BreakdownItems.Select(i => i.Name).ToArray();

        // Add pie chart
        var pie = avaPlot.Plot.Add.Pie(values);
        pie.SliceLabelDistance = 1.3;

        // Assign labels
        for (int i = 0; i < labels.Length && i < pie.Slices.Count; i++)
        {
            pie.Slices[i].Label = $"{labels[i]}\n{viewModel.BreakdownItems[i].PercentageDisplay}";
            pie.Slices[i].LabelFontSize = 11;
            pie.Slices[i].LabelBold = true;
        }

        // Use distinct colors
        var colors = new[]
        {
            "#2196F3", // Blue
            "#4CAF50", // Green
            "#FF9800", // Orange
            "#9C27B0", // Purple
            "#F44336", // Red
            "#00BCD4", // Cyan
            "#FFEB3B", // Yellow
            "#795548", // Brown
            "#607D8B", // Blue Grey
            "#E91E63"  // Pink
        };

        for (int i = 0; i < pie.Slices.Count; i++)
        {
            var colorIndex = i % colors.Length;
            pie.Slices[i].FillColor = ScottPlot.Color.FromHex(colors[colorIndex]);
        }

        // Hide axes for cleaner look
        avaPlot.Plot.Axes.Frameless();
        avaPlot.Plot.HideGrid();

        // Add title
        avaPlot.Plot.Title($"{viewModel.ItemName} - Cost Breakdown");

        // Add the chart to the container
        chartContainer.Child = avaPlot;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
