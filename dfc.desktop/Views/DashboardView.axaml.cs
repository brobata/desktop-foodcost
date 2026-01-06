using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.Views;

public partial class DashboardView : UserControl
{
    private readonly ILogger? _logger;
    private bool _hasLoaded = false;

    public DashboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        _logger = App.Services?.GetService<ILogger<DashboardView>>();
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_hasLoaded) return; // Prevent reload if already loaded

            if (DataContext is DashboardViewModel viewModel)
            {
                await viewModel.LoadDashboardAsync();
                CreateCharts(viewModel);
                _hasLoaded = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading dashboard");
        }
    }

    private void CreateCharts(DashboardViewModel viewModel)
    {
        CreateCategoryBreakdownChart(viewModel);
        CreateCostDistributionChart(viewModel);
    }

    private void CreateCategoryBreakdownChart(DashboardViewModel viewModel)
    {
        var container = this.FindControl<Border>("CategoryChartContainer");
        if (container == null)
            return;

        // Check if there's data to display
        if (viewModel.Ingredients == null || !viewModel.Ingredients.Any())
        {
            var emptyState = new Avalonia.Controls.StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 8
            };
            emptyState.Children.Add(new TextBlock
            {
                Text = "📊",
                FontSize = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Opacity = 0.3
            });
            emptyState.Children.Add(new TextBlock
            {
                Text = "No category data yet",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            container.Child = emptyState;
            return;
        }

        var avaPlot = new AvaPlot
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        // Group ingredients by category
        var categoryGroups = viewModel.Ingredients
            .Where(i => !string.IsNullOrWhiteSpace(i.Category))
            .GroupBy(i => i.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        if (!categoryGroups.Any())
        {
            var emptyState = new Avalonia.Controls.StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 8
            };
            emptyState.Children.Add(new TextBlock
            {
                Text = "📊",
                FontSize = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Opacity = 0.3
            });
            emptyState.Children.Add(new TextBlock
            {
                Text = "Add categories to ingredients to see breakdown",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            container.Child = emptyState;
            return;
        }

        // Create pie chart
        var values = categoryGroups.Select(x => (double)x.Count).ToArray();
        var labels = categoryGroups.Select(x => $"{x.Category} ({x.Count})").ToArray();

        var pie = avaPlot.Plot.Add.Pie(values);
        for (int i = 0; i < pie.Slices.Count && i < labels.Length; i++)
        {
            pie.Slices[i].Label = labels[i];
            pie.Slices[i].LabelStyle.FontSize = 12;
            pie.Slices[i].LabelStyle.Bold = true;
        }

        avaPlot.Plot.Title("Ingredients by Category");
        avaPlot.Plot.ShowLegend();
        avaPlot.Plot.Layout.Frameless();

        container.Child = avaPlot;
    }

    private void CreateCostDistributionChart(DashboardViewModel viewModel)
    {
        var container = this.FindControl<Border>("CostChartContainer");
        if (container == null)
            return;

        // Check if there's data to display
        if (viewModel.Entrees == null || !viewModel.Entrees.Any())
        {
            var emptyState = new Avalonia.Controls.StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 8
            };
            emptyState.Children.Add(new TextBlock
            {
                Text = "💰",
                FontSize = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Opacity = 0.3
            });
            emptyState.Children.Add(new TextBlock
            {
                Text = "No entrees yet",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            emptyState.Children.Add(new TextBlock
            {
                Text = "Create entrees to see cost distribution",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                FontStyle = Avalonia.Media.FontStyle.Italic
            });
            container.Child = emptyState;
            return;
        }

        var avaPlot = new AvaPlot
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        // Create bar chart for top 10 entrees by cost
        var topEntrees = viewModel.Entrees
            .OrderByDescending(e => e.TotalCost)
            .Take(10)
            .Reverse()
            .ToList();

        if (!topEntrees.Any())
        {
            var emptyState = new Avalonia.Controls.StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 8
            };
            emptyState.Children.Add(new TextBlock
            {
                Text = "💰",
                FontSize = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Opacity = 0.3
            });
            emptyState.Children.Add(new TextBlock
            {
                Text = "Create entrees to see cost distribution",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });
            container.Child = emptyState;
            return;
        }

        var positions = System.Linq.Enumerable.Range(0, topEntrees.Count).Select(i => (double)i).ToArray();
        var costs = topEntrees.Select(e => (double)e.TotalCost).ToArray();
        var labels = topEntrees.Select(e => e.Name.Length > 20 ? e.Name.Substring(0, 17) + "..." : e.Name).ToArray();

        var bars = avaPlot.Plot.Add.Bars(positions, costs);
        bars.Color = ScottPlot.Color.FromHex("#FF9800");

        // Customize appearance
        avaPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            positions.Select((p, i) => new ScottPlot.Tick(p, labels[i])).ToArray()
        );
        avaPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
        avaPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;

        avaPlot.Plot.YLabel("Cost ($)");
        avaPlot.Plot.Title("Top Entrees by Cost");
        avaPlot.Plot.Layout.Frameless();
        avaPlot.Plot.Axes.AutoScale();

        container.Child = avaPlot;
    }
}
