using Avalonia.Controls;

namespace Dfc.Desktop.Views;

public partial class ImportIngredientsWindow : Window
{
    public bool WasImported { get; private set; }

    public ImportIngredientsWindow()
    {
        InitializeComponent();
    }

    public void OnImportSuccess()
    {
        WasImported = true;
        Close();
    }

    public void OnCancel()
    {
        WasImported = false;
        Close();
    }
}
