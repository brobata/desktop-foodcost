using Avalonia.Controls;
using Freecost.Desktop.ViewModels;

namespace Freecost.Desktop.Views;

public partial class UpdateNotificationWindow : Window
{
    public UpdateNotificationWindow()
    {
        InitializeComponent();
    }

    public UpdateNotificationWindow(UpdateNotificationViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
