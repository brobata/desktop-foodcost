// Location: Dfc.Desktop/Views/EnhancedSelectionDialog.axaml.cs
// Action: REPLACE entire file

using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class EnhancedSelectionDialog : Window
{
    public EnhancedSelectionResult? Result { get; private set; }

    public EnhancedSelectionDialog()
    {
        InitializeComponent();
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EnhancedSelectionDialogViewModel vm && vm.IsValid())
        {
            Result = new EnhancedSelectionResult
            {
                SelectedItem = vm.SelectedItem!,
                Quantity = vm.GetQuantity(),
                Unit = vm.SelectedUnit
            };
            Close(Result);
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Result = null;
        Close(null);
    }
}

public class EnhancedSelectionResult
{
    public SelectionItem SelectedItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public Dfc.Core.Enums.UnitType Unit { get; set; }
}