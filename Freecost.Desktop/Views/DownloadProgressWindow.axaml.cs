using Avalonia.Controls;
using Freecost.Desktop.ViewModels;

namespace Freecost.Desktop.Views;

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
