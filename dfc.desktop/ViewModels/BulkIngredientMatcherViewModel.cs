using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class BulkIngredientMatcherViewModel : ViewModelBase
{
    private readonly IIngredientMatchMappingService _mappingService;
    private readonly Guid _locationId;
    private readonly Action _onComplete;
    private readonly List<MatchableItem> _allAvailableItems;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<UnmatchedItem> _unmatchedItems = new();

    [ObservableProperty]
    private ObservableCollection<MatchableItem> _filteredAvailableItems = new();

    [ObservableProperty]
    private UnmatchedItem? _selectedUnmatchedItem;

    [ObservableProperty]
    private MatchableItem? _selectedAvailableItem;

    [ObservableProperty]
    private int _unmatchedCount;

    [ObservableProperty]
    private int _matchedCount;

    [ObservableProperty]
    private int _totalAvailableCount;

    [ObservableProperty]
    private int _filteredCount;

    public BulkIngredientMatcherViewModel(
        List<string> importNames,
        List<Ingredient> availableIngredients,
        List<Recipe> availableRecipes,
        Guid locationId,
        IIngredientMatchMappingService mappingService,
        Action onComplete)
    {
        _mappingService = mappingService;
        _locationId = locationId;
        _onComplete = onComplete;

        // Create unmatched items from import names
        UnmatchedItems = new ObservableCollection<UnmatchedItem>(
            importNames.Select(name => new UnmatchedItem { ImportName = name })
        );

        // Combine ingredients and recipes into matchable items
        _allAvailableItems = new List<MatchableItem>();

        foreach (var ingredient in availableIngredients.OrderBy(i => i.Name))
        {
            _allAvailableItems.Add(new MatchableItem
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Type = MatchableItemType.Ingredient,
                TypeLabel = "Ingredient"
            });
        }

        foreach (var recipe in availableRecipes.OrderBy(r => r.Name))
        {
            _allAvailableItems.Add(new MatchableItem
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Type = MatchableItemType.Recipe,
                TypeLabel = "Recipe (Sub-recipe)"
            });
        }

        TotalAvailableCount = _allAvailableItems.Count;
        FilteredAvailableItems = new ObservableCollection<MatchableItem>(_allAvailableItems);
        FilteredCount = FilteredAvailableItems.Count;

        UpdateCounts();
    }

    partial void OnSearchTextChanged(string? value)
    {
        FilterAvailableItems();
    }

    private void FilterAvailableItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredAvailableItems = new ObservableCollection<MatchableItem>(_allAvailableItems);
        }
        else
        {
            var searchLower = SearchText.ToLowerInvariant();
            var filtered = _allAvailableItems
                .Where(i => i.Name.ToLowerInvariant().Contains(searchLower))
                .ToList();

            FilteredAvailableItems = new ObservableCollection<MatchableItem>(filtered);
        }

        FilteredCount = FilteredAvailableItems.Count;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task Match(MatchableItem? availableItem)
    {
        if (SelectedUnmatchedItem == null || availableItem == null)
            return;

        try
        {
            // Save the mapping to database
            if (availableItem.Type == MatchableItemType.Ingredient)
            {
                await _mappingService.SaveIngredientMappingAsync(
                    SelectedUnmatchedItem.ImportName,
                    availableItem.Id,
                    _locationId);
            }
            else // Recipe
            {
                await _mappingService.SaveRecipeMappingAsync(
                    SelectedUnmatchedItem.ImportName,
                    availableItem.Id,
                    _locationId);
            }

            // Mark as matched
            SelectedUnmatchedItem.IsMatched = true;
            SelectedUnmatchedItem.MatchedToName = availableItem.Name;
            SelectedUnmatchedItem.MatchedToType = availableItem.TypeLabel;

            UpdateCounts();

            // Auto-select next unmatched item
            var nextUnmatched = UnmatchedItems.FirstOrDefault(i => !i.IsMatched);
            if (nextUnmatched != null)
            {
                SelectedUnmatchedItem = nextUnmatched;
            }

            // Clear search to show all items for next match
            SearchText = string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving mapping: {ex.Message}");
            // Note: Item won't be marked as matched if save fails, providing visual feedback
            // Detailed error logging helps with debugging rare database issues
        }
    }

    [RelayCommand]
    private void Done()
    {
        _onComplete();
    }

    private void UpdateCounts()
    {
        MatchedCount = UnmatchedItems.Count(i => i.IsMatched);
        UnmatchedCount = UnmatchedItems.Count - MatchedCount;
    }
}

/// <summary>
/// Represents an item from import that needs to be matched
/// </summary>
public partial class UnmatchedItem : ObservableObject
{
    [ObservableProperty]
    private string _importName = string.Empty;

    [ObservableProperty]
    private bool _isMatched;

    [ObservableProperty]
    private string? _matchedToName;

    [ObservableProperty]
    private string? _matchedToType;
}

/// <summary>
/// Represents an ingredient or recipe that can be matched to
/// </summary>
public class MatchableItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MatchableItemType Type { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
}

public enum MatchableItemType
{
    Ingredient,
    Recipe
}
