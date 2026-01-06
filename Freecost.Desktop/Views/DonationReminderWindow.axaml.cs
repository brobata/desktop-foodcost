using Avalonia.Controls;
using Freecost.Desktop.ViewModels;

namespace Freecost.Desktop.Views;

public partial class DonationReminderWindow : Window
{
    public DonationReminderWindow()
    {
        InitializeComponent();
    }

    public DonationReminderWindow(DonationReminderViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
