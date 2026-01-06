using Avalonia.Controls;
using Avalonia.Input;
using Dfc.Desktop.ViewModels;
using System.Linq;

namespace Dfc.Desktop.Views;

public partial class ImportMapperWindow : Window
{
    public ImportMapperWindow()
    {
        InitializeComponent();

        // Set up drag and drop
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is ImportMapperViewModel vm && e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            var file = files?.FirstOrDefault();

            if (file != null)
            {
                var path = file.Path.LocalPath;
                if (path.EndsWith(".csv") || path.EndsWith(".xlsx") || path.EndsWith(".xls"))
                {
                    await vm.DropFileCommand.ExecuteAsync(path);
                }
            }
        }
    }
}
