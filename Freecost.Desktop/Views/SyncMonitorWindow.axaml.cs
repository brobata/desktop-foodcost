using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Freecost.Desktop.Views;

public partial class SyncMonitorWindow : Window
{
    public SyncMonitorWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
