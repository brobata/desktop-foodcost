using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dfc.Core.Models;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class IngredientSelectorWindow : Window
{
    public IngredientSelectorWindow()
    {
        InitializeComponent();
    }

    private void Border_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Ingredient ingredient)
        {
            if (DataContext is IngredientSelectorViewModel viewModel)
            {
                viewModel.SelectIngredientCommand.Execute(ingredient);
            }
        }
    }
}
