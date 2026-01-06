using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Enums;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Core.Repositories;
using Freecost.Desktop.Models;
using Freecost.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class IngredientsViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IImportMapService _importMapService;
    private readonly IPriceHistoryService _priceHistoryService;
    private readonly IExcelExportService _excelExportService;
    private readonly IValidationService _validationService;
    private readonly IRecycleBinService _recycleBinService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IStatusNotificationService _notificationService;
    private readonly ICategoryColorService _categoryColorService;
    private readonly IBatchOperationsService _batchOperationsService;
    private readonly INutritionalDataService? _nutritionalDataService;
    private readonly IIngredientConversionRepository? _conversionRepository;
    private Window? _ownerWindow;
    private readonly Func<Task>? _onItemDeleted;

    [ObservableProperty]
    private ObservableCollection<IngredientDisplayModel> _ingredients;

    [ObservableProperty]
    private IngredientDisplayModel? _selectedIngredient;

    [ObservableProperty]
    private ObservableCollection<IngredientDisplayModel> _selectedIngredients = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<AllergenFilterItem> _availableAllergens = new();

    [ObservableProperty]
    private int _activeFilterCount = 0;

    [ObservableProperty]
    private int _totalIngredients = 0;

    [ObservableProperty]
    private int _validIngredients = 0;

    [ObservableProperty]
    private decimal _averagePrice = 0;

    [ObservableProperty]
    private int _uniqueCategories = 0;

    [ObservableProperty]
    private int _validationWarningCount = 0;

    [ObservableProperty]
    private int _missingPrices = 0;

    [ObservableProperty]
    private int _missingVendors = 0;

    [ObservableProperty]
    private int _missingCategories = 0;

    [ObservableProperty]
    private int _missingNutritionalData = 0;

    [ObservableProperty]
    private string _selectedSortOption = "Name (A-Z)";

    public List<string> SortOptions { get; } = new List<string>
    {
        "Name (A-Z)",
        "Name (Z-A)",
        "Price (Low-High)",
        "Price (High-Low)",
        "Category (A-Z)",
        "Vendor (A-Z)"
    };

    [ObservableProperty]
    private string _selectedQuickFilter = "All Items";

    public List<string> QuickFilterOptions { get; } = new List<string>
    {
        "All Items",
        "High Cost (>$10)",
        "Low Cost (<$5)",
        "Missing Prices",
        "No Vendor",
        "No Category"
    };

    private readonly HashSet<AllergenType> _selectedAllergens = new();

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadIngredientsAsync();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        _ = LoadIngredientsAsync();
    }

    partial void OnSelectedQuickFilterChanged(string value)
    {
        _ = LoadIngredientsAsync();
    }

    public IngredientsViewModel(IIngredientService ingredientService, IImportMapService importMapService, IPriceHistoryService priceHistoryService, IExcelExportService excelExportService, IValidationService validationService, IRecycleBinService recycleBinService, ICurrentLocationService currentLocationService, IStatusNotificationService notificationService, ICategoryColorService categoryColorService, IBatchOperationsService batchOperationsService, Window window, Func<Task>? onItemDeleted = null, INutritionalDataService? nutritionalDataService = null, IIngredientConversionRepository? conversionRepository = null)
    {
        _ingredientService = ingredientService;
        _importMapService = importMapService;
        _priceHistoryService = priceHistoryService;
        _excelExportService = excelExportService;
        _validationService = validationService;
        _recycleBinService = recycleBinService;
        _currentLocationService = currentLocationService;
        _notificationService = notificationService;
        _categoryColorService = categoryColorService;
        _batchOperationsService = batchOperationsService;
        _nutritionalDataService = nutritionalDataService;
        _conversionRepository = conversionRepository;
        _ownerWindow = window;
        _onItemDeleted = onItemDeleted;
        _ingredients = new ObservableCollection<IngredientDisplayModel>();
        InitializeAllergenFilters();
        _ = LoadIngredientsAsync();
    }

    private void InitializeAllergenFilters()
    {
        // Add FDA Big 9 allergens as filterable options
        var allergenTypes = new[]
        {
            (AllergenType.Milk, "Milk/Dairy"),
            (AllergenType.Eggs, "Eggs"),
            (AllergenType.Fish, "Fish"),
            (AllergenType.Shellfish, "Shellfish"),
            (AllergenType.TreeNuts, "Tree Nuts"),
            (AllergenType.Peanuts, "Peanuts"),
            (AllergenType.Wheat, "Wheat"),
            (AllergenType.Soybeans, "Soy"),
            (AllergenType.Sesame, "Sesame"),
            (AllergenType.GlutenFree, "Gluten-Free"),
            (AllergenType.Vegan, "Vegan"),
            (AllergenType.Vegetarian, "Vegetarian")
        };

        foreach (var (type, name) in allergenTypes)
        {
            var item = new AllergenFilterItem { Type = type, Name = name };
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AllergenFilterItem.IsSelected))
                {
                    OnAllergenFilterChanged(item);
                }
            };
            AvailableAllergens.Add(item);
        }
    }

    private void OnAllergenFilterChanged(AllergenFilterItem item)
    {
        if (item.IsSelected)
        {
            _selectedAllergens.Add(item.Type);
        }
        else
        {
            _selectedAllergens.Remove(item.Type);
        }
        ActiveFilterCount = _selectedAllergens.Count;
        _ = LoadIngredientsAsync();
    }

    [RelayCommand]
    private void ClearAllFilters()
    {
        foreach (var allergen in AvailableAllergens)
        {
            allergen.IsSelected = false;
        }
        _selectedAllergens.Clear();
        ActiveFilterCount = 0;
        _ = LoadIngredientsAsync();
    }

    public async Task LoadIngredientsAsync()
    {
        IsLoading = true;
        try
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                ingredients = ingredients.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (i.Category != null && i.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (i.VendorName != null && i.VendorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    // NEW: Search aliases
                    (i.Aliases != null && i.Aliases.Any(a => a.AliasName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            // Filter by allergens (if any selected)
            if (_selectedAllergens.Count > 0)
            {
                ingredients = ingredients.Where(i =>
                    i.IngredientAllergens != null &&
                    i.IngredientAllergens.Any(ia =>
                        ia.IsEnabled &&
                        _selectedAllergens.Contains(ia.Allergen.Type))
                ).ToList();
            }

            // Apply quick filters
            ingredients = SelectedQuickFilter switch
            {
                "High Cost (>$10)" => ingredients.Where(i => i.CurrentPrice > 10).ToList(),
                "Low Cost (<$5)" => ingredients.Where(i => i.CurrentPrice < 5).ToList(),
                "Missing Prices" => ingredients.Where(i => i.CurrentPrice == 0).ToList(),
                "No Vendor" => ingredients.Where(i => string.IsNullOrWhiteSpace(i.VendorName)).ToList(),
                "No Category" => ingredients.Where(i => string.IsNullOrWhiteSpace(i.Category)).ToList(),
                _ => ingredients
            };

            // Apply sorting
            ingredients = SelectedSortOption switch
            {
                "Name (A-Z)" => ingredients.OrderBy(i => i.Name).ToList(),
                "Name (Z-A)" => ingredients.OrderByDescending(i => i.Name).ToList(),
                "Price (Low-High)" => ingredients.OrderBy(i => i.CurrentPrice).ToList(),
                "Price (High-Low)" => ingredients.OrderByDescending(i => i.CurrentPrice).ToList(),
                "Category (A-Z)" => ingredients.OrderBy(i => i.Category ?? "").ToList(),
                "Vendor (A-Z)" => ingredients.OrderBy(i => i.VendorName ?? "").ToList(),
                _ => ingredients.OrderBy(i => i.Name).ToList()
            };

            Ingredients.Clear();
            foreach (var ingredient in ingredients)
            {
                Ingredients.Add(new IngredientDisplayModel(ingredient));
            }

            // Update statistics
            TotalIngredients = Ingredients.Count;
            ValidIngredients = Ingredients.Count(i => i.IsValid);
            AveragePrice = Ingredients.Any()
                ? Ingredients.Average(i => i.Ingredient.CurrentPrice)
                : 0;
            UniqueCategories = Ingredients
                .Where(i => !string.IsNullOrWhiteSpace(i.Category))
                .Select(i => i.Category)
                .Distinct()
                .Count();

            // Calculate validation warnings
            ValidationWarningCount = Ingredients.Count(i => !i.IsValid);
            MissingPrices = Ingredients.Count(i => i.Ingredient.CurrentPrice == 0);
            MissingVendors = Ingredients.Count(i => string.IsNullOrWhiteSpace(i.Ingredient.VendorName));
            MissingCategories = Ingredients.Count(i => string.IsNullOrWhiteSpace(i.Ingredient.Category));
            MissingNutritionalData = Ingredients.Count(i =>
                !i.Ingredient.CaloriesPerUnit.HasValue &&
                !i.Ingredient.ProteinPerUnit.HasValue &&
                !i.Ingredient.CarbohydratesPerUnit.HasValue &&
                !i.Ingredient.FatPerUnit.HasValue);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddIngredient()
    {
        if (_ownerWindow == null) return;

        var window = new AddEditIngredientWindow();

        AddEditIngredientViewModel? viewModel = null;
        viewModel = new AddEditIngredientViewModel(
            _ingredientService,
            _validationService,
            _categoryColorService,
            _currentLocationService.CurrentLocationId,
            async () =>
            {
                window.Close();
                await LoadIngredientsAsync();
            },
            () => window.Close(),
            null, // existingIngredient
            _nutritionalDataService,
            _conversionRepository,
            window);

        window.DataContext = viewModel;
        window.SetViewModel(viewModel);
        await window.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task EditIngredient(Ingredient? ingredient)
    {
        if (ingredient == null || _ownerWindow == null) return;

        var window = new AddEditIngredientWindow();

        AddEditIngredientViewModel? viewModel = null;
        viewModel = new AddEditIngredientViewModel(
            _ingredientService,
            _validationService,
            _categoryColorService,
            _currentLocationService.CurrentLocationId,
            async () =>
            {
                window.Close();
                await LoadIngredientsAsync();
            },
            () => window.Close(),
            ingredient,
            _nutritionalDataService,
            _conversionRepository,
            window);

        window.DataContext = viewModel;
        window.SetViewModel(viewModel);
        await window.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task DeleteIngredient(Ingredient? ingredient)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("[DELETE START] IngredientsViewModel.DeleteIngredient");

            if (ingredient == null || _ownerWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("[DELETE ABORTED] Ingredient or window is null");
                return;
            }

            var ingredientId = ingredient.Id;
            System.Diagnostics.Debug.WriteLine($"[DELETE] Ingredient ID: {ingredientId}");
            System.Diagnostics.Debug.WriteLine($"[DELETE] Ingredient Name: {ingredient.Name}");
            System.Diagnostics.Debug.WriteLine($"[DELETE] Location ID: {_currentLocationService.CurrentLocationId}");

            // CRITICAL: Load a fresh, detached copy for recycle bin to avoid EF tracking conflicts
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 1] Loading fresh copy from database...");
            var freshCopy = await _ingredientService.GetIngredientByIdAsync(ingredientId);
            if (freshCopy == null)
            {
                System.Diagnostics.Debug.WriteLine("[DELETE ABORTED] Fresh copy is null");
                return;
            }
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 1 COMPLETE] Fresh copy loaded successfully");

            // Move to recycle bin instead of permanent delete
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 2] Moving to recycle bin...");
            await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Ingredient, _currentLocationService.CurrentLocationId);
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 2 COMPLETE] Moved to recycle bin successfully");

            System.Diagnostics.Debug.WriteLine("[DELETE STEP 3] Deleting ingredient from database...");
            await _ingredientService.DeleteIngredientAsync(ingredientId);
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 3 COMPLETE] Ingredient deleted successfully");

            System.Diagnostics.Debug.WriteLine("[DELETE STEP 4] Reloading ingredients list...");
            await LoadIngredientsAsync();
            System.Diagnostics.Debug.WriteLine("[DELETE STEP 4 COMPLETE] Ingredients list reloaded");

            // Notify MainWindowViewModel to update recycle bin count
            if (_onItemDeleted != null)
            {
                System.Diagnostics.Debug.WriteLine("[DELETE STEP 5] Notifying main window...");
                await _onItemDeleted();
                System.Diagnostics.Debug.WriteLine("[DELETE STEP 5 COMPLETE] Main window notified");
            }

            System.Diagnostics.Debug.WriteLine("[DELETE SUCCESS] All steps completed successfully");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("║ [DELETE EXCEPTION] IngredientsViewModel          ║");
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
            _notificationService.ShowError($"Failed to delete ingredient: {ex.Message}");
            throw; // Re-throw to see in debugger
        }
    }

    [RelayCommand]
    private async Task DuplicateIngredient(Ingredient? ingredient)
    {
        if (ingredient == null || _ownerWindow == null) return;

        // Create a copy of the ingredient
        var duplicate = new Ingredient
        {
            Name = $"{ingredient.Name} (Copy)",
            Category = ingredient.Category,
            VendorName = ingredient.VendorName,
            VendorSku = ingredient.VendorSku != null ? $"{ingredient.VendorSku}-COPY" : null,
            CurrentPrice = ingredient.CurrentPrice,
            Unit = ingredient.Unit,
            UseAlternateUnit = ingredient.UseAlternateUnit,
            AlternateUnit = ingredient.AlternateUnit,
            AlternateConversionQuantity = ingredient.AlternateConversionQuantity,
            AlternateConversionUnit = ingredient.AlternateConversionUnit,
            LocationId = ingredient.LocationId
        };

        // Copy allergens
        if (ingredient.IngredientAllergens != null)
        {
            duplicate.IngredientAllergens = new List<IngredientAllergen>();
            foreach (var allergen in ingredient.IngredientAllergens)
            {
                duplicate.IngredientAllergens.Add(new IngredientAllergen
                {
                    AllergenId = allergen.AllergenId,
                    IsAutoDetected = allergen.IsAutoDetected,
                    IsEnabled = allergen.IsEnabled,
                    SourceIngredients = allergen.SourceIngredients
                });
            }
        }

        // Copy aliases
        if (ingredient.Aliases != null)
        {
            duplicate.Aliases = new List<IngredientAlias>();
            foreach (var alias in ingredient.Aliases)
            {
                duplicate.Aliases.Add(new IngredientAlias
                {
                    AliasName = alias.AliasName,
                    IsPrimary = alias.IsPrimary
                });
            }
        }

        await _ingredientService.CreateIngredientAsync(duplicate);
        await LoadIngredientsAsync();
    }

    [RelayCommand]
    private async Task BulkImport()
    {
        if (_ownerWindow == null) return;

        var dialog = new BulkImportWindow();
        var viewModel = new BulkImportViewModel(
            _importMapService,
            _ingredientService,
            () => dialog.OnImportSuccess(),
            () => dialog.OnCancel(),
            _currentLocationService.CurrentLocationId,
            _ownerWindow.StorageProvider
        );

        dialog.DataContext = viewModel;
        await dialog.ShowDialog(_ownerWindow);

        if (dialog.WasImported)
        {
            await LoadIngredientsAsync();
        }
    }

    private static async Task<bool> ShowConfirmationDialog(Window owner, string title, string message)
    {
        try
        {
            var dialog = new Window
            {
                Title = title,
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var grid = new Grid
            {
                Margin = new Avalonia.Thickness(20),
                RowDefinitions = new RowDefinitions("*,Auto")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(buttonPanel, 1);

            var noButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Padding = new Avalonia.Thickness(10)
            };

            var yesButton = new Button
            {
                Content = "Delete",
                Width = 100,
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White,
                Padding = new Avalonia.Thickness(10)
            };

            noButton.Click += (s, e) => dialog.Close(false);
            yesButton.Click += (s, e) => dialog.Close(true);

            buttonPanel.Children.Add(noButton);
            buttonPanel.Children.Add(yesButton);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;

            var result = await dialog.ShowDialog<bool>(owner);
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing confirmation dialog: {ex.Message}");
            return false; // Default to cancel on error
        }
    }


    [RelayCommand]
    private async Task BatchDelete()
    {
        if (SelectedIngredients == null || SelectedIngredients.Count == 0 || _ownerWindow == null) return;

        // Enhanced confirmation with impact summary
        var totalValue = SelectedIngredients.Sum(i => i.Ingredient.CurrentPrice);
        var categoriesAffected = SelectedIngredients
            .Where(i => !string.IsNullOrWhiteSpace(i.Ingredient.Category))
            .Select(i => i.Ingredient.Category)
            .Distinct()
            .Count();

        var impactMessage = $"Are you sure you want to delete {SelectedIngredients.Count} ingredient(s)?\n\n";
        impactMessage += "ℹ️ Info:\n";
        impactMessage += $"• {SelectedIngredients.Count} ingredients will be moved to the Recycle Bin\n";
        impactMessage += "• Any recipes using these ingredients may be affected\n";
        impactMessage += $"• Total value: {totalValue:C2}\n";
        if (categoriesAffected > 0)
        {
            impactMessage += $"• {categoriesAffected} categories affected\n";
        }
        impactMessage += "\n💡 You can restore deleted items from the Recycle Bin.";

        var result = await ShowConfirmationDialog(
            _ownerWindow,
            $"Delete {SelectedIngredients.Count} Ingredients - Confirm Action",
            impactMessage);

        if (result)
        {
            var ingredientIds = SelectedIngredients.Select(i => i.Ingredient.Id).ToList();
            foreach (var ingredientId in ingredientIds)
            {
                // CRITICAL: Load fresh copy for recycle bin to avoid EF tracking conflicts
                var freshCopy = await _ingredientService.GetIngredientByIdAsync(ingredientId);
                if (freshCopy != null)
                {
                    // Move to recycle bin
                    await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Ingredient, _currentLocationService.CurrentLocationId);
                    await _ingredientService.DeleteIngredientAsync(ingredientId);
                }
            }
            await LoadIngredientsAsync();

            // Notify MainWindowViewModel to update recycle bin count
            if (_onItemDeleted != null)
                await _onItemDeleted();
        }
    }

    [RelayCommand]
    private async Task BatchDuplicate()
    {
        if (SelectedIngredients == null || SelectedIngredients.Count == 0) return;

        foreach (var displayModel in SelectedIngredients.ToList())
        {
            await DuplicateIngredient(displayModel.Ingredient);
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        SelectedIngredients.Clear();
        foreach (var ingredient in Ingredients)
        {
            SelectedIngredients.Add(ingredient);
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedIngredients.Clear();
    }

    [RelayCommand]
    private async Task ViewPriceHistory(Ingredient? ingredient)
    {
        if (ingredient == null || _ownerWindow == null) return;

        var window = new PriceHistoryWindow();
        var viewModel = new PriceHistoryViewModel(_priceHistoryService, ingredient);
        window.DataContext = viewModel;
        await window.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        if (_ownerWindow == null)
        {
            _notificationService.ShowError("Cannot export: Window not initialized.");
            return;
        }

        try
        {
            var saveDialog = await _ownerWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export Ingredients to Excel",
                SuggestedFileName = $"Ingredients_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Excel Files")
                    {
                        Patterns = new[] { "*.xlsx" }
                    }
                }
            });

            if (saveDialog != null)
            {
                var ingredients = Ingredients.Select(i => i.Ingredient).ToList();
                var filePath = await _excelExportService.ExportIngredientsToExcelAsync(ingredients, saveDialog.Path.LocalPath);

                // Open the folder containing the exported file
                var folder = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(folder))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = folder,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting ingredients: {ex}");
            _notificationService.ShowError($"Export failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BulkMapNutritionalData()
    {
        if (_nutritionalDataService == null)
        {
            _notificationService.ShowError("USDA API key not configured. Please set the USDA_API_KEY environment variable.");
            return;
        }

        if (_ownerWindow == null)
        {
            _notificationService.ShowError("Cannot open mapper: Window not initialized.");
            return;
        }

        try
        {
            // Get all ingredients (allow re-mapping or updating existing data)
            var allIngredients = Ingredients.Select(i => i.Ingredient).ToList();

            if (!allIngredients.Any())
            {
                _notificationService.ShowInfo("No ingredients to map. Add some ingredients first!");
                return;
            }

            var ingredientCollection = new ObservableCollection<Ingredient>(allIngredients);
            var mapperWindow = new NutritionalDataMapperWindow();

            var mapperViewModel = new NutritionalDataMapperViewModel(
                _nutritionalDataService,
                async (success) =>
                {
                    mapperWindow.Close();
                    if (success)
                    {
                        System.Diagnostics.Debug.WriteLine("========== SAVING NUTRITIONAL DATA ==========");
                        System.Diagnostics.Debug.WriteLine($"Number of ingredients to save: {ingredientCollection.Count}");

                        // Save all modified ingredients to database
                        foreach (var ingredient in ingredientCollection)
                        {
                            System.Diagnostics.Debug.WriteLine($"Saving ingredient: {ingredient.Name}");
                            System.Diagnostics.Debug.WriteLine($"  Calories: {ingredient.CaloriesPerUnit}");
                            System.Diagnostics.Debug.WriteLine($"  Protein: {ingredient.ProteinPerUnit}");
                            System.Diagnostics.Debug.WriteLine($"  Carbs: {ingredient.CarbohydratesPerUnit}");
                            System.Diagnostics.Debug.WriteLine($"  Fat: {ingredient.FatPerUnit}");

                            await _ingredientService.UpdateIngredientAsync(ingredient);

                            System.Diagnostics.Debug.WriteLine($"  ✓ Saved successfully");
                        }

                        System.Diagnostics.Debug.WriteLine("========== RELOADING INGREDIENTS ==========");
                        // Reload ingredients to reflect changes
                        await LoadIngredientsAsync();

                        System.Diagnostics.Debug.WriteLine("========== VERIFYING SAVED DATA ==========");
                        foreach (var ingredient in ingredientCollection)
                        {
                            var reloaded = Ingredients.FirstOrDefault(i => i.Ingredient.Id == ingredient.Id);
                            if (reloaded != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ingredient: {reloaded.Ingredient.Name}");
                                System.Diagnostics.Debug.WriteLine($"  Calories after reload: {reloaded.Ingredient.CaloriesPerUnit}");
                                System.Diagnostics.Debug.WriteLine($"  Protein after reload: {reloaded.Ingredient.ProteinPerUnit}");
                            }
                        }

                        _notificationService.ShowSuccess("Nutritional data applied successfully!");
                    }
                });

            mapperWindow.DataContext = mapperViewModel;
            await mapperViewModel.InitializeAsync(ingredientCollection);
            await mapperWindow.ShowDialog(_ownerWindow);

            // Reload after closing regardless, to update any partially mapped data
            await LoadIngredientsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in bulk nutritional data mapping: {ex}");
            _notificationService.ShowError($"Mapping failed: {ex.Message}");
        }
    }
}