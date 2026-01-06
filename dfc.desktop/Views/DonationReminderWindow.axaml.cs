using Avalonia.Controls;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

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
