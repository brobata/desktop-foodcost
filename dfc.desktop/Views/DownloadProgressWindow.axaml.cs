using Avalonia.Controls;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class DownloadProgressWindow : Window
{
    public DownloadProgressWindow()
    {
        InitializeComponent();
    }

    public DownloadProgressWindow(UpdateNotificationViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
