using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Freecost.Desktop.Views;

public partial class RecipesView : UserControl
{
    private readonly ILogger? _logger;

    public RecipesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        _logger = App.Services?.GetService<ILogger<RecipesView>>();
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        KeyDown += OnViewKeyDown;
    }

    private async void OnViewKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (DataContext is not RecipesViewModel viewModel) return;

            // Ctrl+N = New recipe
            if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
            {
                await viewModel.AddRecipeCommand.ExecuteAsync(null);
                e.Handled = true;
                return;
            }

            // Ctrl+D = Duplicate selected
            if (e.Key == Key.D && e.KeyModifiers == KeyModifiers.Control && viewModel.SelectedRecipe != null)
            {
                await viewModel.DuplicateRecipeCommand.ExecuteAsync(viewModel.SelectedRecipe.Recipe);
                e.Handled = true;
                return;
            }

            // Ctrl+P = Print selected
            if (e.Key == Key.P && e.KeyModifiers == KeyModifiers.Control && viewModel.SelectedRecipe != null)
            {
                await viewModel.GenerateRecipePdfCommand.ExecuteAsync(viewModel.SelectedRecipe.Recipe);
                e.Handled = true;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling keyboard shortcut in RecipesView");
        }
    }

    private async void OnRecipeDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                await viewModel.EditSelectedRecipeCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling double-tap in RecipesView");
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                if (e.Key == Key.Enter)
                {
                    await viewModel.EditSelectedRecipeCommand.ExecuteAsync(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    await viewModel.DeleteSelectedCommand.ExecuteAsync(null);
                    e.Handled = true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling key press in RecipesView DataGrid");
        }
    }

    // CORRECTED: Made async void and awaited the command
    private async void OnEditClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { DataContext: Models.RecipeDisplayModel displayModel } && DataContext is RecipesViewModel viewModel)
            {
                await viewModel.EditRecipe(displayModel.Recipe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling edit button click in RecipesView");
        }
    }

    // CORRECTED: Made async void and awaited the command
    private async void OnDeleteClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { DataContext: Models.RecipeDisplayModel displayModel } && DataContext is RecipesViewModel viewModel)
            {
                await viewModel.DeleteRecipe(displayModel.Recipe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling delete button click in RecipesView");
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is RecipesViewModel viewModel && sender is DataGrid dataGrid)
        {
            viewModel.SelectedRecipes.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                if (item is Models.RecipeDisplayModel displayModel)
                {
                    viewModel.SelectedRecipes.Add(displayModel);
                }
            }
        }
    }

    // Context Menu Handlers
    private async void OnViewCostHistory(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            try
            {
                var serviceProvider = App.Services;
                if (serviceProvider == null) return;

                var priceHistoryService = serviceProvider.GetRequiredService<IPriceHistoryService>();

                var dialog = new RecipeCostHistoryWindow();
                var dialogViewModel = new RecipeCostHistoryViewModel(viewModel.SelectedRecipe.Recipe, priceHistoryService);
                dialog.DataContext = dialogViewModel;

                await dialog.ShowDialog(window);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening cost history for recipe");
            }
        }
    }

    private async void OnViewCostBreakdown(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            try
            {
                var dialog = new CostBreakdownWindow();
                var dialogViewModel = new CostBreakdownViewModel(viewModel.SelectedRecipe.Recipe);
                dialog.DataContext = dialogViewModel;

                await dialog.ShowDialog(window);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening cost breakdown for recipe");
            }
        }
    }

    private async void OnEditRecipe(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                await viewModel.EditSelectedRecipeCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling edit recipe context menu in RecipesView");
        }
    }

    private async void OnDuplicateRecipe(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                await viewModel.DuplicateRecipeCommand.ExecuteAsync(viewModel.SelectedRecipe.Recipe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling duplicate recipe context menu in RecipesView");
        }
    }

    private async void OnPrintRecipe(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                await viewModel.GenerateRecipePdfCommand.ExecuteAsync(viewModel.SelectedRecipe.Recipe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling print recipe context menu in RecipesView");
        }
    }

    private async void OnDeleteRecipe(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is RecipesViewModel viewModel && viewModel.SelectedRecipe != null)
            {
                await viewModel.DeleteSelectedCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling delete recipe context menu in RecipesView");
        }
    }
}