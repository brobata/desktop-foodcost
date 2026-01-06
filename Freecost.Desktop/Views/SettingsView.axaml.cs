using Avalonia.Controls;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;

namespace Freecost.Desktop.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void OnShowSubstitutionsGuideClicked(object? sender, RoutedEventArgs e)
    {
        // Get parent window
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        var dialog = new SubstitutionsGuideWindow();
        var viewModel = new SubstitutionsGuideViewModel();
        dialog.DataContext = viewModel;
        await dialog.ShowDialog(window);
    }
}