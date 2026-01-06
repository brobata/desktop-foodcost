using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Dfc.Desktop.Views;

public partial class RecipeCostAnalysisWindow : Window
{
    public RecipeCostAnalysisWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
