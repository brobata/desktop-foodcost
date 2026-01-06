using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Dfc.Desktop.Views;

public partial class KeyboardShortcutsHelpWindow : Window
{
    public KeyboardShortcutsHelpWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
