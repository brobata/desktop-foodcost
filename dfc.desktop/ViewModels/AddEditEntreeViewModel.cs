// Location: Dfc.Desktop/ViewModels/AddEditEntreeViewModel.cs
// Action: REPLACE entire file

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Enums;
using Dfc.Core.Helpers;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class ComponentDisplayModel : ObservableObject
{
    public object Component { get; }
    public string Name { get; }
    public string QuantityDisplay { get; }
    public decimal Cost { get; private set; }

    [ObservableProperty]
    private bool _hasMapping;

    [ObservableProperty]
    private Guid? _mappingId;

    // Track if this component points to a placeholder (unmatched import)
    public bool IsPlaceholder { get; private set; }

    // True if connected to a real ingredient/recipe, false if placeholder
    public bool IsConnected => !IsPlaceholder;

    public ComponentDisplayModel(EntreeRecipe er)
    {
        Component = er;
        // Use DisplayName if available (from entree card import), otherwise use full recipe name
        Name = !string.IsNullOrWhiteSpace(er.DisplayName) ? er.DisplayName : er.Recipe.Name;

        // Handle "To Taste" special case (0.01 tsp)
        if (er.Quantity == 0.01m && er.Unit == UnitType.Teaspoon)
        {
            QuantityDisplay = "To Taste";
        }
        else
        {
            // Format quantity: show whole numbers without decimals (e.g., "2" not "2.00")
            var qtyFormat = er.Quantity % 1 == 0 ? er.Quantity.ToString("0") : er.Quantity.ToString("0.##");
            var unitAbbrev = er.Unit != UnitType.Each ? $" {GetUnitAbbreviation(er.Unit)}" : " portion(s)";
            QuantityDisplay = $"{qtyFormat}{unitAbbrev}";
        }

        // Check if this is a placeholder recipe from import
        IsPlaceholder = er.Recipe.Category == "[UNMATCHED - Import]";

        Cost = CalculateRecipeCost(er);
    }

    public ComponentDisplayModel(EntreeIngredient ei, decimal cost)
    {
        Component = ei;
        // Use DisplayName if available (from entree card import), otherwise use full ingredient name
        Name = !string.IsNullOrWhiteSpace(ei.DisplayName) ? ei.DisplayName : ei.Ingredient.Name;

        // Handle "To Taste" special case (0.01 tsp)
        if (ei.Quantity == 0.01m && ei.Unit == UnitType.Teaspoon)
        {
            QuantityDisplay = "To Taste";
        }
        else
        {
            // Format quantity: show whole numbers without decimals (e.g., "2" not "2.00")
            var qtyFormat = ei.Quantity % 1 == 0 ? ei.Quantity.ToString("0") : ei.Quantity.ToString("0.##");
            QuantityDisplay = $"{qtyFormat} {GetUnitAbbreviation(ei.Unit)}";
        }
        Cost = cost;

        // Check if this is a placeholder ingredient from import
        IsPlaceholder = ei.Ingredient.Category == "[UNMATCHED - Import]";
    }

    private decimal CalculateRecipeCost(EntreeRecipe er)
    {
        try
        {
            if (er.Unit == UnitType.Each)
            {
                return er.Recipe.Yield > 0 ? (er.Recipe.TotalCost / er.Recipe.Yield) * er.Quantity : 0;
            }

            if (!Enum.TryParse<UnitType>(er.Recipe.YieldUnit, true, out var yieldUnitType))
            {
                yieldUnitType = er.Recipe.YieldUnit.ToLower() switch
                {
                    "oz" or "ounce" or "ounces" => UnitType.Ounce,
                    "lb" or "pound" or "pounds" => UnitType.Pound,
                    "g" or "gram" or "grams" => UnitType.Gram,
                    "kg" or "kilogram" or "kilograms" => UnitType.Kilogram,
                    "cup" or "cups" => UnitType.Cup,
                    "pint" or "pints" or "pt" => UnitType.Pint,
                    "quart" or "quarts" or "qt" => UnitType.Quart,
                    "gallon" or "gallons" or "gal" => UnitType.Gallon,
                    "ml" or "milliliter" or "milliliters" => UnitType.Milliliter,
                    "liter" or "liters" or "l" => UnitType.Liter,
                    "fl oz" or "floz" or "fluid ounce" or "fluid ounces" => UnitType.FluidOunce,
                    "tbsp" or "tablespoon" or "tablespoons" => UnitType.Tablespoon,
                    "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
                    _ => UnitType.Each
                };
            }

            decimal convertedQuantity;
            if (er.Unit == yieldUnitType)
            {
                convertedQuantity = er.Quantity;
            }
            else if (UnitConverter.CanConvert(er.Unit, yieldUnitType))
            {
                try
                {
                    convertedQuantity = UnitConverter.Convert(er.Quantity, er.Unit, yieldUnitType);
                }
                catch
                {
                    // Conversion failed (e.g., volume to weight without density)
                    // Fall back to simple multiplication
                    return er.Recipe.TotalCost * er.Quantity;
                }
            }
            else
            {
                // Units are incompatible - fall back to simple multiplication
                return er.Recipe.TotalCost * er.Quantity;
            }

            return er.Recipe.Yield > 0 ? (convertedQuantity / er.Recipe.Yield) * er.Recipe.TotalCost : 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating recipe cost: {ex.Message}");
            // Fall back to simple cost calculation
            return er.Recipe.TotalCost * er.Quantity;
        }
    }

    private string GetUnitAbbreviation(UnitType unit)
    {
        return unit switch
        {
            UnitType.Gram => "g",
            UnitType.Kilogram => "kg",
            UnitType.Ounce => "oz",
            UnitType.Pound => "lb",
            UnitType.Milliliter => "mL",
            UnitType.Liter => "L",
            UnitType.FluidOunce => "fl oz",
            UnitType.Cup => "cup",
            UnitType.Pint => "pt",
            UnitType.Quart => "qt",
            UnitType.Gallon => "gal",
            UnitType.Tablespoon => "tbsp",
            UnitType.Teaspoon => "tsp",
            UnitType.Each => "ea",
            _ => unit.ToString()
        };
    }
}

