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

public partial class RecipeCardImportViewModel : ViewModelBase
{
    private readonly IRecipeCardImportService _importService;
    private readonly IRecipeService _recipeService;
    private readonly IIngredientMatchingService _matchingService;
    private readonly IIngredientService _ingredientService;
    private readonly IIngredientMatchMappingService? _mappingService;
    private readonly Action _onImportSuccess;
    private readonly Action _onCancel;
    private readonly Guid _locationId;
    private readonly IStorageProvider? _storageProvider;
    private RecipeCardImportResult? _importResult;

    [ObservableProperty]
    private string? _selectedFilePath;

    [ObservableProperty]
    private string? _parseStatus;

    [ObservableProperty]
    private ObservableCollection<RecipeCardPreview> _recipePreviews = new();

    [ObservableProperty]
    private RecipeCardPreview? _selectedRecipe;

    [ObservableProperty]
    private ObservableCollection<UnmatchedIngredient> _unmatchedIngredients = new();

    [ObservableProperty]
    private UnmatchedIngredient? _selectedUnmatchedIngredient;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> _errors = new();

    [ObservableProperty]
    private bool _canImport;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalRecipes;

    [ObservableProperty]
    private int _validRecipes;

    [ObservableProperty]
    private int _invalidRecipes;

    [ObservableProperty]
    private int _unmatchedCount;

    public string ImportSummary =>
        TotalRecipes > 0
            ? $"{ValidRecipes} valid, {InvalidRecipes} invalid recipe(s) from {TotalRecipes} tab(s). {UnmatchedCount} unmatched ingredient(s)."
            : string.Empty;

