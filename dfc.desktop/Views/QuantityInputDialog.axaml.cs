using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class QuantityInputDialog : Window
{
    public QuantityInputResult? Result { get; private set; }

    public QuantityInputDialog()
    {
        InitializeComponent();
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        if (DataContext is QuantityInputDialogViewModel vm && vm.IsValid())
        {
            Result = new QuantityInputResult
            {
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

public class QuantityInputResult
{
    public decimal Quantity { get; set; }
    public Dfc.Core.Enums.UnitType Unit { get; set; }
}