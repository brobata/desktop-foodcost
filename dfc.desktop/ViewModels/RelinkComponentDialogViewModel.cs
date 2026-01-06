using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Enums;
using Dfc.Core.Helpers;
using Dfc.Core.Models;
using Dfc.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dfc.Desktop.ViewModels;

public partial class RelinkComponentDialogViewModel : ObservableObject
{
    private readonly RelinkComponentDialog _dialog;
    private readonly List<SelectionItem> _allIngredients;
    private readonly List<SelectionItem> _allRecipes;
    private readonly UnitType _currentUnit;
    private readonly decimal _currentQuantity;

    [ObservableProperty]
    private string _componentName;

    [ObservableProperty]
    private bool _isIngredientSelected = true;

    [ObservableProperty]
    private bool _isRecipeSelected = false;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SelectionItem> _filteredItems = new();

    [ObservableProperty]
    private SelectionItem? _selectedItem;

    [ObservableProperty]
    private string _quantity = "1";

    [ObservableProperty]
    private UnitType _selectedUnit;

    [ObservableProperty]
    private List<UnitType> _units;

    public RelinkComponentDialogViewModel(
        RelinkComponentDialog dialog,
        string componentName,
        List<SelectionItem> ingredients,
        List<SelectionItem> recipes,
        bool isCurrentlyIngredient,
        decimal currentQuantity,
        UnitType currentUnit)
    {
        _dialog = dialog;
        _componentName = componentName;
        _allIngredients = ingredients;
        _allRecipes = recipes;
        _currentQuantity = currentQuantity;
        _currentUnit = currentUnit;

        // Initialize
        _isIngredientSelected = isCurrentlyIngredient;
        _isRecipeSelected = !isCurrentlyIngredient;
        _quantity = currentQuantity.ToString("F2");
        _units = Enum.GetValues<UnitType>().ToList();
        _selectedUnit = currentUnit;

        // Load initial items
        RefreshFilteredItems();
    }

    partial void OnIsIngredientSelectedChanged(bool value)
    {
        if (value)
        {
            IsRecipeSelected = false;
            RefreshFilteredItems();
        }
    }

    partial void OnIsRecipeSelectedChanged(bool value)
    {
        if (value)
        {
            IsIngredientSelected = false;
            RefreshFilteredItems();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        RefreshFilteredItems();
    }

    partial void OnSelectedItemChanged(SelectionItem? value)
    {
        if (value?.Data is Ingredient ingredient)
        {
            // For ingredients: Show units based on ingredient configuration
            if (ingredient.UseAlternateUnit && ingredient.AlternateUnit.HasValue)
            {
                var alternateCategory = UnitConverter.GetUnitsInSameCategory(ingredient.AlternateUnit.Value);
                var purchaseCategory = UnitConverter.GetUnitsInSameCategory(ingredient.Unit);
                var newUnits = alternateCategory.Union(purchaseCategory).Distinct().ToList();

                var unitsToSet = newUnits.Any() ? newUnits : Enum.GetValues<UnitType>().ToList();
                Units = unitsToSet;
                SelectedUnit = unitsToSet.Contains(ingredient.AlternateUnit.Value)
                    ? ingredient.AlternateUnit.Value
                    : unitsToSet.First();
            }
            else
            {
                var categoryUnits = UnitConverter.GetUnitsInSameCategory(ingredient.Unit);

                if (categoryUnits != null && categoryUnits.Any())
                {
                    Units = categoryUnits;
                    SelectedUnit = categoryUnits.Contains(ingredient.Unit) ? ingredient.Unit : categoryUnits.First();
                }
                else
                {
                    var allUnits = Enum.GetValues<UnitType>().ToList();
                    Units = allUnits;
                    SelectedUnit = allUnits.Contains(_currentUnit) ? _currentUnit : allUnits.First();
                }
            }
        }
        else if (value?.Data is Recipe)
        {
            // For recipes: Show all units
            var allUnits = Enum.GetValues<UnitType>().ToList();
            Units = allUnits;
            // Keep current unit if possible
            SelectedUnit = allUnits.Contains(_currentUnit) ? _currentUnit : UnitType.Each;
        }
    }

    private void RefreshFilteredItems()
    {
        var source = IsIngredientSelected ? _allIngredients : _allRecipes;

        IEnumerable<SelectionItem> filtered = source;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = source.Where(item =>
                item.Name.ToLowerInvariant().Contains(searchLower));
        }

        FilteredItems = new ObservableCollection<SelectionItem>(filtered.OrderBy(i => i.Name));
    }

    [RelayCommand]
    private void Relink()
    {
        if (SelectedItem == null || !decimal.TryParse(Quantity, out var qty) || qty <= 0)
            return;

        var result = new RelinkComponentResult
        {
            SelectedItem = SelectedItem,
            Quantity = qty,
            Unit = SelectedUnit,
            IsIngredient = IsIngredientSelected
        };

        _dialog.SetResult(result);
    }

    [RelayCommand]
    private void Cancel()
    {
        _dialog.SetResult(null);
    }
}
