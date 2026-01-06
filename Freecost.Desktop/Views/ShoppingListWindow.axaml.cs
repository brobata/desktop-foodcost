using Avalonia.Controls;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;
using System.Threading.Tasks;

namespace Freecost.Desktop.Views;

public partial class ShoppingListWindow : Window
{
    public ShoppingListWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShoppingListViewModel viewModel)
        {
            await viewModel.LoadShoppingListAsync();
        }
    }
}
