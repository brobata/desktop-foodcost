using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Desktop.Models;
using Dfc.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Dfc.Desktop.ViewModels;

public partial class RecipeStepModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isEditing = false;

    public string StepNumber { get; set; } = string.Empty;
}

public partial class AddEditRecipeViewModel : ViewModelBase
{
    private Recipe? _existingRecipe;
    private readonly Action _onSaveSuccess;
    private readonly Action _onCancel;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IAllergenDetectionService _allergenDetectionService;
    private readonly IRecipeCardService _recipeCardService;
    private readonly IPhotoService _photoService;
    private readonly IValidationService _validationService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IIngredientMatchMappingService? _mappingService;
    private readonly Window? _ownerWindow;
    private Guid _currentLocationId => _currentLocationService.CurrentLocationId;

    // Event fired when components (ingredients/recipes) are added/removed
    public event EventHandler? IngredientsChanged;

    private readonly IRecipeCostCalculator _costCalculator;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _yieldAmount = string.Empty;

    [ObservableProperty]
    private string _yieldUnit = "servings"; // Default to servings (more common for recipes)

    [ObservableProperty]
    private string _prepTimeMinutes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RecipeStepModel> _instructionSteps = new();

    [ObservableProperty]
    private ObservableCollection<RecipeComponentDisplayModel> _components;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    [ObservableProperty]
    private string _scaleMultiplier = "1.0";

    [ObservableProperty]
    private string? _photoUrl;

    // v1.2.0 properties
    [ObservableProperty]
    private DifficultyLevel _difficulty = DifficultyLevel.NotSet;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _tags = string.Empty;

    // v1.3.0 properties
    [ObservableProperty]
    private string _dietaryLabels = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    public IPhotoService PhotoService => _photoService;

    public List<string> AvailableUnits { get; } = new()
    {
        "oz", "lb", "cup", "pint", "quart", "gallon", "ml", "liter",
        "serving", "servings", "portion", "portions", "piece", "pieces"
    };

    public List<DifficultyLevel> DifficultyLevels { get; } = new()
    {
        DifficultyLevel.NotSet,
        DifficultyLevel.Easy,
        DifficultyLevel.Medium,
        DifficultyLevel.Hard,
        DifficultyLevel.Expert
    };

    public string DialogTitle => _existingRecipe == null ? "Add Recipe" : "Edit Recipe";
    public bool IsEditMode => _existingRecipe != null;
    public Recipe? ExistingRecipe => _existingRecipe;

    public decimal TotalCost => Components.Sum(c => c.Cost);
    public string TotalCostDisplay => $"${TotalCost:F2}";

    public string FoodCostDisplay
    {
        get
        {
            // Recipes don't have menu price, but we show total cost
            return $"Cost: {TotalCost:C2}";
        }
    }

    // Cost alert thresholds
    public bool ShowLowCostAlert => TotalCost > 0 && TotalCost < 5;
    public bool ShowMediumCostAlert => TotalCost >= 5 && TotalCost < 20;
    public bool ShowHighCostAlert => TotalCost >= 20 && TotalCost < 50;
    public bool ShowVeryHighCostAlert => TotalCost >= 50;

    public string CostAlertMessage => TotalCost switch
    {
        < 5 when TotalCost > 0 => "💚 Low cost recipe - great for budget-friendly meals!",
        >= 5 and < 20 => "💛 Medium cost recipe - good value for quality ingredients",
        >= 20 and < 50 => "🧡 High cost recipe - premium ingredients or large yield",
        >= 50 => "❤️ Very high cost recipe - review portions or ingredient prices",
        _ => ""
    };

    public string CostAlertColor => TotalCost switch
    {
        < 5 when TotalCost > 0 => "#4CAF50",
        >= 5 and < 20 => "#FF9800",
        >= 20 and < 50 => "#FF5722",
        >= 50 => "#F44336",
        _ => "#9E9E9E"
    };

    public string CostAlertBackground => TotalCost switch
    {
        < 5 when TotalCost > 0 => "#E8F5E9",
        >= 5 and < 20 => "#FFF3E0",
        >= 20 and < 50 => "#FBE9E7",
        >= 50 => "#FFEBEE",
        _ => "#F5F5F5"
    };

