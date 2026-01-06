using Avalonia.Controls;
using Avalonia.Interactivity;
using Freecost.Desktop.ViewModels;
using Location = Freecost.Core.Models.Location;

namespace Freecost.Desktop.Views;

public partial class LocationSelectorWindow : Window
{
    public Location? SelectedLocation { get; private set; }

    public LocationSelectorWindow()
    {
        InitializeComponent();
    }

    public void SetSelectedLocation(Location? location)
    {
        SelectedLocation = location;
        Close();
    }

    private void OnLocationDoubleClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LocationSelectorViewModel viewModel)
        {
            viewModel.OnLocationActivated();
        }
    }
}
