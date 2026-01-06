using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class SelectionDialog : Window
{
    public SelectionDialog()
    {
        InitializeComponent();
    }

    private void OnSelect(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SelectionDialogViewModel vm)
        {
            var listBox = this.FindControl<ListBox>("ItemsListBox");
            if (listBox?.SelectedItem is SelectionItem selected)
            {
                Close(selected);
            }
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}