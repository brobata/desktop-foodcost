using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dfc.Core.Models;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class EntreeCardImportWindow : Window
{
    public EntreeCardImportWindow()
    {
        InitializeComponent();
    }

    public EntreeCardImportWindow(EntreeCardImportViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    /// <summary>
    /// Handle ingredient suggestion button clicks - maps the unmatched ingredient to the suggestion
    /// </summary>
    private void OnIngredientSuggestionClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (DataContext is not EntreeCardImportViewModel viewModel) return;

        // Tag contains the UnmatchedIngredient (parent data context)
        var unmatchedIngredient = button.Tag as UnmatchedIngredient;
        // CommandParameter contains the IngredientMatchSuggestion (button's data context)
        var suggestion = button.CommandParameter as IngredientMatchSuggestion;

        if (unmatchedIngredient != null && suggestion != null)
        {
            viewModel.MapIngredientWithContext(unmatchedIngredient, suggestion);
        }
    }

    /// <summary>
    /// Handle recipe suggestion button clicks - maps the unmatched recipe to the suggestion
    /// </summary>
    private void OnRecipeSuggestionClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (DataContext is not EntreeCardImportViewModel viewModel) return;

        // Tag contains the UnmatchedRecipe (parent data context)
        var unmatchedRecipe = button.Tag as UnmatchedRecipe;
        // CommandParameter contains the RecipeMatchSuggestion (button's data context)
        var suggestion = button.CommandParameter as RecipeMatchSuggestion;

        if (unmatchedRecipe != null && suggestion != null)
        {
            viewModel.MapRecipeWithContext(unmatchedRecipe, suggestion);
        }
    }
}
