using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Dfc.Desktop.Views;

public partial class PriceHistoryWindow : Window
{
    public PriceHistoryWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
