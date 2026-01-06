using Avalonia.Controls;

namespace Freecost.Desktop.Views;

public partial class BulkImportWindow : Window
{
    public bool WasImported { get; private set; }

    public BulkImportWindow()
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