public partial class PreparationStepModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isEditing = false;

    public string StepNumber { get; set; } = string.Empty;
}

public partial class AddEditEntreeViewModel : ViewModelBase
{
    private readonly IEntreeService _entreeService;
    private readonly IRecipeService _recipeService;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeCostCalculator _costCalculator;
    private readonly IPhotoService _photoService;
    private readonly IAllergenDetectionService _allergenDetectionService;
    private readonly IValidationService _validationService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IIngredientMatchMappingService? _mappingService;
    private readonly Window _owner;
    private readonly Action _onSaveSuccess;
    private readonly Action _onCancel;
    private readonly Entree? _existingEntree;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _category = string.Empty;
    [ObservableProperty] private string _menuPrice = string.Empty;
    [ObservableProperty] private string _platingEquipment = string.Empty;
    [ObservableProperty] private string _preparationInstructions = string.Empty;
    [ObservableProperty] private string? _photoUrl;
    [ObservableProperty] private ObservableCollection<ComponentDisplayModel> _components = new();
    [ObservableProperty] private ObservableCollection<PreparationStepModel> _preparationSteps = new();
    [ObservableProperty] private ValidationResult? _validationResult;
    [ObservableProperty] private bool _isSaving;

    // Photo service for upload control (passed from DI)
    public IPhotoService PhotoService => _photoService;

    // Allergen detection service
    public IAllergenDetectionService AllergenDetectionService => _allergenDetectionService;

    // Existing entree (for loading allergens when editing)
    public Entree? ExistingEntree => _existingEntree;

    // Event fired when components are added/removed/edited
    public event EventHandler? ComponentsChanged;

    public string DialogTitle => _existingEntree == null ? "Add Entree" : "Edit Entree";
    public decimal TotalCost => Components.Sum(c => c.Cost);
    public string TotalCostDisplay => $"Cost: {TotalCost:C2}";

    public string FoodCostDisplay
    {
        get
        {
            if (decimal.TryParse(MenuPrice, out var price) && price > 0 && TotalCost > 0)
            {
                var foodCost = (TotalCost / price) * 100;
                return $"FC: {foodCost:F2}%";
            }
            return "FC: 0.00%";
        }
    }

