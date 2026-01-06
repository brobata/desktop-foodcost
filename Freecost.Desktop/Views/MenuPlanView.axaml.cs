using Avalonia.Controls;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;
using System.Threading.Tasks;

namespace Freecost.Desktop.Views;

public partial class MenuPlanView : UserControl
{
    public MenuPlanView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MenuPlanViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }

    private void OnMenuPlanSelected(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MenuPlanModel menuPlan && DataContext is MenuPlanViewModel viewModel)
        {
            viewModel.SelectedMenuPlan = menuPlan;
        }
    }
}
