using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class EntreeCardImportViewModel : ViewModelBase
{
    private readonly IEntreeCardImportService _importService;
    private readonly IEntreeService _entreeService;
    private readonly IIngredientMatchingService _matchingService;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IIngredientMatchMappingService? _mappingService;
    private readonly Action _onImportSuccess;
    private readonly Action _onCancel;
    private readonly Guid _locationId;
    private readonly IStorageProvider? _storageProvider;
    private EntreeCardImportResult? _importResult;

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string? _parseStatus;

    [ObservableProperty]
    private ObservableCollection<EntreeCardPreview> _entreePreviews = new();

    [ObservableProperty]
    private EntreeCardPreview? _selectedEntree;

    [ObservableProperty]
    private ObservableCollection<UnmatchedIngredient> _unmatchedIngredients = new();

    [ObservableProperty]
    private ObservableCollection<UnmatchedRecipe> _unmatchedRecipes = new();

    [ObservableProperty]
    private UnmatchedIngredient? _selectedUnmatchedIngredient;

    [ObservableProperty]
    private UnmatchedRecipe? _selectedUnmatchedRecipe;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> _errors = new();

    [ObservableProperty]
    private bool _canImport;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalEntrees;

    [ObservableProperty]
    private int _validEntrees;

    [ObservableProperty]
    private int _invalidEntrees;

    [ObservableProperty]
    private int _unmatchedIngredientCount;

    [ObservableProperty]
    private int _unmatchedRecipeCount;

    public string ImportSummary =>
        TotalEntrees > 0
            ? $"{ValidEntrees} valid, {InvalidEntrees} invalid entree(s) from {TotalEntrees} tab(s). {UnmatchedIngredientCount} unmatched ingredient(s), {UnmatchedRecipeCount} unmatched recipe(s)."
            : string.Empty;

    public EntreeCardImportViewModel(
        IEntreeCardImportService importService,
        IEntreeService entreeService,
        IIngredientMatchingService matchingService,
        Action onImportSuccess,
        Action onCancel,
        Guid locationId,
        IStorageProvider? storageProvider = null,
        IIngredientService? ingredientService = null,
        IRecipeService? recipeService = null,
        IIngredientMatchMappingService? mappingService = null)
    {
        _importService = importService;
        _entreeService = entreeService;
        _matchingService = matchingService;
        _ingredientService = ingredientService!;
        _recipeService = recipeService!;
        _mappingService = mappingService;
        _onImportSuccess = onImportSuccess;
        _onCancel = onCancel;
        _locationId = locationId;
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    private async Task BrowseFile()
    {
        if (_storageProvider == null) return;

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Entree Card File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Excel Files")
                {
                    Patterns = new[] { "*.xlsx", "*.xls" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        });

        if (files.Count > 0)
        {
            SelectedFilePath = files[0].Path.LocalPath;
            await ParseFile();
        }
    }

    private async Task ParseFile()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Errors.Clear();
            EntreePreviews.Clear();
            UnmatchedIngredients.Clear();
            UnmatchedRecipes.Clear();
            CanImport = false;

            ParseStatus = "Parsing entree cards...";

            _importResult = await _importService.ParseEntreeCardsAsync(SelectedFilePath, _locationId);

            if (_importResult.Success)
            {
                TotalEntrees = _importResult.TotalTabs;
                ValidEntrees = _importResult.ValidEntrees;
                InvalidEntrees = _importResult.InvalidEntrees;
                UnmatchedIngredientCount = _importResult.UnmatchedIngredients.Count;
                UnmatchedRecipeCount = _importResult.UnmatchedRecipes.Count;

                EntreePreviews = new ObservableCollection<EntreeCardPreview>(_importResult.EntreePreviews);
                UnmatchedIngredients = new ObservableCollection<UnmatchedIngredient>(_importResult.UnmatchedIngredients);
                UnmatchedRecipes = new ObservableCollection<UnmatchedRecipe>(_importResult.UnmatchedRecipes);

                ParseStatus = $"✓ Parsed {TotalEntrees} entree card(s)";

                if (_importResult.Errors.Any())
                {
                    ErrorMessage = $"⚠️ {_importResult.Errors.Count} error(s) occurred during parsing:";
                    foreach (var error in _importResult.Errors.Take(10))
                    {
                        Errors.Add(error);
                    }

                    if (_importResult.Errors.Count > 10)
                    {
                        Errors.Add($"... plus {_importResult.Errors.Count - 10} more error(s)");
                    }
                }

                // Can import if we have valid entrees
                CanImport = ValidEntrees > 0;

                // Show warning if there are unmatched items
                if (UnmatchedIngredientCount > 0 || UnmatchedRecipeCount > 0)
                {
                    ParseStatus += $" - {UnmatchedIngredientCount} ingredient(s) and {UnmatchedRecipeCount} recipe(s) need mapping";
                }
            }
            else
            {
                ErrorMessage = _importResult.ErrorMessage ?? "Failed to parse file";
                ParseStatus = "✗ Parse failed";
            }

            OnPropertyChanged(nameof(ImportSummary));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error parsing file: {ex.Message}";
            ParseStatus = "✗ Parse failed";
            CanImport = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void MapIngredient(IngredientMatchSuggestion suggestion)
    {
        if (SelectedUnmatchedIngredient == null)
            return;

        MapIngredientInternal(SelectedUnmatchedIngredient, suggestion);
    }

    /// <summary>
    /// Map an unmatched ingredient to a suggestion (can be called with both parameters)
    /// </summary>
    public void MapIngredientWithContext(UnmatchedIngredient unmatchedIngredient, IngredientMatchSuggestion suggestion)
    {
        MapIngredientInternal(unmatchedIngredient, suggestion);
    }

    private void MapIngredientInternal(UnmatchedIngredient unmatchedIngredient, IngredientMatchSuggestion suggestion)
    {
        // Map the unmatched ingredient
        unmatchedIngredient.MappedToIngredientId = suggestion.IngredientId;

        // Update all entree previews that use this ingredient
        foreach (var entree in EntreePreviews)
        {
            var ingredients = entree.DirectIngredients.Where(i =>
                i.IngredientName.Equals(unmatchedIngredient.Name, StringComparison.OrdinalIgnoreCase) &&
                !i.IsMatched).ToList();

            foreach (var ingredient in ingredients)
            {
                ingredient.IsMatched = true;
                ingredient.MatchedIngredientId = suggestion.IngredientId;
                ingredient.MatchedIngredientName = suggestion.IngredientName;
                ingredient.MatchConfidence = 100;
                ingredient.MatchMethod = "Manual";
            }

            // Revalidate if all components matched
            UpdateEntreeValidation(entree);
        }

        // Remove from unmatched list
        UnmatchedIngredients.Remove(unmatchedIngredient);
        UnmatchedIngredientCount = UnmatchedIngredients.Count;
        OnPropertyChanged(nameof(ImportSummary));

        UpdateParseStatus();
    }

    [RelayCommand]
    private void MapRecipe(RecipeMatchSuggestion suggestion)
    {
        if (SelectedUnmatchedRecipe == null)
            return;

        MapRecipeInternal(SelectedUnmatchedRecipe, suggestion);
    }

    /// <summary>
    /// Map an unmatched recipe to a suggestion (can be called with both parameters)
    /// </summary>
    public void MapRecipeWithContext(UnmatchedRecipe unmatchedRecipe, RecipeMatchSuggestion suggestion)
    {
        MapRecipeInternal(unmatchedRecipe, suggestion);
    }

    private void MapRecipeInternal(UnmatchedRecipe unmatchedRecipe, RecipeMatchSuggestion suggestion)
    {
        // Map the unmatched recipe
        unmatchedRecipe.MappedToRecipeId = suggestion.RecipeId;

        // Update all entree previews that use this recipe
        foreach (var entree in EntreePreviews)
        {
            var recipes = entree.RecipeComponents.Where(r =>
                r.RecipeName.Equals(unmatchedRecipe.Name, StringComparison.OrdinalIgnoreCase) &&
                !r.IsMatched).ToList();

            foreach (var recipe in recipes)
            {
                recipe.IsMatched = true;
                recipe.MatchedRecipeId = suggestion.RecipeId;
                recipe.MatchedRecipeName = suggestion.RecipeName;
                recipe.MatchConfidence = 100;
                recipe.MatchMethod = "Manual";
            }

            // Revalidate if all components matched
            UpdateEntreeValidation(entree);
        }

        // Remove from unmatched list
        UnmatchedRecipes.Remove(unmatchedRecipe);
        UnmatchedRecipeCount = UnmatchedRecipes.Count;
        OnPropertyChanged(nameof(ImportSummary));

        UpdateParseStatus();
    }

    private void UpdateEntreeValidation(EntreeCardPreview entree)
    {
        // Remove component matching warnings if all matched
        if (entree.DirectIngredients.All(i => i.IsMatched))
        {
            var warning = entree.ValidationWarnings.FirstOrDefault(w => w.Contains("ingredient(s) could not be matched"));
            if (warning != null)
            {
                entree.ValidationWarnings.Remove(warning);
            }
        }

        if (entree.RecipeComponents.All(r => r.IsMatched))
        {
            var warning = entree.ValidationWarnings.FirstOrDefault(w => w.Contains("recipe component(s) could not be matched"));
            if (warning != null)
            {
                entree.ValidationWarnings.Remove(warning);
            }
        }
    }

    private void UpdateParseStatus()
    {
        if (UnmatchedIngredientCount == 0 && UnmatchedRecipeCount == 0)
        {
            ParseStatus = $"✓ All components mapped - ready to import";
        }
        else
        {
            ParseStatus = $"✓ Parsed {TotalEntrees} entree card(s) - {UnmatchedIngredientCount} ingredient(s) and {UnmatchedRecipeCount} recipe(s) need mapping";
        }
    }

    [RelayCommand]
    private async Task BulkMatchComponents()
    {
        if ((_ingredientService == null || _recipeService == null || _mappingService == null) ||
            (UnmatchedIngredients.Count == 0 && UnmatchedRecipes.Count == 0))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Get all ingredients and recipes for matching
            var allIngredients = await _ingredientService.GetAllIngredientsAsync(_locationId);
            var allRecipes = await _recipeService.GetAllRecipesAsync(_locationId);

            if (!allIngredients.Any() && !allRecipes.Any())
            {
                ErrorMessage = "No ingredients or recipes found in database. Please add some first.";
                return;
            }

            // Combine unmatched ingredients and recipes into one list
            var unmatchedNames = new List<string>();
            unmatchedNames.AddRange(UnmatchedIngredients.Select(u => u.Name));
            unmatchedNames.AddRange(UnmatchedRecipes.Select(u => u.Name));

            // Open bulk matcher dialog
            var dialog = new BulkIngredientMatcherWindow();
            var viewModel = new BulkIngredientMatcherViewModel(
                unmatchedNames.Distinct().ToList(), // Remove duplicates if any
                allIngredients.ToList(),
                allRecipes.ToList(),
                _locationId,
                _mappingService,
                () => dialog.Close()
            );

            dialog.DataContext = viewModel;

            // Show dialog modally
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow != null)
            {
                await dialog.ShowDialog(desktop.MainWindow);
            }

            // After dialog closes, refresh matches
            await RefreshMatchesAfterBulkMapping();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to open bulk matcher: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshMatchesAfterBulkMapping()
    {
        if (_mappingService == null || _ingredientService == null || _recipeService == null)
            return;

        try
        {
            // Re-check all unmatched ingredients against saved mappings
            foreach (var unmatchedIngredient in UnmatchedIngredients.ToList())
            {
                var mapping = await _mappingService.GetMappingForNameAsync(unmatchedIngredient.Name, _locationId);

                if (mapping?.MatchedIngredientId.HasValue == true)
                {
                    var ingredient = await _ingredientService.GetIngredientByIdAsync(mapping.MatchedIngredientId.Value);
                    if (ingredient != null)
                    {
                        var suggestion = new IngredientMatchSuggestion
                        {
                            IngredientId = ingredient.Id,
                            IngredientName = ingredient.Name,
                            Confidence = 100,
                            Reason = "Saved Mapping"
                        };

                        MapIngredientInternal(unmatchedIngredient, suggestion);
                    }
                }
            }

            // Re-check all unmatched recipes against saved mappings
            foreach (var unmatchedRecipe in UnmatchedRecipes.ToList())
            {
                var mapping = await _mappingService.GetMappingForNameAsync(unmatchedRecipe.Name, _locationId);

                if (mapping?.MatchedRecipeId.HasValue == true)
                {
                    var recipe = await _recipeService.GetRecipeByIdAsync(mapping.MatchedRecipeId.Value);
                    if (recipe != null)
                    {
                        var suggestion = new RecipeMatchSuggestion
                        {
                            RecipeId = recipe.Id,
                            RecipeName = recipe.Name,
                            Confidence = 100,
                            Reason = "Saved Mapping"
                        };

                        MapRecipeInternal(unmatchedRecipe, suggestion);
                    }
                }
            }

            // Update UI
            OnPropertyChanged(nameof(ImportSummary));
            UpdateParseStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing matches: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (!EntreePreviews.Any() || _importResult == null)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Errors.Clear();

            int successCount = 0;
            var importErrors = new List<string>();

            // Import each valid entree
            foreach (var preview in EntreePreviews.Where(p => p.IsValid))
            {
                try
                {
                    // Create Entree object
                    var entree = new Entree
                    {
                        Id = Guid.NewGuid(),
                        Name = preview.Name,
                        Description = preview.Description,
                        Category = preview.Category,
                        MenuPrice = preview.MenuPrice,
                        PlatingEquipment = preview.PlatingEquipment,
                        PhotoUrl = preview.PhotoUrl, // Photo extracted from Excel
                        PreparationInstructions = preview.Procedures.Any() ? string.Join("\n", preview.Procedures) : null,
                        LocationId = _locationId,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    };

                    // Add direct ingredients
                    var entreeIngredients = new List<EntreeIngredient>();
                    foreach (var ingredientPreview in preview.DirectIngredients)
                    {
                        Guid ingredientId;

                        if (!ingredientPreview.IsMatched)
                        {
                            // Create placeholder ingredient for unmatched items
                            var placeholderIngredient = new Ingredient
                            {
                                Id = Guid.NewGuid(),
                                Name = ingredientPreview.IngredientName,
                                LocationId = _locationId,
                                Unit = ingredientPreview.Unit,
                                CurrentPrice = 0, // Placeholder - needs to be set later
                                Category = "[UNMATCHED - Import]",
                                CreatedAt = DateTime.UtcNow,
                                ModifiedAt = DateTime.UtcNow
                            };

                            await _ingredientService.CreateIngredientAsync(placeholderIngredient);
                            ingredientId = placeholderIngredient.Id;
                        }
                        else
                        {
                            ingredientId = ingredientPreview.MatchedIngredientId!.Value;
                        }

                        var entreeIngredient = new EntreeIngredient
                        {
                            Id = Guid.NewGuid(),
                            EntreeId = entree.Id,
                            IngredientId = ingredientId,
                            Quantity = ingredientPreview.Quantity,
                            Unit = ingredientPreview.Unit,
                            DisplayName = ingredientPreview.IngredientName, // Preserve original card name
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };

                        entreeIngredients.Add(entreeIngredient);
                    }

                    entree.EntreeIngredients = entreeIngredients;

                    // Add recipe components
                    var entreeRecipes = new List<EntreeRecipe>();
                    int sortOrder = 0;

                    foreach (var recipePreview in preview.RecipeComponents)
                    {
                        Guid recipeId;

                        if (!recipePreview.IsMatched)
                        {
                            // Create placeholder recipe for unmatched items
                            var placeholderRecipe = new Recipe
                            {
                                Id = Guid.NewGuid(),
                                Name = recipePreview.RecipeName,
                                LocationId = _locationId,
                                Yield = 1,
                                YieldUnit = recipePreview.Unit.ToString(),
                                Category = "[UNMATCHED - Import]",
                                CreatedAt = DateTime.UtcNow,
                                ModifiedAt = DateTime.UtcNow
                            };

                            await _recipeService.CreateRecipeAsync(placeholderRecipe);
                            recipeId = placeholderRecipe.Id;
                        }
                        else
                        {
                            recipeId = recipePreview.MatchedRecipeId!.Value;
                        }

                        var entreeRecipe = new EntreeRecipe
                        {
                            Id = Guid.NewGuid(),
                            EntreeId = entree.Id,
                            RecipeId = recipeId,
                            Quantity = recipePreview.Quantity,
                            Unit = recipePreview.Unit,
                            DisplayName = recipePreview.RecipeName, // Preserve original card name
                            SortOrder = sortOrder++,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };

                        entreeRecipes.Add(entreeRecipe);
                    }

                    entree.EntreeRecipes = entreeRecipes;

                    // Create entree in database
                    await _entreeService.CreateEntreeAsync(entree);
                    successCount++;
                }
                catch (Exception ex)
                {
                    importErrors.Add($"Failed to import '{preview.Name}': {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                ParseStatus = $"✓ Successfully imported {successCount} entree(s)";

                // Show success message then close
                await Task.Delay(1000);
                _onImportSuccess();
            }
            else
            {
                if (importErrors.Any())
                {
                    ErrorMessage = $"Import failed - {importErrors.Count} error(s) occurred:";
                    Errors.Clear();
                    foreach (var error in importErrors.Take(10))
                    {
                        Errors.Add(error);
                    }
                    if (importErrors.Count > 10)
                    {
                        Errors.Add($"... plus {importErrors.Count - 10} more error(s)");
                    }
                }
                else
                {
                    ErrorMessage = "No entrees were imported. Please verify the file contains valid entree data.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error during import: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }
}
