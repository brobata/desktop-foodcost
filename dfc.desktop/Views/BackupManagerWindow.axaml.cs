using Avalonia.Controls;
using Dfc.Desktop.ViewModels;
using System.Threading.Tasks;

namespace Dfc.Desktop.Views;

public partial class BackupManagerWindow : Window
{
    public BackupManagerWindow()
    {
        InitializeComponent();
    }

    public BackupManagerWindow(BackupManagerViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (DataContext is BackupManagerViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
