using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class GlobalSearchViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly ILogger<GlobalSearchViewModel>? _logger;
    private readonly Guid _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SearchResultItem> _searchResults = new();

    [ObservableProperty]
    private SearchResultItem? _selectedResult;

    [ObservableProperty]
    private bool _isSearching = false;

    public GlobalSearchViewModel(
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService,
        ILogger<GlobalSearchViewModel>? logger = null)
    {
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
        _logger = logger;
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = PerformSearchAsync();
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            return;
        }

        try
        {
            IsSearching = true;

            var results = new List<SearchResultItem>();

            // Search ingredients
            var ingredients = await _ingredientService.SearchIngredientsAsync(SearchText, _currentLocationId);
            results.AddRange(ingredients.Take(10).Select(i => new SearchResultItem
            {
                Id = i.Id,
                Name = i.Name,
                Type = "Ingredient",
                Icon = "🥬",
                Category = i.Category ?? "No Category",
                Details = $"{i.CurrentPrice:C2} per {i.Unit}",
                EntityType = SearchResultEntityType.Ingredient,
                Entity = i
            }));

            // Search recipes
            var recipes = await _recipeService.SearchRecipesAsync(SearchText, _currentLocationId);
            results.AddRange(recipes.Take(10).Select(r => new SearchResultItem
            {
                Id = r.Id,
                Name = r.Name,
                Type = "Recipe",
                Icon = "📋",
                Category = r.Category ?? "No Category",
                Details = $"{r.TotalCost:C2} - Yields {r.Yield} {r.YieldUnit}",
                EntityType = SearchResultEntityType.Recipe,
                Entity = r
            }));

            // Search entrees (get all and filter manually)
            var allEntrees = await _entreeService.GetAllEntreesAsync(_currentLocationId);
            var entrees = allEntrees
                .Where(e => e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            results.AddRange(entrees.Take(10).Select(e => new SearchResultItem
            {
                Id = e.Id,
                Name = e.Name,
                Type = "Menu Item",
                Icon = "🍽️",
                Category = "Menu Item",
                Details = $"Food Cost: {e.TotalCost:C2} - Menu Price: {(e.MenuPrice?.ToString("C2") ?? "N/A")}",
                EntityType = SearchResultEntityType.Entree,
                Entity = e
            }));

            // Sort by relevance (exact matches first, then contains)
            var sortedResults = results
                .OrderByDescending(r => r.Name.Equals(SearchText, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(r => r.Name.StartsWith(SearchText, StringComparison.OrdinalIgnoreCase))
                .ThenBy(r => r.Type)
                .ThenBy(r => r.Name)
                .Take(20)
                .ToList();

            SearchResults.Clear();
            foreach (var result in sortedResults)
            {
                SearchResults.Add(result);
            }

            // Auto-select first result
            if (SearchResults.Any())
            {
                SelectedResult = SearchResults[0];
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error performing global search for query: {SearchQuery}", SearchText);
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void SelectResult(SearchResultItem? result)
    {
        SelectedResult = result;
    }
}

public enum SearchResultEntityType
{
    Ingredient,
    Recipe,
    Entree
}

public class SearchResultItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public SearchResultEntityType EntityType { get; set; }
    public object? Entity { get; set; }
}
