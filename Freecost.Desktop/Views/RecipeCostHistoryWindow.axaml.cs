using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Freecost.Desktop.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class RecipeCostHistoryWindow : Window
{
    public RecipeCostHistoryWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is RecipeCostHistoryViewModel viewModel)
        {
            await viewModel.LoadHistoryAsync();

            if (viewModel.HasData && viewModel.ChartDates.Any())
            {
                CreateChart(viewModel);
            }
        }
    }

    private void CreateChart(RecipeCostHistoryViewModel viewModel)
    {
        var chartContainer = this.FindControl<Border>("ChartContainer");
        if (chartContainer == null) return;

        // Create AvaPlot control
        var avaPlot = new AvaPlot();
        avaPlot.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        avaPlot.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        // Convert DateTime to double (OADate) for plotting
        var dates = viewModel.ChartDates.Select(d => d.ToOADate()).ToArray();
        var costs = viewModel.ChartCosts.ToArray();

        // Add line plot
        var scatter = avaPlot.Plot.Add.Scatter(dates, costs);
        scatter.LineWidth = 2;
        scatter.Color = ScottPlot.Color.FromHex("#2196F3");
        scatter.MarkerSize = 8;
        scatter.MarkerShape = MarkerShape.FilledCircle;

        // Configure axes
        avaPlot.Plot.Axes.DateTimeTicksBottom();
        avaPlot.Plot.XLabel("Date");
        avaPlot.Plot.YLabel("Total Cost ($)");
        avaPlot.Plot.Title($"{viewModel.RecipeName} - Cost Over Time");

        // Style the plot
        avaPlot.Plot.Axes.Bottom.Label.FontSize = 12;
        avaPlot.Plot.Axes.Left.Label.FontSize = 12;

        // Add grid
        avaPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#EEEEEE");

        // Auto-range
        avaPlot.Plot.Axes.AutoScale();

        // Add the chart to the container
        chartContainer.Child = avaPlot;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
