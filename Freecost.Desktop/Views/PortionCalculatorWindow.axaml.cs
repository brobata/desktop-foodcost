using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Freecost.Desktop.Views;

public partial class PortionCalculatorWindow : Window
{
    public PortionCalculatorWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
