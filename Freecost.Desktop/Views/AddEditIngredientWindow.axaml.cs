using Avalonia.Controls;
using Freecost.Core.Services;
using Freecost.Desktop.Controls;
using Freecost.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.Views;

public partial class AddEditIngredientWindow : Window
{
    public bool WasSaved { get; private set; }
    private AddEditIngredientViewModel? _viewModel;
    private readonly IAllergenDetectionService? _allergenDetectionService;

    public AddEditIngredientWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        // Get allergen detection service from DI
        _allergenDetectionService = App.Services?.GetService(typeof(IAllergenDetectionService)) as IAllergenDetectionService;
    }

    public void SetViewModel(AddEditIngredientViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // When editing an existing ingredient, load its allergens OR auto-detect
        if (DataContext is AddEditIngredientViewModel viewModel)
        {
            if (viewModel.ExistingIngredient != null)
            {
                // Editing existing - load saved allergens
                var existingAllergens = viewModel.ExistingIngredient.IngredientAllergens;
                if (existingAllergens != null && existingAllergens.Any())
                {
                    var allergenViewModels = existingAllergens.Select(ia => new AllergenItemViewModel(
                        ia.Allergen.Type,
                        () => { })
                    {
                        IsSelected = ia.IsEnabled,
                        IsAutoDetected = ia.IsAutoDetected,
                        SourceIngredients = ia.SourceIngredients
                    }).ToList();

                    // Set allergens in the selector control
                    foreach (var avm in allergenViewModels)
                    {
                        var existing = AllergenSelector.Allergens.FirstOrDefault(a => a.AllergenType == avm.AllergenType);
                        if (existing != null)
                        {
                            existing.IsSelected = avm.IsSelected;
                            existing.IsAutoDetected = avm.IsAutoDetected;
                            existing.SourceIngredients = avm.SourceIngredients;
                        }
                    }
                }
                else
                {
                    // Existing ingredient but no saved allergens - auto-detect
                    AutoDetectAllergensForIngredient(viewModel.ExistingIngredient.Name);
                }
            }
            else
            {
                // New ingredient - will auto-detect when user types name
                // Subscribe to name changes
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(AddEditIngredientViewModel.Name))
                    {
                        AutoDetectAllergensForIngredient(viewModel.Name);
                    }
                };
            }
        }
    }

    private void AutoDetectAllergensForIngredient(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || _allergenDetectionService == null)
            return;

        try
        {
            // Detect allergens from ingredient name
            var detectedAllergens = _allergenDetectionService.DetectAllergensFromIngredient(ingredientName);

            if (detectedAllergens.Any())
            {
                // Build dictionary for SetAutoDetectedAllergens
                var allergenDict = new Dictionary<Core.Enums.AllergenType, List<string>>();
                foreach (var allergenType in detectedAllergens)
                {
                    allergenDict[allergenType] = new List<string> { ingredientName };
                }

                // Update allergen selector control
                AllergenSelector.SetAutoDetectedAllergens(allergenDict);

                System.Diagnostics.Debug.WriteLine($"[AddEditIngredient] Auto-detected {detectedAllergens.Count} allergens for '{ingredientName}'");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditIngredient] Error auto-detecting allergens: {ex.Message}");
        }
    }

    public void OnSaveSuccess()
    {
        WasSaved = true;
        Close();
    }

    public void OnCancel()
    {
        Close();
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Get allergens from the selector and set them on the ViewModel before saving
        if (_viewModel != null)
        {
            var selectedAllergens = AllergenSelector.GetSelectedAllergens();
            _viewModel.SetAllergens(selectedAllergens);
        }

        // Now execute the Save command
        if (DataContext is AddEditIngredientViewModel viewModel && viewModel.SaveCommand.CanExecute(null))
        {
            viewModel.SaveCommand.Execute(null);
        }
    }
}