using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Dfc.Desktop.Views;

public partial class RecipeComparisonWindow : Window
{
    public RecipeComparisonWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
