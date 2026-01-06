using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dfc.Desktop.ViewModels;
using Dfc.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Dfc.Desktop.Views;

public partial class IngredientsView : UserControl
{
    private readonly ILogger? _logger;

    public IngredientsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        _logger = App.Services?.GetService<ILogger<IngredientsView>>();
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        KeyDown += OnViewKeyDown;
        // PointerPressed += OnViewPointerPressed; // Disabled: was preventing text selection
    }

    private void OnViewPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // If click is not on the DataGrid, clear selection
        var dataGrid = this.FindControl<DataGrid>("IngredientsDataGrid");
        if (dataGrid != null && DataContext is IngredientsViewModel viewModel)
        {
            var point = e.GetPosition(dataGrid);
            var hitTest = dataGrid.InputHitTest(point);

            // If click is outside the DataGrid bounds, clear selection
            if (hitTest == null || !dataGrid.Bounds.Contains(point))
            {
                dataGrid.SelectedItem = null;
                viewModel.SelectedIngredient = null;
            }
        }
    }

    private async void OnViewKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (DataContext is not IngredientsViewModel viewModel) return;

            // Ctrl+N = New ingredient
            if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
            {
                await viewModel.AddIngredientCommand.ExecuteAsync(null);
                e.Handled = true;
                return;
            }

            // Ctrl+D = Duplicate selected (batch or single)
            if (e.Key == Key.D && e.KeyModifiers == KeyModifiers.Control)
            {
                if (viewModel.SelectedIngredients.Count > 1)
                {
                    await viewModel.BatchDuplicateCommand.ExecuteAsync(null);
                }
                else if (viewModel.SelectedIngredient != null)
                {
                    await viewModel.DuplicateIngredientCommand.ExecuteAsync(viewModel.SelectedIngredient.Ingredient);
                }
                e.Handled = true;
                return;
            }

            // Ctrl+I = Bulk import
            if (e.Key == Key.I && e.KeyModifiers == KeyModifiers.Control)
            {
                await viewModel.BulkImportCommand.ExecuteAsync(null);
                e.Handled = true;
                return;
            }

            // Ctrl+A = Select all
            if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
            {
                viewModel.SelectAllCommand.Execute(null);
                e.Handled = true;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling keyboard shortcut in IngredientsView");
        }
    }

    private async void OnDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (DataContext is IngredientsViewModel viewModel && viewModel.SelectedIngredient != null)
            {
                await viewModel.EditIngredientCommand.ExecuteAsync(viewModel.SelectedIngredient.Ingredient);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling double-tap in IngredientsView");
        }
    }

    private async void OnDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (DataContext is not IngredientsViewModel viewModel) return;

            switch (e.Key)
            {
                case Key.Enter when viewModel.SelectedIngredient != null:
                    await viewModel.EditIngredientCommand.ExecuteAsync(viewModel.SelectedIngredient.Ingredient);
                    e.Handled = true;
                    break;

                case Key.Delete when viewModel.SelectedIngredients.Count > 0:
                    // Batch delete if multiple selected, otherwise single delete
                    if (viewModel.SelectedIngredients.Count > 1)
                    {
                        await viewModel.BatchDeleteCommand.ExecuteAsync(null);
                    }
                    else if (viewModel.SelectedIngredient != null)
                    {
                        await viewModel.DeleteIngredientCommand.ExecuteAsync(viewModel.SelectedIngredient.Ingredient);
                    }
                    e.Handled = true;
                    break;

                case Key.F2 when viewModel.SelectedIngredient != null:
                    await viewModel.EditIngredientCommand.ExecuteAsync(viewModel.SelectedIngredient.Ingredient);
                    e.Handled = true;
                    break;

                case Key.Insert:
                    await viewModel.AddIngredientCommand.ExecuteAsync(null);
                    e.Handled = true;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling DataGrid keyboard input in IngredientsView");
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is IngredientsViewModel viewModel && sender is DataGrid dataGrid)
        {
            viewModel.SelectedIngredients.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                if (item is Models.IngredientDisplayModel displayModel)
                {
                    viewModel.SelectedIngredients.Add(displayModel);
                }
            }
        }
    }
}