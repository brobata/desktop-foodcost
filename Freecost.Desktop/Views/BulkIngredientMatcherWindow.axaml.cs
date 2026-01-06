using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;

namespace Freecost.Desktop.Views;

public partial class BulkIngredientMatcherWindow : Window
{
    public BulkIngredientMatcherWindow()
    {
        InitializeComponent();
    }

    private void AvailableItem_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is BulkIngredientMatcherViewModel viewModel &&
            sender is Border border &&
            border.DataContext is MatchableItem item)
        {
            viewModel.MatchCommand.Execute(item);
        }
    }
}
