using Avalonia.Controls;

namespace Freecost.Desktop.Views;

public partial class AddRecipeIngredientWindow : Window
{
    public bool WasSaved { get; private set; }

    public AddRecipeIngredientWindow()
    {
        InitializeComponent();
    }

    public void OnSaveSuccess()
    {
        WasSaved = true;
        Close();
    }

    public void OnCancel()
    {
        WasSaved = false;
        Close();
    }
}