    public RecipeCardImportViewModel(
        IRecipeCardImportService importService,
        IRecipeService recipeService,
        IIngredientMatchingService matchingService,
        IIngredientService ingredientService,
        Action onImportSuccess,
        Action onCancel,
        Guid locationId,
        IStorageProvider? storageProvider = null,
        IIngredientMatchMappingService? mappingService = null)
    {
        _importService = importService;
        _recipeService = recipeService;
        _matchingService = matchingService;
        _ingredientService = ingredientService;
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
            Title = "Select Recipe Card File",
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
            RecipePreviews.Clear();
            UnmatchedIngredients.Clear();
            CanImport = false;

            ParseStatus = "Parsing recipe cards...";

            _importResult = await _importService.ParseRecipeCardsAsync(SelectedFilePath, _locationId);

            if (_importResult.Success)
            {
                TotalRecipes = _importResult.TotalTabs;
                ValidRecipes = _importResult.ValidRecipes;
                InvalidRecipes = _importResult.InvalidRecipes;
                UnmatchedCount = _importResult.UnmatchedIngredients.Count;

                RecipePreviews = new ObservableCollection<RecipeCardPreview>(_importResult.RecipePreviews);
                UnmatchedIngredients = new ObservableCollection<UnmatchedIngredient>(_importResult.UnmatchedIngredients);

                ParseStatus = $"✓ Parsed {TotalRecipes} recipe card(s)";

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

                // Can import if we have valid recipes (with or without unmatched ingredients)
                CanImport = ValidRecipes > 0;

                // Show warning if there are unmatched ingredients
                if (UnmatchedCount > 0)
                {
                    ParseStatus += $" - {UnmatchedCount} ingredient(s) need mapping";
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
        // Map the unmatched ingredient to the selected suggestion
        unmatchedIngredient.MappedToIngredientId = suggestion.IngredientId;

        // Update all recipe previews that use this ingredient
        foreach (var recipe in RecipePreviews)
        {
            var ingredients = recipe.Ingredients.Where(i =>
                i.IngredientName.Equals(unmatchedIngredient.Name, StringComparison.OrdinalIgnoreCase) &&
                !i.IsMatched).ToList();

            foreach (var ingredient in ingredients)
            {
                ingredient.IsMatched = true;
                ingredient.MatchedIngredientId = suggestion.IngredientId;
                ingredient.MatchedIngredientName = suggestion.IngredientName;
                ingredient.MatchConfidence = 100; // Manual mapping is 100%
                ingredient.MatchMethod = "Manual";
            }

            // Revalidate recipe
            if (recipe.Ingredients.All(i => i.IsMatched))
            {
                var warning = recipe.ValidationWarnings.FirstOrDefault(w => w.Contains("could not be matched"));
                if (warning != null)
                {
                    recipe.ValidationWarnings.Remove(warning);
                }
            }
        }

        // Remove from unmatched list
        UnmatchedIngredients.Remove(unmatchedIngredient);
        UnmatchedCount = UnmatchedIngredients.Count;
        OnPropertyChanged(nameof(ImportSummary));

        // Refresh status
        if (UnmatchedCount == 0)
        {
            ParseStatus = $"✓ All ingredients mapped - ready to import";
        }
        else
        {
            ParseStatus = $"✓ Parsed {TotalRecipes} recipe card(s) - {UnmatchedCount} ingredient(s) need mapping";
        }
    }

    [RelayCommand]
    private async Task ChooseExistingIngredient(UnmatchedIngredient unmatchedIngredient)
    {
        if (unmatchedIngredient == null)
            return;

        try
        {
            // Get all ingredients for this location
            var allIngredients = await _ingredientService.GetAllIngredientsAsync(_locationId);

            if (!allIngredients.Any())
            {
                ErrorMessage = "No ingredients found in database";
                return;
            }

            // Sort alphabetically
            var sortedIngredients = allIngredients.OrderBy(i => i.Name).ToList();

            // Open ingredient selector window
            var dialog = new Views.IngredientSelectorWindow();
            var viewModel = new IngredientSelectorViewModel(
                sortedIngredients,
                selectedIngredient =>
                {
                    // User selected an ingredient - map it to the unmatched ingredient
                    SelectedUnmatchedIngredient = unmatchedIngredient;

                    var suggestion = new IngredientMatchSuggestion
                    {
                        IngredientId = selectedIngredient.Id,
                        IngredientName = selectedIngredient.Name,
                        Confidence = 100,
                        Reason = "Manual Selection"
                    };

                    MapIngredient(suggestion);
                    dialog.Close();
                },
                () => dialog.Close()
            );

            dialog.DataContext = viewModel;

            // Show dialog modally
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow != null)
            {
                await dialog.ShowDialog(desktop.MainWindow);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load ingredients: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task BulkMatchIngredients()
    {
        if (!UnmatchedIngredients.Any() || _mappingService == null)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Get all ingredients and recipes for matching
            var allIngredients = await _ingredientService.GetAllIngredientsAsync(_locationId);
            var allRecipes = await _recipeService.GetAllRecipesAsync(_locationId);

            if (!allIngredients.Any())
            {
                ErrorMessage = "No ingredients found in database. Please add ingredients first.";
                return;
            }

            // Get list of unmatched ingredient names
            var unmatchedNames = UnmatchedIngredients.Select(u => u.Name).ToList();

            // Open bulk matcher dialog
            var dialog = new BulkIngredientMatcherWindow();
            var viewModel = new BulkIngredientMatcherViewModel(
                unmatchedNames,
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
        if (_mappingService == null)
            return;

        try
        {
            // Re-check all unmatched ingredients against saved mappings
            var itemsToRemove = new List<UnmatchedIngredient>();

            foreach (var unmatchedIngredient in UnmatchedIngredients.ToList())
            {
                // Check if mapping was created
                var mapping = await _mappingService.GetMappingForNameAsync(unmatchedIngredient.Name, _locationId);

                if (mapping != null)
                {
                    // Create suggestion from mapping
                    IngredientMatchSuggestion? suggestion = null;

                    if (mapping.MatchedIngredientId.HasValue)
                    {
                        var ingredient = await _ingredientService.GetIngredientByIdAsync(mapping.MatchedIngredientId.Value);
                        if (ingredient != null)
                        {
                            suggestion = new IngredientMatchSuggestion
                            {
                                IngredientId = ingredient.Id,
                                IngredientName = ingredient.Name,
                                Confidence = 100,
                                Reason = "Saved Mapping"
                            };
                        }
                    }
                    else if (mapping.MatchedRecipeId.HasValue)
                    {
                        var recipe = await _recipeService.GetRecipeByIdAsync(mapping.MatchedRecipeId.Value);
                        if (recipe != null)
                        {
                            // Map to recipe as an ingredient (using recipe as sub-recipe)
                            // For now, we'll skip recipes in recipe imports
                            // In the future, could support sub-recipes
                            continue;
                        }
                    }

                    if (suggestion != null)
                    {
                        // Apply the mapping
                        MapIngredientInternal(unmatchedIngredient, suggestion);
                        itemsToRemove.Add(unmatchedIngredient);
                    }
                }
            }

            // Items are already removed by MapIngredientInternal, just update counts
            OnPropertyChanged(nameof(ImportSummary));

            if (UnmatchedCount == 0)
            {
                ParseStatus = $"✓ All ingredients mapped - ready to import";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing matches: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (!RecipePreviews.Any() || _importResult == null)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Errors.Clear();

            int successCount = 0;
            var importErrors = new List<string>();

            // Import each valid recipe
            foreach (var preview in RecipePreviews.Where(p => p.IsValid))
            {
                try
                {
                    // Create Recipe object
                    var recipe = new Recipe
                    {
                        Id = Guid.NewGuid(),
                        Name = preview.Name,
                        Description = preview.Description,
                        Category = preview.Category,
                        Instructions = preview.Instructions,
                        Yield = preview.Yield,
                        YieldUnit = preview.YieldUnit,
                        PrepTimeMinutes = preview.PrepTimeMinutes,
                        Tags = preview.Tags,
                        Notes = preview.Notes,
                        LocationId = _locationId,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    };

                    // Add ingredients
                    var recipeIngredients = new List<RecipeIngredient>();
                    int sortOrder = 0;

                    foreach (var ingredientPreview in preview.Ingredients)
                    {
                        RecipeIngredient recipeIngredient;

                        if (ingredientPreview.IsMatched)
                        {
                            // Matched ingredient - create with IngredientId and add alias
                            recipeIngredient = new RecipeIngredient
                            {
                                Id = Guid.NewGuid(),
                                RecipeId = recipe.Id,
                                IngredientId = ingredientPreview.MatchedIngredientId!.Value,
                                UnmatchedIngredientName = null, // Matched, so no unmatched name
                                Quantity = ingredientPreview.Quantity,
                                Unit = ingredientPreview.Unit,
                                DisplayText = ingredientPreview.DisplayText,
                                IsOptional = ingredientPreview.IsOptional,
                                SortOrder = sortOrder++,
                                CreatedAt = DateTime.UtcNow,
                                ModifiedAt = DateTime.UtcNow
                            };

                            // Add alias to ingredient if the original name differs from matched name
                            try
                            {
                                var originalName = ingredientPreview.IngredientName?.Trim();
                                var matchedName = ingredientPreview.MatchedIngredientName?.Trim();

                                if (!string.IsNullOrWhiteSpace(originalName) &&
                                    !string.IsNullOrWhiteSpace(matchedName) &&
                                    !originalName.Equals(matchedName, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Load the ingredient to add alias
                                    var ingredient = await _ingredientService.GetIngredientByIdAsync(ingredientPreview.MatchedIngredientId!.Value);
                                    if (ingredient != null)
                                    {
                                        // Check if alias already exists
                                        var aliasExists = ingredient.Aliases?.Any(a =>
                                            a.AliasName.Equals(originalName, StringComparison.OrdinalIgnoreCase)) ?? false;

                                        if (!aliasExists)
                                        {
                                            // Add new alias
                                            ingredient.Aliases ??= new List<IngredientAlias>();
                                            ingredient.Aliases.Add(new IngredientAlias
                                            {
                                                Id = Guid.NewGuid(),
                                                AliasName = originalName,
                                                IsPrimary = false,
                                                IngredientId = ingredient.Id,
                                                CreatedAt = DateTime.UtcNow
                                            });

                                            // Update ingredient to save alias
                                            await _ingredientService.UpdateIngredientAsync(ingredient);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Don't fail the whole import if alias addition fails
                                // Just log and continue
                            }
                        }
                        else
                        {
                            // Unmatched ingredient - create placeholder ingredient
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

                            recipeIngredient = new RecipeIngredient
                            {
                                Id = Guid.NewGuid(),
                                RecipeId = recipe.Id,
                                IngredientId = placeholderIngredient.Id,
                                UnmatchedIngredientName = null, // Now matched to placeholder
                                Quantity = ingredientPreview.Quantity,
                                Unit = ingredientPreview.Unit,
                                DisplayText = ingredientPreview.DisplayText,
                                IsOptional = ingredientPreview.IsOptional,
                                SortOrder = sortOrder++,
                                CreatedAt = DateTime.UtcNow,
                                ModifiedAt = DateTime.UtcNow
                            };
                        }

                        recipeIngredients.Add(recipeIngredient);
                    }

                    recipe.RecipeIngredients = recipeIngredients;

                    // Create recipe in database
                    await _recipeService.CreateRecipeAsync(recipe);
                    successCount++;
                }
                catch (Exception ex)
                {
                    importErrors.Add($"Failed to import '{preview.Name}': {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                ParseStatus = $"✓ Successfully imported {successCount} recipe(s)";

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
                    ErrorMessage = "No recipes were imported. Please verify the file contains valid recipe data.";
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
