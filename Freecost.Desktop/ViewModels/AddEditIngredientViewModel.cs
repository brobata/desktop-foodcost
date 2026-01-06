// Location: Freecost.Desktop/ViewModels/AddEditIngredientViewModel.cs
// Action: UPDATE - Add category dropdown support

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Enums;
using Freecost.Core.Helpers;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class AddEditIngredientViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IValidationService _validationService;
    private readonly ICategoryColorService _categoryColorService;
    private readonly INutritionalDataService? _nutritionalDataService;
    private readonly IIngredientConversionRepository? _conversionRepository;
    private readonly Guid _locationId;
    private readonly Action _onSaveSuccess;
    private readonly Action _onCancel;
    private Ingredient? _existingIngredient;
    private Window? _ownerWindow;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _category;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private string _vendorSku = string.Empty;

    [ObservableProperty]
    private string _currentPrice = string.Empty;

    [ObservableProperty]
    private string _caseQuantity = "1";

    [ObservableProperty]
    private UnitType _selectedUnit;

    [ObservableProperty]
    private bool _useAlternateUnit;

    [ObservableProperty]
    private UnitType? _alternateUnit;

    [ObservableProperty]
    private string _alternateConversionQuantity = "1.0";

    [ObservableProperty]
    private UnitType? _alternateConversionUnit;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    [ObservableProperty]
    private string _newAliasName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<IngredientAlias> _aliases = new();

    private List<Freecost.Desktop.Controls.AllergenItemViewModel> _selectedAllergens = new();

    // Nutrition fields (per unit)
    [ObservableProperty]
    private string _caloriesPerUnit = string.Empty;

    [ObservableProperty]
    private string _proteinPerUnit = string.Empty;

    [ObservableProperty]
    private string _carbohydratesPerUnit = string.Empty;

    [ObservableProperty]
    private string _fatPerUnit = string.Empty;

    [ObservableProperty]
    private string _fiberPerUnit = string.Empty;

    [ObservableProperty]
    private string _sugarPerUnit = string.Empty;

    [ObservableProperty]
    private string _sodiumPerUnit = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private ObservableCollection<IngredientConversion> _usdaConversions = new();

    [ObservableProperty]
    private string _conversionSource = string.Empty;

    [ObservableProperty]
    private DateTime? _conversionLastUpdated;

    [ObservableProperty]
    private bool _hasConversions;

    [ObservableProperty]
    private string _conversionStatusText = string.Empty;

    // Predefined category list
    public List<string> AvailableCategories { get; } = new List<string>
    {
        "Meat & Poultry",
        "Seafood & Fish",
        "Dairy & Eggs",
        "Fresh Produce",
        "Dry Goods",
        "Dry Spices & Herbs",
        "Paper & Disposables",
        "Frozen Items",
        "Bakery Items",
        "Non-Alcoholic Beverage",
        "Beer",
        "Liquor",
        "Wine"
    };

    public string WindowTitle => _existingIngredient == null ? "Add New Ingredient" : "Edit Ingredient";
    public Ingredient? ExistingIngredient => _existingIngredient;
    public List<UnitType> AvailableUnits => AllUnits;
    public List<UnitType> AllUnits { get; } = Enum.GetValues<UnitType>().ToList();

    public List<UnitType> AlternateUnitOptions { get; private set; } = new();

    public List<UnitType> AlternateConversionUnitOptions { get; private set; } = new();

    public string AlternateUnitAbbreviation =>
        AlternateUnit.HasValue ? UnitConverter.GetAbbreviation(AlternateUnit.Value) : "";

    public bool ShowConversionPreview =>
        UseAlternateUnit &&
        AlternateUnit.HasValue &&
        !string.IsNullOrWhiteSpace(AlternateConversionQuantity) &&
        decimal.TryParse(AlternateConversionQuantity, out _) &&
        AlternateConversionUnit.HasValue;

    public string ConversionPreviewText
    {
        get
        {
            if (!ShowConversionPreview || !AlternateUnit.HasValue || !AlternateConversionUnit.HasValue)
                return string.Empty;

            if (decimal.TryParse(AlternateConversionQuantity, out var qty))
            {
                return $"1 {AlternateUnit.Value} = {qty:F2} {UnitConverter.GetAbbreviation(AlternateConversionUnit.Value)}";
            }
            return string.Empty;
        }
    }

    public string ConversionExampleText =>
        AlternateUnit.HasValue
        ? $"Example: If 1 {UnitConverter.GetAbbreviation(AlternateUnit.Value)} of {Name} weighs a certain amount, enter that weight here"
        : "Example: If 1 jalapeño weighs 1 ounce, enter 1.0 and select Ounce";

    public bool ShowConversionSection => UseAlternateUnit;

    public string ConversionStatusBadgeColor =>
        HasConversions ? "#4CAF50" : "#9E9E9E"; // Green if has conversions, gray otherwise

    public string ConversionStatusIcon =>
        HasConversions ? "✓" : "○"; // Checkmark if has conversions, circle otherwise

    public AddEditIngredientViewModel(
        IIngredientService ingredientService,
        IValidationService validationService,
        ICategoryColorService categoryColorService,
        Guid locationId,
        Action onSaveSuccess,
        Action onCancel,
        Ingredient? existingIngredient = null,
        INutritionalDataService? nutritionalDataService = null,
        IIngredientConversionRepository? conversionRepository = null,
        Window? ownerWindow = null)
    {
        _ingredientService = ingredientService;
        _validationService = validationService;
        _categoryColorService = categoryColorService;
        _nutritionalDataService = nutritionalDataService;
        _conversionRepository = conversionRepository;
        _locationId = locationId;
        _onSaveSuccess = onSaveSuccess;
        _onCancel = onCancel;
        _existingIngredient = existingIngredient;
        _ownerWindow = ownerWindow;

        if (existingIngredient != null)
        {
            LoadExistingIngredient(existingIngredient);
        }

        UpdateAlternateUnitOptions();
    }

    private void LoadExistingIngredient(Ingredient ingredient)
    {
        Name = ingredient.Name;
        Category = ingredient.Category; // This will now bind to ComboBox
        VendorName = ingredient.VendorName ?? string.Empty;
        VendorSku = ingredient.VendorSku ?? string.Empty;
        CurrentPrice = ingredient.CurrentPrice.ToString("F2");
        CaseQuantity = ingredient.CaseQuantity.ToString("F2");
        SelectedUnit = ingredient.Unit;

        UseAlternateUnit = ingredient.UseAlternateUnit;
        AlternateUnit = ingredient.AlternateUnit;
        AlternateConversionQuantity = ingredient.AlternateConversionQuantity?.ToString("F2") ?? "1.0";
        AlternateConversionUnit = ingredient.AlternateConversionUnit;

        // Load nutrition fields
        CaloriesPerUnit = ingredient.CaloriesPerUnit?.ToString("F2") ?? string.Empty;
        ProteinPerUnit = ingredient.ProteinPerUnit?.ToString("F2") ?? string.Empty;
        CarbohydratesPerUnit = ingredient.CarbohydratesPerUnit?.ToString("F2") ?? string.Empty;
        FatPerUnit = ingredient.FatPerUnit?.ToString("F2") ?? string.Empty;
        FiberPerUnit = ingredient.FiberPerUnit?.ToString("F2") ?? string.Empty;
        SugarPerUnit = ingredient.SugarPerUnit?.ToString("F2") ?? string.Empty;
        SodiumPerUnit = ingredient.SodiumPerUnit?.ToString("F2") ?? string.Empty;

        Aliases.Clear();
        if (ingredient.Aliases != null)
        {
            foreach (var alias in ingredient.Aliases)
            {
                Aliases.Add(alias);
            }
        }

        // Load conversion metadata
        ConversionSource = ingredient.ConversionSource ?? string.Empty;
        ConversionLastUpdated = ingredient.ConversionLastUpdated;
        UpdateConversionStatus();

        // Load conversions asynchronously
        _ = LoadConversionsAsync();
    }

    private async Task LoadConversionsAsync()
    {
        if (_existingIngredient == null || _conversionRepository == null)
            return;

        try
        {
            var conversions = await _conversionRepository.GetByIngredientIdAsync(_existingIngredient.Id);
            
            UsdaConversions.Clear();
            foreach (var conversion in conversions.OrderBy(c => c.FromUnit).ThenBy(c => c.ToUnit))
            {
                UsdaConversions.Add(conversion);
            }

            HasConversions = UsdaConversions.Count > 0;
            UpdateConversionStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AddEditIngredientVM] Error loading conversions: {ex.Message}");
            HasConversions = false;
        }
    }

    private void UpdateConversionStatus()
    {
        if (HasConversions)
        {
            var source = !string.IsNullOrEmpty(ConversionSource) ? ConversionSource : "Unknown";
            var lastUpdated = ConversionLastUpdated.HasValue 
                ? ConversionLastUpdated.Value.ToLocalTime().ToString("MMM d, yyyy") 
                : "Never";
            ConversionStatusText = $"✓ {UsdaConversions.Count} conversion(s) from {source} (Updated: {lastUpdated})";
        }
        else
        {
            ConversionStatusText = "○ No automatic conversions found";
        }
    }

        [RelayCommand]
    private void AddAlias()
    {
        if (string.IsNullOrWhiteSpace(NewAliasName))
            return;

        var alias = new IngredientAlias
        {
            Id = Guid.NewGuid(),
            AliasName = NewAliasName.Trim(),
            IngredientId = _existingIngredient?.Id ?? Guid.Empty
        };

        Aliases.Add(alias);
        NewAliasName = string.Empty;
    }

    [RelayCommand]
    private void RemoveAlias(IngredientAlias? alias)
    {
        if (alias != null)
        {
            Aliases.Remove(alias);
        }
    }

    partial void OnSelectedUnitChanged(UnitType value)
    {
        UpdateAlternateUnitOptions();
    }

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(ConversionExampleText));
    }

    partial void OnUseAlternateUnitChanged(bool value)
    {
        if (value)
        {
            UpdateAlternateConversionUnitOptions();
        }

        OnPropertyChanged(nameof(ShowConversionPreview));
        OnPropertyChanged(nameof(ConversionPreviewText));
        OnPropertyChanged(nameof(ShowConversionSection));
    }

    partial void OnAlternateUnitChanged(UnitType? value)
    {
        UpdateAlternateConversionUnitOptions();
        OnPropertyChanged(nameof(AlternateUnitAbbreviation));
        OnPropertyChanged(nameof(ShowConversionPreview));
        OnPropertyChanged(nameof(ConversionPreviewText));
        OnPropertyChanged(nameof(ConversionExampleText));
        OnPropertyChanged(nameof(ShowConversionSection));
    }

    partial void OnAlternateConversionQuantityChanged(string value)
    {
        OnPropertyChanged(nameof(ShowConversionPreview));
        OnPropertyChanged(nameof(ConversionPreviewText));
    }

    partial void OnAlternateConversionUnitChanged(UnitType? value)
    {
        OnPropertyChanged(nameof(ShowConversionPreview));
        OnPropertyChanged(nameof(ConversionPreviewText));
    }

    partial void OnCategoryChanged(string? value)
    {
        // Assign color when category is set or changed
        if (!string.IsNullOrWhiteSpace(value))
        {
            // Get existing color from the ingredient (if editing), otherwise null
            var existingColor = _existingIngredient?.CategoryColor;
            var assignedColor = _categoryColorService.GetOrAssignColor(value, existingColor);

            // Update the ingredient's category color
            if (_existingIngredient != null)
            {
                _existingIngredient.CategoryColor = assignedColor;
            }
        }
    }

    private void UpdateAlternateUnitOptions()
    {
        var alternateUnits = UnitConverter.GetUnitsInSameCategory(SelectedUnit);
        AlternateUnitOptions = alternateUnits.Where(u => u != SelectedUnit).ToList();
        OnPropertyChanged(nameof(AlternateUnitOptions));
    }

    private void UpdateAlternateConversionUnitOptions()
    {
        if (AlternateUnit.HasValue)
        {
            AlternateConversionUnitOptions = UnitConverter.GetUnitsInSameCategory(SelectedUnit).ToList();
        }
        else
        {
            AlternateConversionUnitOptions = new List<UnitType>();
        }
        OnPropertyChanged(nameof(AlternateConversionUnitOptions));
    }

    [RelayCommand]
    private async Task Save()
    {
        // Prevent duplicate saves from rapid clicks
        if (IsSaving) return;

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;
            ValidationResult = null;

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("[SAVE START] AddEditIngredientViewModel.Save");
            System.Diagnostics.Debug.WriteLine($"[SAVE] Mode: {(_existingIngredient != null ? "UPDATE" : "CREATE")}");
            System.Diagnostics.Debug.WriteLine($"[SAVE] Name: {Name}");
            System.Diagnostics.Debug.WriteLine($"[SAVE] Location ID: {_locationId}");

            // Log database path for diagnostics
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Freecost",
                "freecost.db"
            );
            System.Diagnostics.Debug.WriteLine($"[SAVE] Database path: {dbPath}");
            System.Diagnostics.Debug.WriteLine($"[SAVE] Database exists: {System.IO.File.Exists(dbPath)}");

            // Check directory permissions
            var dbDirectory = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDirectory))
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE] Database directory: {dbDirectory}");
                System.Diagnostics.Debug.WriteLine($"[SAVE] Directory exists: {System.IO.Directory.Exists(dbDirectory)}");

                // Try to create a test file to verify write permissions
                try
                {
                    var testFile = System.IO.Path.Combine(dbDirectory, $"write_test_{Guid.NewGuid()}.tmp");
                    System.IO.File.WriteAllText(testFile, "test");
                    System.IO.File.Delete(testFile);
                    System.Diagnostics.Debug.WriteLine($"[SAVE] Write permissions: OK");
                }
                catch (Exception permEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] Write permissions: FAILED - {permEx.Message}");
                    ErrorMessage = $"Database directory is not writable: {permEx.Message}\n\nPath: {dbDirectory}\n\nPlease check folder permissions.";

                    // Log to file for user to review
                    await LogSaveErrorToFileAsync("Permission Error", permEx, dbPath);
                    return;
                }
            }

            // Update or create the ingredient
            if (_existingIngredient != null)
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE] Updating existing ingredient ID: {_existingIngredient.Id}");

                // CRITICAL: Validate BEFORE saving
                UpdateExistingIngredient(_existingIngredient);
                if (!ValidateIngredient(_existingIngredient))
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] Validation failed - aborting update");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[SAVE] Calling UpdateIngredientAsync...");
                await _ingredientService.UpdateIngredientAsync(_existingIngredient);
                System.Diagnostics.Debug.WriteLine($"[SAVE] Update successful");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE] Creating new ingredient");

                // CRITICAL: Validate BEFORE saving to database
                // Create a temporary ingredient for validation (don't save yet)
                var tempIngredient = new Ingredient
                {
                    Name = Name.Trim(),
                    Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim(),
                    CategoryColor = _categoryColorService.GetOrAssignColor(string.IsNullOrWhiteSpace(Category) ? null : Category.Trim()),
                    VendorName = string.IsNullOrWhiteSpace(VendorName) ? null : VendorName.Trim(),
                    VendorSku = string.IsNullOrWhiteSpace(VendorSku) ? null : VendorSku.Trim(),
                    CurrentPrice = decimal.TryParse(CurrentPrice, out var price) ? price : 0,
                    CaseQuantity = decimal.TryParse(CaseQuantity, out var caseQty) ? caseQty : 1,
                    Unit = SelectedUnit,
                    LocationId = _locationId,
                    Aliases = Aliases.ToList()
                };

                UpdateAlternateUnits(tempIngredient);
                UpdateNutritionFields(tempIngredient);

                // Add allergens if any were selected
                if (_selectedAllergens.Any())
                {
                    tempIngredient.IngredientAllergens = _selectedAllergens.Select(a => new IngredientAllergen
                    {
                        Id = Guid.NewGuid(),
                        AllergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                        IsAutoDetected = a.IsAutoDetected,
                        IsEnabled = a.IsSelected,
                        SourceIngredients = a.SourceIngredients
                    }).ToList();
                }

                System.Diagnostics.Debug.WriteLine($"[SAVE] Validating new ingredient...");
                if (!ValidateIngredient(tempIngredient))
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] Validation failed - aborting create");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[SAVE] Validation passed - calling CreateIngredientAsync...");
                _existingIngredient = await _ingredientService.CreateIngredientAsync(tempIngredient);
                System.Diagnostics.Debug.WriteLine($"[SAVE] Create successful - ID: {_existingIngredient.Id}");
            }

            System.Diagnostics.Debug.WriteLine($"[SAVE] Calling success callback...");

            // Reload conversions to show what was auto-extracted
            if (_existingIngredient != null)
            {
                await LoadConversionsAsync();
                System.Diagnostics.Debug.WriteLine($"[SAVE] Conversions reloaded - Count: {UsdaConversions.Count}");
            }

            _onSaveSuccess();
            System.Diagnostics.Debug.WriteLine($"[SAVE] Save operation completed successfully");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("║ [SAVE EXCEPTION]                                  ║");
            System.Diagnostics.Debug.WriteLine("╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("╚═══════════════════════════════════════════════════╝");

            ErrorMessage = $"Error saving ingredient: {ex.Message}";

            // Log detailed error to file for troubleshooting
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Freecost",
                "freecost.db"
            );
            await LogSaveErrorToFileAsync("Save Exception", ex, dbPath);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Log save errors to a file for user troubleshooting
    /// </summary>
    private async Task LogSaveErrorToFileAsync(string errorType, Exception ex, string dbPath)
    {
        try
        {
            var logDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "OneDrive",
                "Desktop",
                "Logs"
            );

            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            var logFile = System.IO.Path.Combine(logDirectory, $"ingredient_save_errors_{DateTime.Now:yyyyMMdd}.txt");

            var logEntry = $@"
═══════════════════════════════════════════════════════════════
{errorType} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}
═══════════════════════════════════════════════════════════════
Ingredient Name: {Name}
Location ID: {_locationId}
Database Path: {dbPath}
Database Exists: {System.IO.File.Exists(dbPath)}