    // Calculated nutrition properties (aggregated from recipes and ingredients)
    public string CalculatedCalories
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Calories.ToString("F1");
        }
    }

    public string CalculatedProtein
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Protein.ToString("F1");
        }
    }

    public string CalculatedCarbohydrates
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Carbohydrates.ToString("F1");
        }
    }

    public string CalculatedFat
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Fat.ToString("F1");
        }
    }

    public string CalculatedFiber
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Fiber.ToString("F1");
        }
    }

    public string CalculatedSugar
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Sugar.ToString("F1");
        }
    }

    public string CalculatedSodium
    {
        get
        {
            var entree = GetEntree();
            if (entree == null) return "0";
            return entree.CalculatedNutrition.Sodium.ToString("F1");
        }
    }

    public AddEditEntreeViewModel(
        IEntreeService entreeService,
        IRecipeService recipeService,
        IIngredientService ingredientService,
        IRecipeCostCalculator costCalculator,
        IPhotoService photoService,
        IAllergenDetectionService allergenDetectionService,
        IValidationService validationService,
        ICurrentLocationService currentLocationService,
        Window owner,
        Action onSaveSuccess,
        Action onCancel,
        Entree? entree = null,
        IIngredientMatchMappingService? mappingService = null)
    {
        _entreeService = entreeService;
        _recipeService = recipeService;
        _ingredientService = ingredientService;
        _costCalculator = costCalculator;
        _photoService = photoService;
        _allergenDetectionService = allergenDetectionService;
        _validationService = validationService;
        _currentLocationService = currentLocationService;
        _mappingService = mappingService;
        _owner = owner;
        _onSaveSuccess = onSaveSuccess;
        _onCancel = onCancel;
        _existingEntree = entree;

        if (_existingEntree != null)
        {
            Name = _existingEntree.Name;
            Category = _existingEntree.Category ?? string.Empty;
            MenuPrice = _existingEntree.MenuPrice?.ToString("F2") ?? string.Empty;
            PlatingEquipment = _existingEntree.PlatingEquipment ?? string.Empty;
            PreparationInstructions = _existingEntree.PreparationInstructions ?? string.Empty;
            PhotoUrl = _existingEntree.PhotoUrl;
            _ = LoadComponentsAsync(); // Fire and forget - components will load async
            LoadPreparationSteps();
        }
        else
        {
            // New entree - initialize with 2 empty steps
            InitializePreparationSteps(2);
        }
    }

    private void InitializePreparationSteps(int count)
    {
        PreparationSteps.Clear();
        for (int i = 1; i <= count; i++)
        {
            PreparationSteps.Add(new PreparationStepModel
            {
                StepNumber = $"{i}.",
                Description = string.Empty
            });
        }
    }

    private void LoadPreparationSteps()
    {
        PreparationSteps.Clear();
        
        if (string.IsNullOrWhiteSpace(PreparationInstructions))
        {
            // No existing steps - initialize with 2 empty steps
            InitializePreparationSteps(2);
            return;
        }

        var lines = PreparationInstructions.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int stepNum = 1;
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                // Remove existing numbering if present
                var description = System.Text.RegularExpressions.Regex.Replace(trimmed, @"^\d+\.\s*", "");
                
                PreparationSteps.Add(new PreparationStepModel
                {
                    StepNumber = $"{stepNum}.",
                    Description = description
                });
                stepNum++;
            }
        }
    }

    private async Task LoadComponentsAsync()
    {
        try
        {
            if (_existingEntree == null) return;

            foreach (var er in _existingEntree.EntreeRecipes)
            {
                Components.Add(new ComponentDisplayModel(er));
            }

            foreach (var ei in _existingEntree.EntreeIngredients)
            {
                var cost = await _costCalculator.CalculateIngredientCostAsync(
                    new RecipeIngredient
                    {
                        Ingredient = ei.Ingredient,
                        IngredientId = ei.IngredientId,
                        Quantity = ei.Quantity,
                        Unit = ei.Unit
                    });
                Components.Add(new ComponentDisplayModel(ei, cost));
            }

            NotifyCostChanged();

            // Load mapping status for all components
            _ = LoadMappingStatusAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading components: {ex.Message}");
        }
    }

    private async Task LoadMappingStatusAsync()
    {
        if (_mappingService == null) return;

        try
        {
            foreach (var componentDisplay in Components)
            {
                // Check mapping based on the component's display name
                var mapping = await _mappingService.GetMappingForNameAsync(
                    componentDisplay.Name,
                    _currentLocationService.CurrentLocationId);

                if (mapping != null)
                {
                    componentDisplay.HasMapping = true;
                    componentDisplay.MappingId = mapping.Id;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading mapping status: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddRecipeComponent()
    {
        try
        {
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);

            var selectableItems = recipes.Select(r => new SelectionItem
            {
                Id = r.Id,
                Name = r.Name,
                Data = r
            }).ToList();

            var dialog = new EnhancedSelectionDialog();
            var viewModel = new EnhancedSelectionDialogViewModel(
                selectableItems,
                "Select a Recipe",
                UnitType.Each
            );

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_owner);

            if (result != null)
            {
                var selectedRecipe = (Recipe)result.SelectedItem.Data;
                var newComponent = new EntreeRecipe
                {
                    Recipe = selectedRecipe,
                    RecipeId = selectedRecipe.Id,
                    Quantity = result.Quantity,
                    Unit = result.Unit
                };

                Components.Add(new ComponentDisplayModel(newComponent));
                NotifyCostChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding recipe component: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddIngredientComponent()
    {
        try
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);

            var selectableItems = ingredients.Select(i => new SelectionItem
            {
                Id = i.Id,
                Name = i.Name,
                Data = i
            }).ToList();

            var dialog = new EnhancedSelectionDialog();
            var viewModel = new EnhancedSelectionDialogViewModel(
                selectableItems,
                "Select an Ingredient",
                UnitType.Gram
            );

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_owner);

            if (result != null)
            {
                var selectedIngredient = (Ingredient)result.SelectedItem.Data;
                var newComponent = new EntreeIngredient
                {
                    Ingredient = selectedIngredient,
                    IngredientId = selectedIngredient.Id,
                    Quantity = result.Quantity,
                    Unit = result.Unit
                };

                var cost = await _costCalculator.CalculateIngredientCostAsync(
                    new RecipeIngredient
                    {
                        Ingredient = newComponent.Ingredient,
                        IngredientId = newComponent.IngredientId,
                        Quantity = newComponent.Quantity,
                        Unit = newComponent.Unit
                    });

                Components.Add(new ComponentDisplayModel(newComponent, cost));
                NotifyCostChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding ingredient component: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditComponent(ComponentDisplayModel componentToEdit)
    {
        if (componentToEdit == null) return;

        try
        {
            Components.Remove(componentToEdit);

            if (componentToEdit.Component is EntreeRecipe entreeRecipe)
            {
                var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);

                var selectableItems = recipes.Select(r => new SelectionItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    Data = r
                }).ToList();

                var dialog = new EnhancedSelectionDialog();
                var viewModel = new EnhancedSelectionDialogViewModel(
                    selectableItems,
                    "Edit Recipe Component",
                    entreeRecipe.Unit
                );

                viewModel.SelectedItem = selectableItems.FirstOrDefault(si => si.Id == entreeRecipe.RecipeId);
                viewModel.Quantity = entreeRecipe.Quantity.ToString("F2");
                viewModel.SelectedUnit = entreeRecipe.Unit;

                dialog.DataContext = viewModel;
                var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_owner);

                if (result != null)
                {
                    var selectedRecipe = (Recipe)result.SelectedItem.Data;
                    var updatedComponent = new EntreeRecipe
                    {
                        Recipe = selectedRecipe,
                        RecipeId = selectedRecipe.Id,
                        Quantity = result.Quantity,
                        Unit = result.Unit
                    };

                    Components.Add(new ComponentDisplayModel(updatedComponent));
                    NotifyCostChanged();
                }
                else
                {
                    Components.Add(componentToEdit);
                }
            }
            else if (componentToEdit.Component is EntreeIngredient entreeIngredient)
            {
                var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);

                var selectableItems = ingredients.Select(i => new SelectionItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Data = i
                }).ToList();

                var dialog = new EnhancedSelectionDialog();
                var viewModel = new EnhancedSelectionDialogViewModel(
                    selectableItems,
                    "Edit Ingredient Component",
                    entreeIngredient.Unit
                );

                viewModel.SelectedItem = selectableItems.FirstOrDefault(si => si.Id == entreeIngredient.IngredientId);
                viewModel.Quantity = entreeIngredient.Quantity.ToString("F2");
                viewModel.SelectedUnit = entreeIngredient.Unit;

                dialog.DataContext = viewModel;
                var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_owner);

                if (result != null)
                {
                    var selectedIngredient = (Ingredient)result.SelectedItem.Data;
                    var updatedComponent = new EntreeIngredient
                    {
                        Ingredient = selectedIngredient,
                        IngredientId = selectedIngredient.Id,
                        Quantity = result.Quantity,
                        Unit = result.Unit
                    };

                    var cost = await _costCalculator.CalculateIngredientCostAsync(
                        new RecipeIngredient
                        {
                            Ingredient = updatedComponent.Ingredient,
                            IngredientId = updatedComponent.IngredientId,
                            Quantity = updatedComponent.Quantity,
                            Unit = updatedComponent.Unit
                        });

                    Components.Add(new ComponentDisplayModel(updatedComponent, cost));
                    NotifyCostChanged();
                }
                else
                {
                    Components.Add(componentToEdit);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error editing component: {ex.Message}");
            Components.Add(componentToEdit);
        }
    }

    [RelayCommand]
    private void RemoveComponent(ComponentDisplayModel component)
    {
        if (component != null)
        {
            Components.Remove(component);
            NotifyCostChanged();
        }
    }

    [RelayCommand]
    private async Task RelinkComponent(ComponentDisplayModel? component)
    {
        if (component == null)
            return;

        try
        {
            // Get all ingredients and recipes
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);

            var ingredientItems = ingredients.Select(i => new SelectionItem
            {
                Id = i.Id,
                Name = i.Name,
                Data = i
            }).ToList();

            var recipeItems = recipes.Select(r => new SelectionItem
            {
                Id = r.Id,
                Name = r.Name,
                Data = r
            }).ToList();

            // Determine if current component is ingredient or recipe
            bool isCurrentlyIngredient = component.Component is EntreeIngredient;
            decimal currentQuantity = 1;
            UnitType currentUnit = UnitType.Each;

            if (component.Component is EntreeIngredient ei)
            {
                currentQuantity = ei.Quantity;
                currentUnit = ei.Unit;
            }
            else if (component.Component is EntreeRecipe er)
            {
                currentQuantity = er.Quantity;
                currentUnit = er.Unit;
            }

            // Open relink dialog
            var dialog = new RelinkComponentDialog();
            var viewModel = new RelinkComponentDialogViewModel(
                dialog,
                component.Name,
                ingredientItems,
                recipeItems,
                isCurrentlyIngredient,
                currentQuantity,
                currentUnit
            );

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<RelinkComponentResult?>(_owner);

            if (result != null)
            {
                // Remove old component
                Components.Remove(component);

                // Add new component based on selection
                if (result.IsIngredient)
                {
                    var selectedIngredient = (Ingredient)result.SelectedItem.Data;
                    var newComponent = new EntreeIngredient
                    {
                        Ingredient = selectedIngredient,
                        IngredientId = selectedIngredient.Id,
                        Quantity = result.Quantity,
                        Unit = result.Unit,
                        DisplayName = component.Name // Preserve original display name
                    };

                    var cost = await _costCalculator.CalculateIngredientCostAsync(
                        new RecipeIngredient
                        {
                            Ingredient = newComponent.Ingredient,
                            IngredientId = newComponent.IngredientId,
                            Quantity = newComponent.Quantity,
                            Unit = newComponent.Unit
                        });

                    Components.Add(new ComponentDisplayModel(newComponent, cost));
                }
                else
                {
                    var selectedRecipe = (Recipe)result.SelectedItem.Data;
                    var newComponent = new EntreeRecipe
                    {
                        Recipe = selectedRecipe,
                        RecipeId = selectedRecipe.Id,
                        Quantity = result.Quantity,
                        Unit = result.Unit,
                        DisplayName = component.Name // Preserve original display name
                    };

                    Components.Add(new ComponentDisplayModel(newComponent));
                }

                NotifyCostChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error relinking component: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewMapping(ComponentDisplayModel? component)
    {
        if (component == null || !component.HasMapping || component.MappingId == null || _mappingService == null)
            return;

        try
        {
            var dialog = new EditMappingDialog();
            var viewModel = new EditMappingDialogViewModel(
                component.MappingId.Value,
                _mappingService,
                _ingredientService,
                _recipeService,
                _currentLocationService.CurrentLocationId,
                () => dialog.Close(),
                async () => await LoadMappingStatusAsync() // Reload mappings after changes
            );

            dialog.DataContext = viewModel;
            await dialog.ShowDialog(_owner);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening mapping dialog: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddPreparationStep()
    {
        int nextNumber = PreparationSteps.Count + 1;
        PreparationSteps.Add(new PreparationStepModel
        {
            StepNumber = $"{nextNumber}.",
            Description = string.Empty
        });
    }

    [RelayCommand]
    private void RemovePreparationStep(PreparationStepModel? step)
    {
        if (step == null) return;

        PreparationSteps.Remove(step);

        // Renumber all steps
        for (int i = 0; i < PreparationSteps.Count; i++)
        {
            PreparationSteps[i].StepNumber = $"{i + 1}.";
        }
    }

    private void NotifyCostChanged()
    {
        OnPropertyChanged(nameof(TotalCost));
        OnPropertyChanged(nameof(TotalCostDisplay));
        OnPropertyChanged(nameof(FoodCostDisplay));

        // Update nutrition displays
        OnPropertyChanged(nameof(CalculatedCalories));
        OnPropertyChanged(nameof(CalculatedProtein));
        OnPropertyChanged(nameof(CalculatedCarbohydrates));
        OnPropertyChanged(nameof(CalculatedFat));
        OnPropertyChanged(nameof(CalculatedFiber));
        OnPropertyChanged(nameof(CalculatedSugar));
        OnPropertyChanged(nameof(CalculatedSodium));

        // Raise ComponentsChanged event for allergen detection
        ComponentsChanged?.Invoke(this, EventArgs.Empty);
    }

    public Dictionary<AllergenType, List<string>> DetectAllergens(Entree entree)
    {
        return _allergenDetectionService.DetectAllergensFromEntree(entree);
    }

    [RelayCommand]
    private void Save()
    {
        // Prevent duplicate saves from rapid clicks
        if (IsSaving) return;

        try
        {
            IsSaving = true;

            // Build the entree object
            var entree = _existingEntree ?? new Entree
            {
                LocationId = _currentLocationService.CurrentLocationId
            };

            entree.Name = Name;
            entree.Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim();
            entree.MenuPrice = decimal.TryParse(MenuPrice, out var price) ? price : 0;
            entree.PlatingEquipment = string.IsNullOrWhiteSpace(PlatingEquipment) ? null : PlatingEquipment;
            
            // Convert preparation steps to string
            var nonEmptySteps = PreparationSteps.Where(s => !string.IsNullOrWhiteSpace(s.Description)).ToList();
            if (nonEmptySteps.Any())
            {
                entree.PreparationInstructions = string.Join("\n", nonEmptySteps.Select(s => $"{s.StepNumber} {s.Description}"));
            }
            else
            {
                entree.PreparationInstructions = null;
            }

            entree.PhotoUrl = PhotoUrl;

            entree.EntreeRecipes = Components
                .Select(c => c.Component)
                .OfType<EntreeRecipe>()
                .ToList();

            entree.EntreeIngredients = Components
                .Select(c => c.Component)
                .OfType<EntreeIngredient>()
                .ToList();

            // Validate the entree
            ValidationResult = _validationService.ValidateEntree(entree);
            if (!ValidationResult.IsValid)
            {
                return;
            }

            _onSaveSuccess();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }

    public Entree GetEntree()
    {
        // Build the entree object from current form values
        var entree = _existingEntree ?? new Entree
        {
            LocationId = _currentLocationService.CurrentLocationId
        };

        entree.Name = Name;
        entree.Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim();
        entree.MenuPrice = decimal.TryParse(MenuPrice, out var price) ? price : 0;
        entree.PlatingEquipment = string.IsNullOrWhiteSpace(PlatingEquipment) ? null : PlatingEquipment;
        
        // Convert preparation steps to string
        var nonEmptySteps = PreparationSteps.Where(s => !string.IsNullOrWhiteSpace(s.Description)).ToList();
        if (nonEmptySteps.Any())
        {
            entree.PreparationInstructions = string.Join("\n", nonEmptySteps.Select(s => $"{s.StepNumber} {s.Description}"));
        }
        else
        {
            entree.PreparationInstructions = null;
        }

        entree.PhotoUrl = PhotoUrl;

        entree.EntreeRecipes = Components
            .Select(c => c.Component)
            .OfType<EntreeRecipe>()
            .ToList();

        entree.EntreeIngredients = Components
            .Select(c => c.Component)
            .OfType<EntreeIngredient>()
            .ToList();

        return entree;
    }
}