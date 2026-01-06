using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Freecost.Desktop.Views;

public partial class SubstitutionsGuideWindow : Window
{
    public SubstitutionsGuideWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