Error Type: {ex.GetType().Name}
Error Message: {ex.Message}

Stack Trace:
{ex.StackTrace}
";

            if (ex.InnerException != null)
            {
                logEntry += $@"
Inner Exception: {ex.InnerException.GetType().Name}
Inner Message: {ex.InnerException.Message}
Inner Stack Trace:
{ex.InnerException.StackTrace}
";
            }

            logEntry += "\n═══════════════════════════════════════════════════════════════\n\n";

            await System.IO.File.AppendAllTextAsync(logFile, logEntry);

            // Update error message to include log file location
            ErrorMessage += $"\n\nDetails logged to:\n{logFile}";
        }
        catch (Exception logEx)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to write error log: {logEx.Message}");
        }
    }

    /// <summary>
    /// Update existing ingredient with form values
    /// </summary>
    private void UpdateExistingIngredient(Ingredient ingredient)
    {
        // Basic fields
        ingredient.Name = Name.Trim();
        ingredient.Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim();
        ingredient.CategoryColor = _categoryColorService.GetOrAssignColor(ingredient.Category, ingredient.CategoryColor);
        ingredient.VendorName = string.IsNullOrWhiteSpace(VendorName) ? null : VendorName.Trim();
        ingredient.VendorSku = string.IsNullOrWhiteSpace(VendorSku) ? null : VendorSku.Trim();

        // Pricing and units
        ingredient.CurrentPrice = decimal.TryParse(CurrentPrice, out var price) ? price : 0;
        ingredient.CaseQuantity = decimal.TryParse(CaseQuantity, out var caseQty) ? caseQty : 1;
        ingredient.Unit = SelectedUnit;

        // Alternate units
        UpdateAlternateUnits(ingredient);

        // Nutrition fields
        UpdateNutritionFields(ingredient);

        // Aliases
        UpdateAliases(ingredient);

        // Allergens
        UpdateAllergens(ingredient);
    }


    /// <summary>
    /// Update alternate unit fields on ingredient
    /// </summary>
    private void UpdateAlternateUnits(Ingredient ingredient)
    {
        ingredient.UseAlternateUnit = UseAlternateUnit;
        ingredient.AlternateUnit = UseAlternateUnit ? AlternateUnit : null;
        ingredient.AlternateConversionQuantity = UseAlternateUnit && decimal.TryParse(AlternateConversionQuantity, out var qty) ? qty : null;
        ingredient.AlternateConversionUnit = UseAlternateUnit ? AlternateConversionUnit : null;
    }

    /// <summary>
    /// Update nutrition fields on ingredient
    /// </summary>
    private void UpdateNutritionFields(Ingredient ingredient)
    {
        ingredient.CaloriesPerUnit = decimal.TryParse(CaloriesPerUnit, out var cal) ? cal : null;
        ingredient.ProteinPerUnit = decimal.TryParse(ProteinPerUnit, out var pro) ? pro : null;
        ingredient.CarbohydratesPerUnit = decimal.TryParse(CarbohydratesPerUnit, out var carb) ? carb : null;
        ingredient.FatPerUnit = decimal.TryParse(FatPerUnit, out var fat) ? fat : null;
        ingredient.FiberPerUnit = decimal.TryParse(FiberPerUnit, out var fib) ? fib : null;
        ingredient.SugarPerUnit = decimal.TryParse(SugarPerUnit, out var sug) ? sug : null;
        ingredient.SodiumPerUnit = decimal.TryParse(SodiumPerUnit, out var sod) ? sod : null;
    }

    /// <summary>
    /// Update aliases on ingredient
    /// </summary>
    private void UpdateAliases(Ingredient ingredient)
    {
        ingredient.Aliases ??= new List<IngredientAlias>();
        ingredient.Aliases.Clear();
        foreach (var alias in Aliases)
        {
            alias.IngredientId = ingredient.Id;
            ingredient.Aliases.Add(alias);
        }
    }

    /// <summary>
    /// Update allergens on ingredient
    /// </summary>
    private void UpdateAllergens(Ingredient ingredient)
    {
        ingredient.IngredientAllergens ??= new List<IngredientAllergen>();
        ingredient.IngredientAllergens.Clear();

        foreach (var allergen in _selectedAllergens)
        {
            ingredient.IngredientAllergens.Add(new IngredientAllergen
            {
                Id = Guid.NewGuid(),
                IngredientId = ingredient.Id,
                AllergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(allergen.AllergenType),
                IsAutoDetected = allergen.IsAutoDetected,
                IsEnabled = allergen.IsSelected,
                SourceIngredients = allergen.SourceIngredients
            });
        }
    }

    /// <summary>
    /// Validate ingredient and return whether it's valid
    /// </summary>
    private bool ValidateIngredient(Ingredient? ingredient)
    {
        if (ingredient == null)
            return false;

        ValidationResult = _validationService.ValidateIngredient(ingredient);
        return ValidationResult.IsValid;
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }

    [RelayCommand]
    private async Task MapNutritionalData()
    {
        if (_nutritionalDataService == null)
        {
            ErrorMessage = "USDA API key not configured. Please set the USDA_API_KEY environment variable.";
            return;
        }

        if (_ownerWindow == null)
        {
            ErrorMessage = "Window not initialized";
            return;
        }

        try
        {
            // First, save the ingredient to ensure we have an ID
            if (_existingIngredient == null)
            {
                ErrorMessage = "Please save the ingredient first before mapping nutritional data";
                return;
            }

            // Prepare ingredient for mapping
            var ingredientCollection = new ObservableCollection<Ingredient> { _existingIngredient };

            var mapperWindow = new Views.NutritionalDataMapperWindow();

            // Create the mapper viewmodel with callback
            var mapperViewModel = new NutritionalDataMapperViewModel(
                _nutritionalDataService,
                async (success) =>
                {
                    mapperWindow.Close();
                    if (success && _existingIngredient != null)
                    {
                        // Save the ingredient with updated nutritional data
                        await _ingredientService.UpdateIngredientAsync(_existingIngredient);

                        // Reload nutritional data into the form
                        LoadExistingIngredient(_existingIngredient);
                    }
                });

            mapperWindow.DataContext = mapperViewModel;
            await mapperViewModel.InitializeAsync(ingredientCollection);
            await mapperWindow.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error mapping nutritional data: {ex.Message}";
        }
    }

    public void SetAllergens(List<Freecost.Desktop.Controls.AllergenItemViewModel> allergens)
    {
        _selectedAllergens = allergens;
    }
}