    // Calculated nutrition properties (aggregated from ingredients)
    public string CalculatedCalories
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Calories.ToString("F1");
        }
    }

    public string CalculatedProtein
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Protein.ToString("F1");
        }
    }

    public string CalculatedCarbohydrates
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Carbohydrates.ToString("F1");
        }
    }

    public string CalculatedFat
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Fat.ToString("F1");
        }
    }

    public string CalculatedFiber
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Fiber.ToString("F1");
        }
    }

    public string CalculatedSugar
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Sugar.ToString("F1");
        }
    }

    public string CalculatedSodium
    {
        get
        {
            var recipe = GetRecipe();
            if (recipe?.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                return "0";
            return recipe.CalculatedNutrition.Sodium.ToString("F1");
        }
    }

    public AddEditRecipeViewModel(
        Action onSaveSuccess,
        Action onCancel,
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IAllergenDetectionService allergenDetectionService,
        IRecipeCardService recipeCardService,
        IPhotoService photoService,
        IValidationService validationService,
        ICurrentLocationService currentLocationService,
        IRecipeCostCalculator costCalculator,
        Window? ownerWindow = null,
        Recipe? existingRecipe = null,
        IIngredientMatchMappingService? mappingService = null)
    {
        _onSaveSuccess = onSaveSuccess;
        _onCancel = onCancel;
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _allergenDetectionService = allergenDetectionService;
        _validationService = validationService;
        _recipeCardService = recipeCardService;
        _photoService = photoService;
        _currentLocationService = currentLocationService;
        _costCalculator = costCalculator;
        _mappingService = mappingService;
        _ownerWindow = ownerWindow;
        _existingRecipe = existingRecipe;
        _components = new ObservableCollection<RecipeComponentDisplayModel>();

        if (existingRecipe != null)
        {
            // Load existing recipe data
            Name = existingRecipe.Name;
            Description = existingRecipe.Description ?? string.Empty;
            Category = existingRecipe.Category ?? string.Empty;
            YieldAmount = existingRecipe.Yield.ToString();
            YieldUnit = existingRecipe.YieldUnit;
            PrepTimeMinutes = existingRecipe.PrepTimeMinutes?.ToString() ?? string.Empty;
            LoadInstructionSteps(existingRecipe.Instructions);
            PhotoUrl = existingRecipe.PhotoUrl;

            // v1.2.0 fields
            Difficulty = existingRecipe.Difficulty;
            Notes = existingRecipe.Notes ?? string.Empty;
            Tags = existingRecipe.Tags ?? string.Empty;

            // v1.3.0 fields
            DietaryLabels = existingRecipe.DietaryLabels ?? string.Empty;

            // Load existing components (ingredients and sub-recipes)
            _ = LoadComponentsAsync();
        }
        else
        {
            // New recipe - initialize with 2 empty steps
            InitializeInstructionSteps(2);
        }
    }

    private async Task LoadComponentsAsync()
    {
        try
        {
            if (_existingRecipe == null) return;

            // Load recipe components (sub-recipes)
            foreach (var rr in _existingRecipe.RecipeRecipes)
            {
                Components.Add(new RecipeComponentDisplayModel(rr));
            }

            // Load ingredient components
            foreach (var ri in _existingRecipe.RecipeIngredients)
            {
                var cost = await _costCalculator.CalculateIngredientCostAsync(ri);
                Components.Add(new RecipeComponentDisplayModel(ri, cost));
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
            foreach (var component in Components)
            {
                // Check mapping based on the component's display name
                var mapping = await _mappingService.GetMappingForNameAsync(
                    component.Name,
                    _currentLocationId);

                if (mapping != null)
                {
                    component.HasMapping = true;
                    component.MappingId = mapping.Id;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading mapping status: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewMapping(RecipeComponentDisplayModel? component)
    {
        if (component == null || !component.HasMapping || component.MappingId == null || _ownerWindow == null || _mappingService == null)
            return;

        try
        {
            var dialog = new EditMappingDialog();
            var viewModel = new EditMappingDialogViewModel(
                component.MappingId.Value,
                _mappingService,
                _ingredientService,
                _recipeService,
                _currentLocationId,
                () => dialog.Close(),
                async () => await LoadMappingStatusAsync() // Reload mappings after changes
            );

            dialog.DataContext = viewModel;
            await dialog.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening mapping dialog: {ex.Message}");
        }
    }

    private void InitializeInstructionSteps(int count)
    {
        InstructionSteps.Clear();
        for (int i = 1; i <= count; i++)
        {
            InstructionSteps.Add(new RecipeStepModel
            {
                StepNumber = $"{i}.",
                Description = string.Empty
            });
        }
    }

    private void LoadInstructionSteps(string? instructions)
    {
        InstructionSteps.Clear();

        if (string.IsNullOrWhiteSpace(instructions))
        {
            // No existing steps - initialize with 2 empty steps
            InitializeInstructionSteps(2);
            return;
        }

        var lines = instructions.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int stepNum = 1;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                // Remove existing numbering if present
                var description = System.Text.RegularExpressions.Regex.Replace(trimmed, @"^\d+\.\s*", "");

                InstructionSteps.Add(new RecipeStepModel
                {
                    StepNumber = $"{stepNum}.",
                    Description = description
                });
                stepNum++;
            }
        }
    }

    [RelayCommand]
    private void AddInstructionStep()
    {
        int nextNumber = InstructionSteps.Count + 1;
        InstructionSteps.Add(new RecipeStepModel
        {
            StepNumber = $"{nextNumber}.",
            Description = string.Empty
        });
    }

    [RelayCommand]
    private void RemoveInstructionStep(RecipeStepModel? step)
    {
        if (step == null) return;

        InstructionSteps.Remove(step);

        // Renumber all steps
        for (int i = 0; i < InstructionSteps.Count; i++)
        {
            InstructionSteps[i].StepNumber = $"{i + 1}.";
        }
    }

    [RelayCommand]
    private async Task AddRecipe()
    {
        if (_ownerWindow == null) return;

        try
        {
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationId);

            var selectableItems = recipes.Select(r => new SelectionItem
            {
                Id = r.Id,
                Name = r.Name,
                Data = r
            }).ToList();

            var dialog = new EnhancedSelectionDialog();
            var viewModel = new EnhancedSelectionDialogViewModel(
                selectableItems,
                "Add Recipe Component",
                UnitType.Each
            );

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_ownerWindow);

            if (result != null)
            {
                var selectedRecipe = (Recipe)result.SelectedItem.Data;
                var newRecipeRecipe = new RecipeRecipe
                {
                    Id = Guid.NewGuid(),
                    ComponentRecipeId = selectedRecipe.Id,
                    Quantity = result.Quantity,
                    Unit = result.Unit,
                    SortOrder = Components.Count,
                    ComponentRecipe = selectedRecipe
                };

                Components.Add(new RecipeComponentDisplayModel(newRecipeRecipe));
                NotifyCostChanged();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error adding recipe: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in AddRecipe: {ex}");
        }
    }

    [RelayCommand]
    private async Task AddIngredient()
    {
        if (_ownerWindow == null) return;

        try
        {
            var allIngredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationId);

            var selectableItems = allIngredients.Select(i => new SelectionItem
            {
                Id = i.Id,
                Name = i.Name,
                Data = i
            }).ToList();

            var dialog = new EnhancedSelectionDialog();
            var viewModel = new EnhancedSelectionDialogViewModel(
                selectableItems,
                "Add Ingredient to Recipe",
                UnitType.Ounce
            );

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_ownerWindow);

            if (result != null)
            {
                var selectedIngredient = (Ingredient)result.SelectedItem.Data;

                var newRecipeIngredient = new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    IngredientId = selectedIngredient.Id,
                    Quantity = result.Quantity,
                    Unit = result.Unit,
                    IsOptional = false,
                    SortOrder = Components.Count,
                    Ingredient = selectedIngredient
                };

                var cost = await _costCalculator.CalculateIngredientCostAsync(newRecipeIngredient);
                Components.Add(new RecipeComponentDisplayModel(newRecipeIngredient, cost));
                NotifyCostChanged();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error adding ingredient: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in AddIngredient: {ex}");
        }
    }

    [RelayCommand]
    private void RemoveComponent(RecipeComponentDisplayModel? component)
    {
        if (component != null)
        {
            Components.Remove(component);
            NotifyCostChanged();
        }
    }

    private void NotifyCostChanged()
    {
        OnPropertyChanged(nameof(TotalCost));
        OnPropertyChanged(nameof(TotalCostDisplay));
        OnPropertyChanged(nameof(FoodCostDisplay));
        OnPropertyChanged(nameof(ShowLowCostAlert));
        OnPropertyChanged(nameof(ShowMediumCostAlert));
        OnPropertyChanged(nameof(ShowHighCostAlert));
        OnPropertyChanged(nameof(ShowVeryHighCostAlert));
        OnPropertyChanged(nameof(CostAlertMessage));
        OnPropertyChanged(nameof(CostAlertColor));
        OnPropertyChanged(nameof(CostAlertBackground));
        OnPropertyChanged(nameof(CalculatedCalories));
        OnPropertyChanged(nameof(CalculatedProtein));
        OnPropertyChanged(nameof(CalculatedCarbohydrates));
        OnPropertyChanged(nameof(CalculatedFat));
        OnPropertyChanged(nameof(CalculatedFiber));
        OnPropertyChanged(nameof(CalculatedSugar));
        OnPropertyChanged(nameof(CalculatedSodium));

        // Raise event for allergen detection
        IngredientsChanged?.Invoke(this, EventArgs.Empty);
    }

    // NOTE: EditIngredient removed - components are now edited via RemoveComponent + AddRecipe/AddIngredient
    /*[RelayCommand]
    private async Task EditIngredient(RecipeIngredientDisplayModel? ingredientToEdit)
    {
        if (ingredientToEdit == null || _ownerWindow == null) return;

        try
        {
            // Remove from list temporarily
            RecipeIngredients.Remove(ingredientToEdit);

            // Get all ingredients
            var allIngredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationId);

            var selectableItems = allIngredients.Select(i => new SelectionItem
            {
                Id = i.Id,
                Name = i.Name,
                Data = i
            }).ToList();

            var dialog = new EnhancedSelectionDialog();
            var viewModel = new EnhancedSelectionDialogViewModel(
                selectableItems,
                "Edit Recipe Ingredient",
                ingredientToEdit.Model.Unit  // Use existing unit as default
            );

            // Pre-select the existing ingredient
            viewModel.SelectedItem = selectableItems.FirstOrDefault(si => si.Id == ingredientToEdit.Model.IngredientId);
            viewModel.Quantity = ingredientToEdit.Model.Quantity.ToString("F2");
            viewModel.SelectedUnit = ingredientToEdit.Model.Unit;

            dialog.DataContext = viewModel;
            var result = await dialog.ShowDialog<EnhancedSelectionResult?>(_ownerWindow);

            if (result != null)
            {
                var selectedIngredient = (Ingredient)result.SelectedItem.Data;

                // Update the recipe ingredient
                var updatedRecipeIngredient = new RecipeIngredient
                {
                    Id = ingredientToEdit.Model.Id != Guid.Empty ? ingredientToEdit.Model.Id : Guid.NewGuid(),
                    IngredientId = selectedIngredient.Id,
                    Quantity = result.Quantity,
                    Unit = result.Unit,
                    IsOptional = ingredientToEdit.Model.IsOptional,
                    SortOrder = ingredientToEdit.Model.SortOrder,
                    // Set navigation property for display purposes only
                    Ingredient = selectedIngredient
                };

                RecipeIngredients.Add(new RecipeIngredientDisplayModel(updatedRecipeIngredient));

                OnPropertyChanged(nameof(TotalCost));
                OnPropertyChanged(nameof(TotalCostDisplay));
                OnPropertyChanged(nameof(ShowLowCostAlert));
                OnPropertyChanged(nameof(ShowMediumCostAlert));
                OnPropertyChanged(nameof(ShowHighCostAlert));
                OnPropertyChanged(nameof(ShowVeryHighCostAlert));
                OnPropertyChanged(nameof(CostAlertMessage));
                OnPropertyChanged(nameof(CostAlertColor));
                OnPropertyChanged(nameof(CostAlertBackground));
                OnPropertyChanged(nameof(CalculatedCalories));
                OnPropertyChanged(nameof(CalculatedProtein));
                OnPropertyChanged(nameof(CalculatedCarbohydrates));
                OnPropertyChanged(nameof(CalculatedFat));
                OnPropertyChanged(nameof(CalculatedFiber));
                OnPropertyChanged(nameof(CalculatedSugar));
                OnPropertyChanged(nameof(CalculatedSodium));
            }
            else
            {
                // User cancelled, add the original ingredient back
                RecipeIngredients.Add(ingredientToEdit);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error editing ingredient: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in EditIngredient: {ex}");
        }
    }*/

    // NOTE: RemoveIngredient removed - use RemoveComponent instead
    /*[RelayCommand]
    private void RemoveIngredient(RecipeIngredientDisplayModel? ingredient)
    {
        if (ingredient != null)
        {
            RecipeIngredients.Remove(ingredient);
            OnPropertyChanged(nameof(TotalCost));
            OnPropertyChanged(nameof(TotalCostDisplay));
            OnPropertyChanged(nameof(ShowLowCostAlert));
            OnPropertyChanged(nameof(ShowMediumCostAlert));
            OnPropertyChanged(nameof(ShowHighCostAlert));
            OnPropertyChanged(nameof(ShowVeryHighCostAlert));
            OnPropertyChanged(nameof(CostAlertMessage));
            OnPropertyChanged(nameof(CostAlertColor));
            OnPropertyChanged(nameof(CostAlertBackground));
            OnPropertyChanged(nameof(CalculatedCalories));
            OnPropertyChanged(nameof(CalculatedProtein));
            OnPropertyChanged(nameof(CalculatedCarbohydrates));
            OnPropertyChanged(nameof(CalculatedFat));
            OnPropertyChanged(nameof(CalculatedFiber));
            OnPropertyChanged(nameof(CalculatedSugar));
            OnPropertyChanged(nameof(CalculatedSodium));

            // Raise event for allergen detection
            IngredientsChanged?.Invoke(this, EventArgs.Empty);
        }
    }*/

    [RelayCommand]
    private void ScaleRecipeByPreset(string multiplierStr)
    {
        if (decimal.TryParse(multiplierStr, out var multiplier))
        {
            ScaleMultiplier = multiplierStr;
            ScaleRecipe();
        }
    }

    [RelayCommand]
    private void ScaleRecipe()
    {
        if (!decimal.TryParse(ScaleMultiplier, out var multiplier) || multiplier <= 0)
        {
            ErrorMessage = "Please enter a valid scale multiplier (e.g., 2 for double, 0.5 for half)";
            return;
        }

        ErrorMessage = string.Empty;

        // Scale all component quantities
        foreach (var component in Components)
        {
            if (component.Component is RecipeIngredient ri)
            {
                ri.Quantity *= multiplier;
            }
            else if (component.Component is RecipeRecipe rr)
            {
                rr.Quantity *= multiplier;
            }
        }

        // Force UI refresh by notifying cost changed
        NotifyCostChanged();

        // Scale the yield
        if (decimal.TryParse(YieldAmount, out var currentYield))
        {
            YieldAmount = (currentYield * multiplier).ToString("F2");
        }

        // Reset multiplier to 1.0
        ScaleMultiplier = "1.0";
    }

    [RelayCommand]
    private void Save()
    {
        // Prevent duplicate saves from rapid clicks
        if (IsSaving) return;

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;
            ValidationResult = null;

            // Parse numeric values
            if (!decimal.TryParse(YieldAmount, out var yieldValue))
            {
                yieldValue = 0;
            }

            int? prepTime = null;
            if (!string.IsNullOrWhiteSpace(PrepTimeMinutes))
            {
                if (int.TryParse(PrepTimeMinutes, out var parsedPrepTime))
                {
                    prepTime = parsedPrepTime;
                }
            }

            // Build temp recipe for validation
            if (_existingRecipe != null)
            {
                // Update existing recipe
                _existingRecipe.Name = Name.Trim();
                _existingRecipe.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
                _existingRecipe.Yield = yieldValue;
                _existingRecipe.YieldUnit = YieldUnit;
                _existingRecipe.PrepTimeMinutes = prepTime;

                // Convert instruction steps to string
                var nonEmptySteps = InstructionSteps.Where(s => !string.IsNullOrWhiteSpace(s.Description)).ToList();
                if (nonEmptySteps.Any())
                {
                    _existingRecipe.Instructions = string.Join("\n", nonEmptySteps.Select(s => $"{s.StepNumber} {s.Description}"));
                }
                else
                {
                    _existingRecipe.Instructions = null;
                }

                _existingRecipe.ModifiedAt = DateTime.UtcNow;

                // v1.2.0 fields
                _existingRecipe.Difficulty = Difficulty;
                _existingRecipe.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
                _existingRecipe.Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags.Trim();

                // v1.3.0 fields
                _existingRecipe.DietaryLabels = string.IsNullOrWhiteSpace(DietaryLabels) ? null : DietaryLabels.Trim();

                // Update components (ingredients and sub-recipes)
                _existingRecipe.RecipeIngredients.Clear();
                _existingRecipe.RecipeRecipes.Clear();

                foreach (var component in Components)
                {
                    if (component.Component is RecipeIngredient ri)
                    {
                        _existingRecipe.RecipeIngredients.Add(ri);
                    }
                    else if (component.Component is RecipeRecipe rr)
                    {
                        _existingRecipe.RecipeRecipes.Add(rr);
                    }
                }
            }
            else
            {
                // Create new recipe
                // Convert instruction steps to string
                var nonEmptySteps = InstructionSteps.Where(s => !string.IsNullOrWhiteSpace(s.Description)).ToList();
                string? instructions = null;
                if (nonEmptySteps.Any())
                {
                    instructions = string.Join("\n", nonEmptySteps.Select(s => $"{s.StepNumber} {s.Description}"));
                }

                _existingRecipe = new Recipe
                {
                    Id = Guid.NewGuid(),
                    Name = Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Yield = yieldValue,
                    YieldUnit = YieldUnit,
                    PrepTimeMinutes = prepTime,
                    Instructions = instructions,
                    LocationId = _currentLocationId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    RecipeIngredients = Components.Where(c => c.Component is RecipeIngredient).Select(c => (RecipeIngredient)c.Component).ToList(),
                    RecipeRecipes = Components.Where(c => c.Component is RecipeRecipe).Select(c => (RecipeRecipe)c.Component).ToList(),

                    // v1.2.0 fields
                    Difficulty = Difficulty,
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                    Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags.Trim(),

                    // v1.3.0 fields
                    DietaryLabels = string.IsNullOrWhiteSpace(DietaryLabels) ? null : DietaryLabels.Trim()
                };
            }

            // Validate the recipe
            ValidationResult = _validationService.ValidateRecipe(_existingRecipe);

            // If there are errors, don't save
            if (!ValidationResult.IsValid)
            {
                return;
            }

            _onSaveSuccess();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving recipe: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in Save: {ex}");
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

    [RelayCommand]
    private async Task SelectPhoto()
    {
        if (_ownerWindow == null) return;

        try
        {
            var file = await _ownerWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Select Recipe Photo",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png" }
                    }
                }
            });

            if (file != null && file.Count > 0)
            {
                var sourceFilePath = file[0].Path.LocalPath;
                PhotoUrl = await _photoService.SaveRecipePhotoAsync(sourceFilePath);
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error selecting photo: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        PhotoUrl = null;
    }

    [RelayCommand]
    private async Task GeneratePdf()
    {
        if (_existingRecipe == null)
        {
            ErrorMessage = "Please save the recipe before generating a PDF";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;

            // Generate PDF
            var pdfPath = await _recipeCardService.GenerateRecipeCardPdfAsync(_existingRecipe);

            // Open the PDF folder
            var folder = System.IO.Path.GetDirectoryName(pdfPath);
            if (!string.IsNullOrEmpty(folder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }

            System.Diagnostics.Debug.WriteLine($"PDF generated: {pdfPath}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error generating PDF: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in GeneratePdf: {ex}");
        }
    }

    public Recipe? GetRecipe()
    {
        // Build a temp recipe for allergen detection (even if not saved yet)
        var recipe = _existingRecipe ?? new Recipe
        {
            Id = Guid.NewGuid(),
            LocationId = _currentLocationId,
            Name = Name,
            Description = Description
        };

        // Update with current view model data
        recipe.Name = Name;
        recipe.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        recipe.Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim();
        recipe.YieldUnit = YieldUnit;
        recipe.PhotoUrl = PhotoUrl;

        if (decimal.TryParse(YieldAmount, out var yieldValue))
        {
            recipe.Yield = yieldValue;
        }

        // Include current components for allergen detection and cost calculation
        recipe.RecipeIngredients = Components.Where(c => c.Component is RecipeIngredient).Select(c => (RecipeIngredient)c.Component).ToList();
        recipe.RecipeRecipes = Components.Where(c => c.Component is RecipeRecipe).Select(c => (RecipeRecipe)c.Component).ToList();

        return recipe;
    }

    public Dictionary<AllergenType, List<string>> DetectAllergens(Recipe recipe)
    {
        return _allergenDetectionService.DetectAllergensFromRecipe(recipe);
    }
}