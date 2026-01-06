using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dfc.Core.Models;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class RecipeCardImportWindow : Window
{
    public RecipeCardImportWindow()
    {
        InitializeComponent();
    }

    public RecipeCardImportWindow(RecipeCardImportViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// Handle suggestion button clicks - maps the unmatched ingredient to the suggestion
    /// </summary>
    private void OnSuggestionClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (DataContext is not RecipeCardImportViewModel viewModel) return;

        // Tag contains the UnmatchedIngredient (parent data context)
        var unmatchedIngredient = button.Tag as UnmatchedIngredient;
        // CommandParameter contains the IngredientMatchSuggestion (button's data context)
        var suggestion = button.CommandParameter as IngredientMatchSuggestion;

        if (unmatchedIngredient != null && suggestion != null)
        {
            viewModel.MapIngredientWithContext(unmatchedIngredient, suggestion);
        }
    }
}
