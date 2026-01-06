using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class EditMappingDialogViewModel : ViewModelBase
{
    private readonly IIngredientMatchMappingService _mappingService;
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly Guid _locationId;
    private readonly Action _onClose;
    private readonly Action? _onMappingChanged;

    private IngredientMatchMapping? _currentMapping;

    [ObservableProperty] private string _importName = string.Empty;
    [ObservableProperty] private string _currentMappingType = string.Empty;
    [ObservableProperty] private string _currentMappingName = string.Empty;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<MatchableItem> _availableItems = new();
    [ObservableProperty] private ObservableCollection<MatchableItem> _filteredAvailableItems = new();
    [ObservableProperty] private MatchableItem? _selectedItem;

    public EditMappingDialogViewModel(
        Guid mappingId,
        IIngredientMatchMappingService mappingService,
        IIngredientService ingredientService,
        IRecipeService recipeService,
        Guid locationId,
        Action onClose,
        Action? onMappingChanged = null)
    {
        _mappingService = mappingService;
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _locationId = locationId;
        _onClose = onClose;
        _onMappingChanged = onMappingChanged;

        _ = LoadMappingAsync(mappingId);
    }

    private async Task LoadMappingAsync(Guid mappingId)
    {
        try
        {
            IsLoading = true;

            // Load the current mapping
            var allMappings = await _mappingService.GetAllMappingsForLocationAsync(_locationId);
            _currentMapping = allMappings.FirstOrDefault(m => m.Id == mappingId);

            if (_currentMapping == null)
            {
                ImportName = "Mapping not found";
                CurrentMappingType = "Error";
                CurrentMappingName = "Unknown";
                return;
            }

            ImportName = _currentMapping.ImportName;

            // Determine current mapping type and name
            if (_currentMapping.MatchedIngredientId.HasValue)
            {
                var ingredient = await _ingredientService.GetIngredientByIdAsync(_currentMapping.MatchedIngredientId.Value);
                CurrentMappingType = "Ingredient";
                CurrentMappingName = ingredient?.Name ?? "Unknown Ingredient";
            }
            else if (_currentMapping.MatchedRecipeId.HasValue)
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(_currentMapping.MatchedRecipeId.Value);
                CurrentMappingType = "Recipe (Sub-recipe)";
                CurrentMappingName = recipe?.Name ?? "Unknown Recipe";
            }
            else
            {
                CurrentMappingType = "None";
                CurrentMappingName = "No mapping";
            }

            // Load available ingredients and recipes
            var ingredients = await _ingredientService.GetAllIngredientsAsync(_locationId);
            var recipes = await _recipeService.GetAllRecipesAsync(_locationId);

            foreach (var ingredient in ingredients)
            {
                AvailableItems.Add(new MatchableItem
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name,
                    Type = MatchableItemType.Ingredient,
                    TypeLabel = "Ingredient"
                });
            }

            foreach (var recipe in recipes)
            {
                AvailableItems.Add(new MatchableItem
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    Type = MatchableItemType.Recipe,
                    TypeLabel = "Recipe (Sub-recipe)"
                });
            }

            FilterItems();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading mapping: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterItems();
    }

    private void FilterItems()
    {
        FilteredAvailableItems.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? AvailableItems
            : AvailableItems.Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var item in filtered.OrderBy(i => i.Name))
        {
            FilteredAvailableItems.Add(item);
        }
    }

    [RelayCommand]
    private async Task ChangeMapping()
    {
        if (_currentMapping == null || SelectedItem == null)
            return;

        try
        {
            if (SelectedItem.Type == MatchableItemType.Ingredient)
            {
                await _mappingService.UpdateToIngredientAsync(_currentMapping.Id, SelectedItem.Id);
            }
            else
            {
                await _mappingService.UpdateToRecipeAsync(_currentMapping.Id, SelectedItem.Id);
            }

            _onMappingChanged?.Invoke();
            _onClose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error changing mapping: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteMapping()
    {
        if (_currentMapping == null)
            return;

        try
        {
            await _mappingService.DeleteMappingAsync(_currentMapping.Id);
            _onMappingChanged?.Invoke();
            _onClose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting mapping: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onClose();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }
}
