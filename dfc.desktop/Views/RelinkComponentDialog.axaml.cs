using Avalonia.Controls;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class RelinkComponentDialog : Window
{
    public RelinkComponentResult? Result { get; private set; }

    public RelinkComponentDialog()
    {
        InitializeComponent();
    }

    public void SetResult(RelinkComponentResult? result)
    {
        Result = result;
        Close(result);
    }
}

public class RelinkComponentResult
{
    public SelectionItem SelectedItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public Dfc.Core.Enums.UnitType Unit { get; set; }
    public bool IsIngredient { get; set; }
}
