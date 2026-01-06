using Avalonia.Controls;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

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
