using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Dfc.Desktop.Views;

public partial class CostTrendAnalysisWindow : Window
{
    public CostTrendAnalysisWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
