using Avalonia.Controls;
using Dfc.Desktop.Controls;
using Dfc.Desktop.ViewModels;
using System;
using System.Linq;

namespace Dfc.Desktop.Views;

public partial class AddEditEntreeWindow : Window
{
    public bool WasSaved { get; private set; }

    public AddEditEntreeWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // When the view model is set, detect allergens
        if (DataContext is AddEditEntreeViewModel viewModel)
        {
            viewModel.ComponentsChanged += OnComponentsChanged;

            // Initial allergen detection
            DetectAndLoadAllergens(viewModel);
        }
    }

    private void OnComponentsChanged(object? sender, EventArgs e)
    {
        // Re-detect allergens when components change
        if (DataContext is AddEditEntreeViewModel viewModel)
        {
            DetectAndLoadAllergens(viewModel);
        }
    }

    private void DetectAndLoadAllergens(AddEditEntreeViewModel viewModel)
    {
        // Build a temp entree with current components for detection
        var tempEntree = viewModel.GetEntree();
        if (tempEntree == null) return;

        // Reset all allergens first
        foreach (var allergen in AllergenSelector.Allergens)
        {
            allergen.IsAutoDetected = false;
            allergen.IsSelected = false;
            allergen.SourceIngredients = null;
        }

        // Detect allergens from current components
        var detectedAllergens = viewModel.DetectAllergens(tempEntree);

        // Update allergen selector with detected allergens
        foreach (var (allergenType, sources) in detectedAllergens)
        {
            var allergenItem = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == allergenType);
            if (allergenItem != null)
            {
                allergenItem.IsAutoDetected = true;
                allergenItem.IsSelected = true; // Auto-select detected allergens
                allergenItem.SourceIngredients = string.Join(", ", sources);
            }
        }

        // If editing existing entree, restore saved allergen states (manual overrides)
        if (viewModel.ExistingEntree != null)
        {
            var existingAllergens = viewModel.ExistingEntree.EntreeAllergens;
            if (existingAllergens != null && existingAllergens.Any())
            {
                foreach (var ea in existingAllergens)
                {
                    var allergenItem = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == ea.Allergen.Type);
                    if (allergenItem != null && !allergenItem.IsAutoDetected)
                    {
                        // Only restore manual selections (not auto-detected ones)
                        allergenItem.IsSelected = ea.IsEnabled;
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

    private void OnPhotoChanged(object? sender, string photoUrl)
    {
        if (DataContext is AddEditEntreeViewModel viewModel)
        {
            viewModel.PhotoUrl = photoUrl;
        }
    }

    private void OnStepTextTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Get the TextBlock that was tapped
        if (sender is Avalonia.Controls.TextBlock textBlock && textBlock.DataContext is PreparationStepModel step)
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
        if (sender is Avalonia.Controls.TextBox textBox && textBox.DataContext is PreparationStepModel step)
        {
            // Exit edit mode
            step.IsEditing = false;
        }
    }

    public AllergenSelectorControl GetAllergenSelector() => AllergenSelector;
}
