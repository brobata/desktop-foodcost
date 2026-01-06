// Location: Freecost.Desktop/ViewModels/RecipesViewModel.cs
// Action: UPDATE - Add IsLoading property

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Enums;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Desktop.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class RecipesViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRecipeCostCalculator _costCalculator;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeCardService _recipeCardService;
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly IExcelExportService _excelExportService;
    private readonly IRecycleBinService _recycleBinService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IStatusNotificationService _notificationService;
    private readonly IUserSessionService? _sessionService;
    private readonly Func<Task>? _onItemDeleted;
    private Window? _ownerWindow;
    private Guid _currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Default user for now

    [ObservableProperty]
    private ObservableCollection<RecipeDisplayModel> _recipes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedRecipeCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private RecipeDisplayModel? _selectedRecipe;

    [ObservableProperty]
    private ObservableCollection<RecipeDisplayModel> _selectedRecipes = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    // NEW: Loading indicator
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<AllergenFilterItem> _availableAllergens = new();

    [ObservableProperty]
    private int _activeFilterCount = 0;

    [ObservableProperty]
    private int _totalRecipes = 0;

    [ObservableProperty]
    private decimal _totalCost = 0;

    [ObservableProperty]
    private decimal _averageCostPerRecipe = 0;

    [ObservableProperty]
    private int _uniqueCategories = 0;

    [ObservableProperty]
    private string _selectedSortOption = "Name (A-Z)";

    public List<string> SortOptions { get; } = new List<string>
    {
        "Name (A-Z)",
        "Name (Z-A)",
        "Total Cost (Low-High)",
        "Total Cost (High-Low)",
        "Cost/Unit (Low-High)",
        "Cost/Unit (High-Low)",
        "Prep Time (Low-High)",
        "Prep Time (High-Low)",
        "Category (A-Z)"
    };

    [ObservableProperty]
    private string _selectedQuickFilter = "All Recipes";

    public List<string> QuickFilterOptions { get; } = new List<string>
    {
        "All Recipes",
        "High Cost (>$20)",
        "Low Cost (<$5)",
        "Long Prep (>60 min)",
        "Quick Prep (<15 min)",
        "No Category"
    };

    // New filter properties for v1.2.0/v1.3.0
    [ObservableProperty]
    private string _selectedDifficultyFilter = "All Levels";

    public List<string> DifficultyFilterOptions { get; } = new List<string>
    {
        "All Levels",
        "Not Set",
        "Easy",
        "Medium",
        "Hard",
        "Expert"
    };

    [ObservableProperty]
    private string _categoryFilter = string.Empty;

    [ObservableProperty]
    private string _tagsFilter = string.Empty;

    [ObservableProperty]
    private string _dietaryLabelsFilter = string.Empty;

    private readonly HashSet<AllergenType> _selectedAllergens = new();

    public bool HasSelectedRecipes => SelectedRecipe != null;
    public bool HasSingleSelection => SelectedRecipe != null;
    public string DeleteSelectedText => "Delete Selected";

    // Authentication state for UI binding
    public bool IsAuthenticated => _sessionService?.IsAuthenticated ?? false;

    public RecipesViewModel(
        IRecipeService recipeService,
        IServiceProvider serviceProvider,
        IRecipeCostCalculator costCalculator,
        ICurrentLocationService currentLocationService,
        IStatusNotificationService notificationService,
        Window? ownerWindow = null,
        Func<Task>? onItemDeleted = null)
    {
        _recipeService = recipeService;
        _serviceProvider = serviceProvider;
        _costCalculator = costCalculator;
        _currentLocationService = currentLocationService;
        _notificationService = notificationService;
        _sessionService = _serviceProvider.GetService(typeof(IUserSessionService)) as IUserSessionService;
        _ingredientService = _serviceProvider.GetRequiredService<IIngredientService>();
        _recipeCardService = _serviceProvider.GetRequiredService<IRecipeCardService>();
        _userPreferencesService = _serviceProvider.GetRequiredService<IUserPreferencesService>();
        _excelExportService = _serviceProvider.GetRequiredService<IExcelExportService>();
        _recycleBinService = _serviceProvider.GetRequiredService<IRecycleBinService>();
        _ownerWindow = ownerWindow;
        _onItemDeleted = onItemDeleted;
        _recipes = new ObservableCollection<RecipeDisplayModel>();
        InitializeAllergenFilters();
        _ = LoadUserPreferencesAsync();
        _ = LoadRecipesAsync();
    }

    private async Task LoadUserPreferencesAsync()
    {
        try
        {
            var prefs = await _userPreferencesService.GetPreferencesAsync(_currentUserId);
            if (prefs != null)
            {
                // Restore last used sort option
                if (!string.IsNullOrEmpty(prefs.DefaultRecipeSort) && SortOptions.Contains(prefs.DefaultRecipeSort))
                {
                    SelectedSortOption = prefs.DefaultRecipeSort;
                }

                // Restore last used filter
                if (!string.IsNullOrEmpty(prefs.DefaultRecipeFilter) && QuickFilterOptions.Contains(prefs.DefaultRecipeFilter))
                {
                    SelectedQuickFilter = prefs.DefaultRecipeFilter;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user preferences: {ex.Message}");
        }
    }

    private void InitializeAllergenFilters()
    {
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
        _ = LoadRecipesAsync();
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

        // Clear new filters
        SelectedDifficultyFilter = "All Levels";
        CategoryFilter = string.Empty;
        TagsFilter = string.Empty;
        DietaryLabelsFilter = string.Empty;

        _ = LoadRecipesAsync();
    }

    public void SetOwnerWindow(Window window)
    {
        _ownerWindow = window;
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadRecipesAsync();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        _ = LoadRecipesAsync();
        _ = SaveSortPreferenceAsync(value);
    }

    partial void OnSelectedQuickFilterChanged(string value)
    {
        _ = LoadRecipesAsync();
        _ = SaveFilterPreferenceAsync(value);
    }

    private async Task SaveSortPreferenceAsync(string sortOption)
    {
        try
        {
            await _userPreferencesService.UpdatePreferenceAsync(_currentUserId, "DefaultRecipeSort", sortOption);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving sort preference: {ex.Message}");
        }
    }

    private async Task SaveFilterPreferenceAsync(string filter)
    {
        try
        {
            await _userPreferencesService.UpdatePreferenceAsync(_currentUserId, "DefaultRecipeFilter", filter);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving filter preference: {ex.Message}");
        }
    }

    partial void OnSelectedDifficultyFilterChanged(string value)
    {
        _ = LoadRecipesAsync();
    }

    partial void OnCategoryFilterChanged(string value)
    {
        _ = LoadRecipesAsync();
    }

    partial void OnTagsFilterChanged(string value)
    {
        _ = LoadRecipesAsync();
    }

    partial void OnDietaryLabelsFilterChanged(string value)
    {
        _ = LoadRecipesAsync();
    }

    public async Task LoadRecipesAsync()
    {
        IsLoading = true; // NEW: Show spinner
        try
        {
            var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);

            if (!string.IsNullOrEmpty(SearchText))
            {
                recipes = recipes.Where(r =>
                    r.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // Filter by allergens (if any selected)
            if (_selectedAllergens.Count > 0)
            {
                recipes = recipes.Where(r =>
                    r.RecipeAllergens != null &&
                    r.RecipeAllergens.Any(ra =>
                        ra.IsEnabled &&
                        _selectedAllergens.Contains(ra.Allergen.Type))
                ).ToList();
            }

            // Filter by difficulty
            if (SelectedDifficultyFilter != "All Levels")
            {
                var targetDifficulty = SelectedDifficultyFilter switch
                {
                    "Not Set" => DifficultyLevel.NotSet,
                    "Easy" => DifficultyLevel.Easy,
                    "Medium" => DifficultyLevel.Medium,
                    "Hard" => DifficultyLevel.Hard,
                    "Expert" => DifficultyLevel.Expert,
                    _ => (DifficultyLevel?)null
                };

                if (targetDifficulty.HasValue)
                {
                    recipes = recipes.Where(r => r.Difficulty == targetDifficulty.Value).ToList();
                }
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                recipes = recipes.Where(r =>
                    r.Category?.Contains(CategoryFilter, StringComparison.OrdinalIgnoreCase) ?? false
                ).ToList();
            }

            // Filter by tags
            if (!string.IsNullOrWhiteSpace(TagsFilter))
            {
                var searchTags = TagsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower()).ToList();

                recipes = recipes.Where(r =>
                    !string.IsNullOrWhiteSpace(r.Tags) &&
                    searchTags.Any(searchTag =>
                        r.Tags.Contains(searchTag, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Filter by dietary labels
            if (!string.IsNullOrWhiteSpace(DietaryLabelsFilter))
            {
                var searchLabels = DietaryLabelsFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().ToLower()).ToList();

                recipes = recipes.Where(r =>
                    !string.IsNullOrWhiteSpace(r.DietaryLabels) &&
                    searchLabels.Any(searchLabel =>
                        r.DietaryLabels.Contains(searchLabel, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Apply quick filters
            recipes = SelectedQuickFilter switch
            {
                "High Cost (>$20)" => recipes.Where(r => r.TotalCost > 20).ToList(),
                "Low Cost (<$5)" => recipes.Where(r => r.TotalCost < 5).ToList(),
                "Long Prep (>60 min)" => recipes.Where(r => (r.PrepTimeMinutes ?? 0) > 60).ToList(),
                "Quick Prep (<15 min)" => recipes.Where(r => (r.PrepTimeMinutes ?? 0) < 15 && r.PrepTimeMinutes > 0).ToList(),
                "No Category" => recipes.Where(r => string.IsNullOrWhiteSpace(r.Category)).ToList(),
                _ => recipes
            };

            // Apply sorting
            recipes = SelectedSortOption switch
            {
                "Name (A-Z)" => recipes.OrderBy(r => r.Name).ToList(),
                "Name (Z-A)" => recipes.OrderByDescending(r => r.Name).ToList(),
                "Total Cost (Low-High)" => recipes.OrderBy(r => r.TotalCost).ToList(),
                "Total Cost (High-Low)" => recipes.OrderByDescending(r => r.TotalCost).ToList(),
                "Cost/Unit (Low-High)" => recipes.OrderBy(r => r.Yield > 0 ? r.TotalCost / r.Yield : r.TotalCost).ToList(),
                "Cost/Unit (High-Low)" => recipes.OrderByDescending(r => r.Yield > 0 ? r.TotalCost / r.Yield : r.TotalCost).ToList(),
                "Prep Time (Low-High)" => recipes.OrderBy(r => r.PrepTimeMinutes ?? 0).ToList(),
                "Prep Time (High-Low)" => recipes.OrderByDescending(r => r.PrepTimeMinutes ?? 0).ToList(),
                "Category (A-Z)" => recipes.OrderBy(r => r.Category ?? "").ToList(),
                _ => recipes.OrderBy(r => r.Name).ToList()
            };

            Recipes.Clear();
            foreach (var recipe in recipes)
            {
                Recipes.Add(new RecipeDisplayModel(recipe, _costCalculator));
            }

            // Update statistics
            TotalRecipes = Recipes.Count;
            TotalCost = Recipes.Sum(r => r.Recipe.TotalCost);
            AverageCostPerRecipe = Recipes.Any()
                ? Recipes.Average(r => r.Recipe.TotalCost)
                : 0;
            UniqueCategories = Recipes
                .Where(r => !string.IsNullOrWhiteSpace(r.Recipe.Category))
                .Select(r => r.Recipe.Category)
                .Distinct()
                .Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading recipes: {ex.Message}");
        }
        finally
        {
            IsLoading = false; // NEW: Hide spinner
        }
    }

    [RelayCommand]
    private async Task AddRecipe()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ║ Starting AddRecipe                                ║");
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╠═══════════════════════════════════════════════════╣");

            if (_ownerWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("[RECIPES VM] Owner window is null - returning");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[RECIPES VM] Creating AddEditRecipeWindow dialog...");
            var dialog = new Views.AddEditRecipeWindow();
            var viewModel = new AddEditRecipeViewModel(
                () => dialog.OnSaveSuccess(),
                () => dialog.OnCancel(),
                _ingredientService,
                _recipeService,
                _serviceProvider.GetRequiredService<IAllergenDetectionService>(),
                _serviceProvider.GetRequiredService<IRecipeCardService>(),
                _serviceProvider.GetRequiredService<IPhotoService>(),
                _serviceProvider.GetRequiredService<IValidationService>(),
                _currentLocationService,
                _serviceProvider.GetRequiredService<IRecipeCostCalculator>(),
                _ownerWindow
            );
            dialog.DataContext = viewModel;

            System.Diagnostics.Debug.WriteLine("[RECIPES VM] Showing dialog...");
            await dialog.ShowDialog(_ownerWindow);
            System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Dialog closed. WasSaved={dialog.WasSaved}");

            if (dialog.WasSaved)
            {
                System.Diagnostics.Debug.WriteLine("[RECIPES VM] Recipe was saved - processing...");
                var newRecipe = viewModel.GetRecipe();
                if (newRecipe != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Recipe retrieved: '{newRecipe.Name}', ID={newRecipe.Id}");

                    // Add allergens from the selector control
                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] Getting selected allergens...");
                    var selectedAllergens = dialog.GetAllergenSelector().GetSelectedAllergens();
                    System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Selected allergens count: {selectedAllergens.Count}");

                    newRecipe.RecipeAllergens = selectedAllergens.Select(a => new Freecost.Core.Models.RecipeAllergen
                    {
                        Id = Guid.NewGuid(),
                        AllergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                        IsAutoDetected = a.IsAutoDetected,
                        IsEnabled = a.IsSelected,
                        SourceIngredients = a.SourceIngredients
                    }).ToList();

                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] Calling CreateRecipeAsync...");
                    await _recipeService.CreateRecipeAsync(newRecipe);
                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] ✓ CreateRecipeAsync complete");

                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] Calling LoadRecipesAsync to refresh UI...");
                    await LoadRecipesAsync();
                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] ✓ LoadRecipesAsync complete");
                    System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Recipes count after load: {Recipes.Count}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[RECIPES VM] newRecipe is null - skipping save");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[RECIPES VM] Recipe was NOT saved (user cancelled)");
            }

            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╚═══════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╔═══════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ║ [EXCEPTION IN AddRecipe]                          ║");
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╠═══════════════════════════════════════════════════╣");
            System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"[RECIPES VM] Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
            System.Diagnostics.Debug.WriteLine("[RECIPES VM] ╚═══════════════════════════════════════════════════╝");
            _notificationService.ShowError($"Error adding recipe: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (_ownerWindow == null) return;

        try
        {
            var saveDialog = await _ownerWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export Recipes to Excel",
                SuggestedFileName = $"Recipes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
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
                var recipes = Recipes.Select(r => r.Recipe).ToList();
                await _excelExportService.ExportRecipesToExcelAsync(recipes, saveDialog.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Export failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (_ownerWindow == null) return;

        try
        {
            var openDialog = await _ownerWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Import Recipes from Excel",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Excel Files")
                    {
                        Patterns = new[] { "*.xlsx" }
                    }
                }
            });

            if (openDialog != null && openDialog.Count > 0)
            {
                var filePath = openDialog[0].Path.LocalPath;
                var importedRecipes = await _excelExportService.ImportRecipesFromExcelAsync(filePath);

                // Save imported recipes
                foreach (var recipe in importedRecipes)
                {
                    recipe.LocationId = _currentLocationService.CurrentLocationId;
                    await _recipeService.CreateRecipeAsync(recipe);
                }

                await LoadRecipesAsync();
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Import failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportRecipeCards()
    {
        if (_ownerWindow == null) return;

        try
        {
            var importService = _serviceProvider.GetRequiredService<IRecipeCardImportService>();
            var matchingService = _serviceProvider.GetRequiredService<IIngredientMatchingService>();
            var mappingService = _serviceProvider.GetRequiredService<IIngredientMatchMappingService>();

            var dialog = new Views.RecipeCardImportWindow();
            var viewModel = new RecipeCardImportViewModel(
                importService,
                _recipeService,
                matchingService,
                _ingredientService,
                async () =>
                {
                    await LoadRecipesAsync();
                    dialog.Close();
                },
                () => dialog.Close(),
                _currentLocationService.CurrentLocationId,
                _ownerWindow.StorageProvider,
                mappingService
            );
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Recipe card import failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportFromUrl()
    {
        if (_ownerWindow == null) return;

        try
        {
            var urlImportService = _serviceProvider.GetRequiredService<IRecipeUrlImportService>();
            var dialog = new Views.ImportRecipeFromUrlWindow(urlImportService);

            await dialog.ShowDialog(_ownerWindow);

            if (dialog.WasImported && dialog.ImportedRecipe != null)
            {
                var importedRecipe = dialog.ImportedRecipe;

                // Set location for the recipe
                importedRecipe.LocationId = _currentLocationService.CurrentLocationId;

                // Open the Add/Edit dialog to let user review and match ingredients
                var editDialog = new Views.AddEditRecipeWindow();
                var viewModel = new AddEditRecipeViewModel(
                    () => editDialog.OnSaveSuccess(),
                    () => editDialog.OnCancel(),
                    _ingredientService,
                    _recipeService,
                    _serviceProvider.GetRequiredService<IAllergenDetectionService>(),
                    _serviceProvider.GetRequiredService<IRecipeCardService>(),
                    _serviceProvider.GetRequiredService<IPhotoService>(),
                    _serviceProvider.GetRequiredService<IValidationService>(),
                    _currentLocationService,
                    _serviceProvider.GetRequiredService<IRecipeCostCalculator>(),
                    _ownerWindow,
                    importedRecipe // Pass imported recipe for editing
                );
                editDialog.DataContext = viewModel;

                await editDialog.ShowDialog(_ownerWindow);

                if (editDialog.WasSaved)
                {
                    var finalRecipe = viewModel.GetRecipe();
                    if (finalRecipe != null)
                    {
                        // Add allergens from the selector control
                        var selectedAllergens = editDialog.GetAllergenSelector().GetSelectedAllergens();
                        finalRecipe.RecipeAllergens = selectedAllergens.Select(a => new Freecost.Core.Models.RecipeAllergen
                        {
                            Id = Guid.NewGuid(),
                            AllergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                            IsAutoDetected = a.IsAutoDetected,
                            IsEnabled = a.IsSelected,
                            SourceIngredients = a.SourceIngredients
                        }).ToList();

                        await _recipeService.CreateRecipeAsync(finalRecipe);
                        await LoadRecipesAsync();

                        _notificationService.ShowSuccess($"Recipe '{finalRecipe.Name}' imported successfully!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"URL import failed: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(HasSingleSelection))]
    private async Task EditSelectedRecipe()
    {
        if (SelectedRecipe?.Recipe != null)
        {
            await EditRecipe(SelectedRecipe.Recipe);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedRecipes))]
    private async Task DeleteSelected()
    {
        if (SelectedRecipe?.Recipe != null)
        {
            await DeleteRecipe(SelectedRecipe.Recipe);
        }
    }

    [RelayCommand]
    public async Task EditRecipe(Recipe? recipe)
    {
        if (recipe == null || _ownerWindow == null) return;

        var dialog = new Views.AddEditRecipeWindow();
        var viewModel = new AddEditRecipeViewModel(
            () => dialog.OnSaveSuccess(),
            () => dialog.OnCancel(),
            _ingredientService,
            _recipeService,
            _serviceProvider.GetRequiredService<IAllergenDetectionService>(),
            _serviceProvider.GetRequiredService<IRecipeCardService>(),
            _serviceProvider.GetRequiredService<IPhotoService>(),
            _serviceProvider.GetRequiredService<IValidationService>(),
            _currentLocationService,
            _serviceProvider.GetRequiredService<IRecipeCostCalculator>(),
            _ownerWindow,
            recipe
        );
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(_ownerWindow);

        if (dialog.WasSaved)
        {
            var updatedRecipe = viewModel.GetRecipe();
            if (updatedRecipe != null)
            {
                // Add allergens from the selector control
                var selectedAllergens = dialog.GetAllergenSelector().GetSelectedAllergens();
                updatedRecipe.RecipeAllergens = selectedAllergens.Select(a => new Freecost.Core.Models.RecipeAllergen
                {
                    Id = Guid.NewGuid(),
                    AllergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(a.AllergenType),
                    IsAutoDetected = a.IsAutoDetected,
                    IsEnabled = a.IsSelected,
                    SourceIngredients = a.SourceIngredients
                }).ToList();

                await _recipeService.UpdateRecipeAsync(updatedRecipe);
                await LoadRecipesAsync();
            }
        }
    }

    [RelayCommand]
    public async Task DeleteRecipe(Recipe? recipe)
    {
        if (recipe == null || _ownerWindow == null) return;

        // CRITICAL: Load a fresh, detached copy for recycle bin to avoid EF tracking conflicts
        var recipeId = recipe.Id;
        var freshCopy = await _recipeService.GetRecipeByIdAsync(recipeId);
        if (freshCopy == null) return;

        // Move to recycle bin before deleting
        await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Recipe, _currentLocationService.CurrentLocationId);
        await _recipeService.DeleteRecipeAsync(recipeId);
        await LoadRecipesAsync();

        // Notify MainWindowViewModel to update recycle bin count
        if (_onItemDeleted != null)
            await _onItemDeleted();
    }

    [RelayCommand]
    public async Task DuplicateRecipe(Recipe? recipe)
    {
        if (recipe == null) return;

        // Fetch full recipe with all relationships
        var fullRecipe = await _recipeService.GetRecipeByIdAsync(recipe.Id);
        if (fullRecipe == null) return;

        // Create a copy of the recipe
        var duplicate = new Recipe
        {
            Name = $"{fullRecipe.Name} (Copy)",
            Description = fullRecipe.Description,
            Category = fullRecipe.Category,
            Yield = fullRecipe.Yield,
            YieldUnit = fullRecipe.YieldUnit,
            PrepTimeMinutes = fullRecipe.PrepTimeMinutes,
            PhotoUrl = fullRecipe.PhotoUrl,
            LocationId = fullRecipe.LocationId
        };

        // Copy recipe ingredients
        if (fullRecipe.RecipeIngredients != null)
        {
            duplicate.RecipeIngredients = new List<RecipeIngredient>();
            foreach (var ri in fullRecipe.RecipeIngredients)
            {
                duplicate.RecipeIngredients.Add(new RecipeIngredient
                {
                    IngredientId = ri.IngredientId,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit,
                    DisplayText = ri.DisplayText
                });
            }
        }

        // Copy allergens
        if (fullRecipe.RecipeAllergens != null)
        {
            duplicate.RecipeAllergens = new List<RecipeAllergen>();
            foreach (var allergen in fullRecipe.RecipeAllergens)
            {
                duplicate.RecipeAllergens.Add(new RecipeAllergen
                {
                    AllergenId = allergen.AllergenId,
                    IsAutoDetected = allergen.IsAutoDetected,
                    IsEnabled = allergen.IsEnabled,
                    SourceIngredients = allergen.SourceIngredients
                });
            }
        }

        await _recipeService.CreateRecipeAsync(duplicate);
        await LoadRecipesAsync();
    }

    [RelayCommand]
    public async Task GenerateRecipePdf(Recipe? recipe)
    {
        if (recipe == null) return;

        try
        {
            var pdfPath = await _recipeCardService.GenerateRecipeCardPdfAsync(recipe);

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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating PDF: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BatchDelete()
    {
        if (SelectedRecipes == null || SelectedRecipes.Count == 0 || _ownerWindow == null) return;

        // Enhanced confirmation with impact summary
        var impactMessage = $"Are you sure you want to delete {SelectedRecipes.Count} recipe(s)?\n\n";
        impactMessage += "ℹ️ Info:\n";
        impactMessage += $"• {SelectedRecipes.Count} recipes will be moved to the Recycle Bin\n";

        var totalCost = SelectedRecipes.Sum(r => r.Recipe.TotalCost);
        impactMessage += $"• Total combined cost: {totalCost:C2}\n";

        var totalIngredients = SelectedRecipes.Sum(r => r.Recipe.RecipeIngredients?.Count ?? 0);
        impactMessage += $"• {totalIngredients} total ingredient references\n";

        var categoriesAffected = SelectedRecipes
            .Where(r => !string.IsNullOrWhiteSpace(r.Recipe.Category))
            .Select(r => r.Recipe.Category)
            .Distinct()
            .Count();
        if (categoriesAffected > 0)
        {
            impactMessage += $"• {categoriesAffected} categories affected\n";
        }

        impactMessage += "\n💡 You can restore deleted items from the Recycle Bin.";

        var result = await ShowConfirmationDialog(_ownerWindow,
            $"Delete {SelectedRecipes.Count} Recipes - Confirm Action",
            impactMessage);

        if (result)
        {
            var recipeIds = SelectedRecipes.Select(r => r.Recipe.Id).ToList();
            foreach (var recipeId in recipeIds)
            {
                // CRITICAL: Load fresh copy for recycle bin to avoid EF tracking conflicts
                var freshCopy = await _recipeService.GetRecipeByIdAsync(recipeId);
                if (freshCopy != null)
                {
                    // Move to recycle bin before deleting
                    await _recycleBinService.MoveToRecycleBinAsync(freshCopy, DeletedItemType.Recipe, _currentLocationService.CurrentLocationId);
                    await _recipeService.DeleteRecipeAsync(recipeId);
                }
            }
            await LoadRecipesAsync();

            // Notify MainWindowViewModel to update recycle bin count
            if (_onItemDeleted != null)
                await _onItemDeleted();
        }
    }

    [RelayCommand]
    private async Task BatchDuplicate()
    {
        if (SelectedRecipes == null || SelectedRecipes.Count == 0) return;

        foreach (var displayModel in SelectedRecipes.ToList())
        {
            await DuplicateRecipe(displayModel.Recipe);
        }
    }

    [RelayCommand]
    private async Task CompareRecipes()
    {
        if (SelectedRecipes == null || SelectedRecipes.Count < 2 || SelectedRecipes.Count > 3 || _ownerWindow == null)
        {
            // TODO: Show message that 2-3 recipes must be selected
            return;
        }

        // Fetch full recipes with all data
        var recipes = new System.Collections.Generic.List<Recipe>();
        foreach (var selected in SelectedRecipes)
        {
            var fullRecipe = await _recipeService.GetRecipeByIdAsync(selected.Recipe.Id);
            if (fullRecipe != null)
            {
                await _costCalculator.CalculateRecipeTotalCostAsync(fullRecipe);
                recipes.Add(fullRecipe);
            }
        }

        if (recipes.Count >= 2)
        {
            var dialog = new Views.RecipeComparisonWindow();
            var viewModel = new RecipeComparisonViewModel(recipes.ToArray());
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(_ownerWindow);
        }
    }

    [RelayCommand]
    private async Task GenerateShoppingList()
    {
        if (SelectedRecipes == null || SelectedRecipes.Count == 0 || _ownerWindow == null) return;

        var selectedRecipeIds = SelectedRecipes.Select(r => r.Recipe.Id).ToList();

        var dialog = new Views.ShoppingListWindow();
        var viewModel = new ShoppingListViewModel(
            _recipeService,
            _serviceProvider.GetService(typeof(IIngredientService)) as IIngredientService ?? throw new InvalidOperationException("IIngredientService not found"),
            selectedRecipeIds,
            () => dialog.Close()
        );

        dialog.DataContext = viewModel;
        await dialog.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task GenerateBatchPdfs()
    {
        if (SelectedRecipes == null || SelectedRecipes.Count == 0) return;

        IsLoading = true;
        try
        {
            // Get full recipe details for selected recipes
            var recipes = new List<Recipe>();
            foreach (var selected in SelectedRecipes)
            {
                var fullRecipe = await _recipeService.GetRecipeByIdAsync(selected.Recipe.Id);
                if (fullRecipe != null)
                {
                    await _costCalculator.CalculateRecipeTotalCostAsync(fullRecipe);
                    recipes.Add(fullRecipe);
                }
            }

            var result = await _recipeCardService.GenerateBatchRecipeCardPdfsAsync(recipes);

            // Log results
            System.Diagnostics.Debug.WriteLine($"Batch PDF Generation: {result.SuccessCount}/{result.TotalRecipes} successful");
            if (result.FailureCount > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
            }

            // Open folder with generated PDFs if any were successful
            if (result.SuccessCount > 0 && result.GeneratedFiles.Any())
            {
                var folderPath = System.IO.Path.GetDirectoryName(result.GeneratedFiles.First());
                if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating batch PDFs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenPortionCalculator(Recipe? recipe)
    {
        if (recipe == null || _ownerWindow == null) return;

        // Fetch full recipe with ingredients
        var fullRecipe = await _recipeService.GetRecipeByIdAsync(recipe.Id);
        if (fullRecipe == null) return;

        var dialog = new Views.PortionCalculatorWindow();
        var viewModel = new PortionCalculatorViewModel(fullRecipe);
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private async Task OpenCostAnalysis(Recipe? recipe)
    {
        if (recipe == null || _ownerWindow == null) return;

        // Fetch full recipe with ingredients and calculated costs
        var fullRecipe = await _recipeService.GetRecipeByIdAsync(recipe.Id);
        if (fullRecipe == null) return;

        // Calculate costs
        await _costCalculator.CalculateRecipeTotalCostAsync(fullRecipe);

        var dialog = new Views.RecipeCostAnalysisWindow();
        var viewModel = new RecipeCostAnalysisViewModel(fullRecipe);
        dialog.DataContext = viewModel;

        await dialog.ShowDialog(_ownerWindow);
    }

    [RelayCommand]
    private void SelectAll()
    {
        SelectedRecipes.Clear();
        foreach (var recipe in Recipes)
        {
            SelectedRecipes.Add(recipe);
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedRecipes.Clear();
    }

    private async Task<bool> ShowConfirmationDialog(Window owner, string title, string message)
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
                Margin = new Thickness(20),
                RowDefinitions = new RowDefinitions("*,Auto")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
            };
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(buttonPanel, 1);

            var noButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Padding = new Thickness(10)
            };
            noButton.Click += (s, e) => dialog.Close(false);

            var yesButton = new Button
            {
                Content = "Delete",
                Width = 100,
                Background = Avalonia.Media.Brushes.Red,
                Foreground = Avalonia.Media.Brushes.White,
                Padding = new Thickness(10)
            };
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
}