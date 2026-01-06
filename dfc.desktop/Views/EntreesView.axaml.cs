using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dfc.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Dfc.Desktop.Views;

public partial class EntreesView : UserControl
{
    private readonly ILogger? _logger;

    public EntreesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        _logger = App.Services?.GetService<ILogger<EntreesView>>();
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        KeyDown += OnViewKeyDown;
    }

    private async void OnViewKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (DataContext is not EntreesViewModel viewModel) return;

            // Ctrl+N = New entree
            if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
            {
                await viewModel.AddEntreeCommand.ExecuteAsync(null);
                e.Handled = true;
                return;
            }

            // Ctrl+D = Duplicate selected
            if (e.Key == Key.D && e.KeyModifiers == KeyModifiers.Control && viewModel.SelectedEntree != null)
            {
                await viewModel.DuplicateEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
                e.Handled = true;
                return;
            }

            // Ctrl+P = Print selected
            if (e.Key == Key.P && e.KeyModifiers == KeyModifiers.Control && viewModel.SelectedEntree != null)
            {
                await viewModel.GenerateEntreePdfCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
                e.Handled = true;
                return;
            }

            // Delete = Delete selected
            if (e.Key == Key.Delete && viewModel.SelectedEntree != null)
            {
                await viewModel.DeleteEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
                e.Handled = true;
                return;
            }

            // Enter or F2 = Edit selected
            if ((e.Key == Key.Enter || e.Key == Key.F2) && viewModel.SelectedEntree != null)
            {
                await viewModel.EditEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
                e.Handled = true;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling keyboard shortcut in EntreesView");
        }
    }

    private async void OnDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (DataContext is EntreesViewModel viewModel && viewModel.SelectedEntree != null)
            {
                await viewModel.EditEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling double-tap in EntreesView");
        }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is EntreesViewModel viewModel && sender is DataGrid dataGrid)
        {
            viewModel.SelectedEntrees.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                if (item is Models.EntreeDisplayModel displayModel)
                {
                    viewModel.SelectedEntrees.Add(displayModel);
                }
            }
        }
    }

    // Context Menu Handlers
    private async void OnViewCostBreakdown(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EntreesViewModel viewModel && viewModel.SelectedEntree != null)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            try
            {
                var dialog = new CostBreakdownWindow();
                var dialogViewModel = new CostBreakdownViewModel(viewModel.SelectedEntree.Entree);
                dialog.DataContext = dialogViewModel;

                await dialog.ShowDialog(window);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening cost breakdown for entree");
            }
        }
    }

    private async void OnEditEntree(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is EntreesViewModel viewModel && viewModel.SelectedEntree != null)
            {
                await viewModel.EditEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling edit entree context menu in EntreesView");
        }
    }

    private async void OnDeleteEntree(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is EntreesViewModel viewModel && viewModel.SelectedEntree != null)
            {
                await viewModel.DeleteEntreeCommand.ExecuteAsync(viewModel.SelectedEntree.Entree);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling delete entree context menu in EntreesView");
        }
    }
}