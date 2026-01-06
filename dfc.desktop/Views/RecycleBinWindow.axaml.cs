using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;
using System;

namespace Dfc.Desktop.Views;

public partial class RecycleBinWindow : Window
{
    public RecycleBinWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBinWindow] Window loaded, attempting to load deleted items");

            if (DataContext is RecycleBinViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[RecycleBinWindow] DataContext is RecycleBinViewModel, loading items...");
                await viewModel.LoadDeletedItemsAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RecycleBinWindow] WARNING: DataContext is not RecycleBinViewModel! Type: {DataContext?.GetType().Name ?? "null"}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecycleBinWindow] Error in OnLoaded: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
