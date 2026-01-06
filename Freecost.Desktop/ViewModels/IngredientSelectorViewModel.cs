using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class IngredientSelectorViewModel : ViewModelBase
{
    private readonly List<Ingredient> _allIngredients;
    private readonly Action<Ingredient> _onSelect;
    private readonly Action _onCancel;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<Ingredient> _filteredIngredients = new();

    [ObservableProperty]
    private Ingredient? _selectedIngredient;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _filteredCount;

    public IngredientSelectorViewModel(
        List<Ingredient> allIngredients,
        Action<Ingredient> onSelect,
        Action onCancel)
    {
        _allIngredients = allIngredients;
        _onSelect = onSelect;
        _onCancel = onCancel;

        TotalCount = _allIngredients.Count;

        // Initially show all ingredients
        FilteredIngredients = new ObservableCollection<Ingredient>(_allIngredients);
        FilteredCount = FilteredIngredients.Count;
    }

    partial void OnSearchTextChanged(string? value)
    {
        FilterIngredients();
    }

    private void FilterIngredients()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredIngredients = new ObservableCollection<Ingredient>(_allIngredients);
        }
        else
        {
            var searchLower = SearchText.ToLowerInvariant();
            var filtered = _allIngredients
                .Where(i => i.Name.ToLowerInvariant().Contains(searchLower))
                .ToList();

            FilteredIngredients = new ObservableCollection<Ingredient>(filtered);
        }

        FilteredCount = FilteredIngredients.Count;
    }

    [RelayCommand]
    private void Select()
    {
        if (SelectedIngredient != null)
        {
            _onSelect(SelectedIngredient);
        }
    }

    [RelayCommand]
    private void SelectIngredient(Ingredient ingredient)
    {
        if (ingredient != null)
        {
            _onSelect(ingredient);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }
}
