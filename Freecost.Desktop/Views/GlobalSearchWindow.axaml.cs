using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;
using System;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class GlobalSearchWindow : Window
{
    public SearchResultItem? SelectedResult { get; private set; }

    public GlobalSearchWindow()
    {
        InitializeComponent();
        KeyDown += OnWindowKeyDown;
        Opened += OnWindowOpened;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // Focus search box when window opens
        var searchBox = this.FindControl<TextBox>("SearchBox");
        searchBox?.Focus();
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not GlobalSearchViewModel viewModel) return;

        // ESC to close
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

        // Enter to select and close
        if (e.Key == Key.Enter && viewModel.SelectedResult != null)
        {
            SelectedResult = viewModel.SelectedResult;
            Close();
            e.Handled = true;
            return;
        }

        // Arrow keys to navigate
        if (e.Key == Key.Down)
        {
            NavigateResults(1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            NavigateResults(-1);
            e.Handled = true;
            return;
        }
    }

    private void NavigateResults(int direction)
    {
        if (DataContext is not GlobalSearchViewModel viewModel) return;
        if (!viewModel.SearchResults.Any()) return;

        var currentIndex = viewModel.SelectedResult != null
            ? viewModel.SearchResults.IndexOf(viewModel.SelectedResult)
            : -1;

        var newIndex = currentIndex + direction;

        if (newIndex < 0)
            newIndex = viewModel.SearchResults.Count - 1;
        else if (newIndex >= viewModel.SearchResults.Count)
            newIndex = 0;

        viewModel.SelectedResult = viewModel.SearchResults[newIndex];
    }

    private void OnResultClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: SearchResultItem result })
        {
            if (DataContext is GlobalSearchViewModel viewModel)
            {
                viewModel.SelectedResult = result;
            }
        }
    }

    private void OnResultDoubleClicked(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: SearchResultItem result })
        {
            SelectedResult = result;
            Close();
        }
    }
}
