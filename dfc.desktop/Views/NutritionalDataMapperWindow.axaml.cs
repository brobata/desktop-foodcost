using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Core.Models;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class NutritionalDataMapperWindow : Window
{
    public NutritionalDataMapperWindow()
    {
        InitializeComponent();
    }

    public NutritionalDataMapperWindow(NutritionalDataMapperViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// Handle suggestion button clicks - maps the ingredient to the selected nutritional data
    /// </summary>
    private void OnSuggestionClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (DataContext is not NutritionalDataMapperViewModel viewModel) return;

        // Tag contains the IngredientMappingItem (parent data context)
        var mappingItem = button.Tag as IngredientMappingItem;
        // CommandParameter contains the NutritionalDataResult (button's data context)
        var suggestion = button.CommandParameter as NutritionalDataResult;

        if (mappingItem != null && suggestion != null)
        {
            viewModel.SelectSuggestionCommand.Execute(new object[] { mappingItem, suggestion });
        }
    }
}
