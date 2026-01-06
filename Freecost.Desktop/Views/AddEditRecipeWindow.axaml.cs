using Avalonia.Controls;
using Avalonia.Input;
using Freecost.Desktop.Controls;
using Freecost.Desktop.ViewModels;
using System;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class AddEditRecipeWindow : Window
{
    public bool WasSaved { get; private set; }

    public AddEditRecipeWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnKeyDown;
        Opened += OnWindowOpened;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // Focus the name text box when window opens for better UX
        NameTextBox?.Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not AddEditRecipeViewModel viewModel) return;

        // Ctrl+S to save
        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            if (viewModel.SaveCommand.CanExecute(null))
            {
                viewModel.SaveCommand.Execute(null);
                e.Handled = true;
            }
        }
        // Escape to cancel
        else if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            if (viewModel.CancelCommand.CanExecute(null))
            {
                viewModel.CancelCommand.Execute(null);
                e.Handled = true;
            }
        }
        // Ctrl+N to add ingredient
        else if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
        {
            if (viewModel.AddIngredientCommand.CanExecute(null))
            {
                viewModel.AddIngredientCommand.Execute(null);
                e.Handled = true;
            }
        }
        // Ctrl+P to generate PDF (only in edit mode)
        else if (e.Key == Key.P && e.KeyModifiers == KeyModifiers.Control && viewModel.IsEditMode)
        {
            if (viewModel.GeneratePdfCommand.CanExecute(null))
            {
                viewModel.GeneratePdfCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is AddEditRecipeViewModel viewModel)
        {
            viewModel.IngredientsChanged += OnIngredientsChanged;

            // Initial allergen detection
            DetectAndLoadAllergens(viewModel);
        }
    }

    private void OnIngredientsChanged(object? sender, EventArgs e)
    {
        if (DataContext is AddEditRecipeViewModel viewModel)
        {
            DetectAndLoadAllergens(viewModel);
        }
    }

    private void DetectAndLoadAllergens(AddEditRecipeViewModel viewModel)
    {
        var recipe = viewModel.GetRecipe();
        if (recipe == null) return;

        // Reset all allergens first
        foreach (var allergen in AllergenSelector.Allergens)
        {
            allergen.IsAutoDetected = false;
            allergen.IsSelected = false;
            allergen.SourceIngredients = null;
        }

        // Detect allergens from ingredients
        var detectedAllergens = viewModel.DetectAllergens(recipe);

        // Update allergen selector with detected allergens
        foreach (var (allergenType, sources) in detectedAllergens)
        {
            var allergenItem = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == allergenType);
            if (allergenItem != null)
            {
                allergenItem.IsAutoDetected = true;
                allergenItem.IsSelected = true;
                allergenItem.SourceIngredients = string.Join(", ", sources);
            }
        }

        // If editing existing recipe, restore saved allergen states (manual overrides)
        if (viewModel.ExistingRecipe != null)
        {
            var existingAllergens = viewModel.ExistingRecipe.RecipeAllergens;
            if (existingAllergens != null && existingAllergens.Any())
            {
                foreach (var ra in existingAllergens)
                {
                    var allergenItem = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == ra.Allergen.Type);
                    if (allergenItem != null && !allergenItem.IsAutoDetected)
                    {
                        allergenItem.IsSelected = ra.IsEnabled;
                    }
                }
            }
        }
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

    public AllergenSelectorControl GetAllergenSelector() => AllergenSelector;

    private void OnPhotoChanged(object? sender, string? newPhotoUrl)
    {
        if (DataContext is AddEditRecipeViewModel viewModel)
        {
            viewModel.PhotoUrl = newPhotoUrl;
        }
    }

    private void OnStepTextTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Get the TextBlock that was tapped
        if (sender is Avalonia.Controls.TextBlock textBlock && textBlock.DataContext is RecipeStepModel step)
        {
            // Enter edit mode
            step.IsEditing = true;

            // Focus the TextBox after a short delay to ensure it's visible
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                // Find the TextBox in the visual tree and focus it
                if (textBlock.Parent?.Parent is Avalonia.Controls.Panel panel)
                {
                    var textBox = panel.Children.OfType<Avalonia.Controls.TextBox>().FirstOrDefault();
                    textBox?.Focus();
                }
            }, Avalonia.Threading.DispatcherPriority.Background);
        }
    }

    private void OnStepTextBoxLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Get the TextBox that lost focus
        if (sender is Avalonia.Controls.TextBox textBox && textBox.DataContext is RecipeStepModel step)
        {
            // Exit edit mode
            step.IsEditing = false;
        }
    }
}
