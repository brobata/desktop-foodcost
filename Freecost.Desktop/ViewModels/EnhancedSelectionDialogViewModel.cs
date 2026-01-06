using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Enums;
using Freecost.Core.Helpers;
using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class EnhancedSelectionDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private List<SelectionItem> _items;

    [ObservableProperty]
    private SelectionItem? _selectedItem;

    [ObservableProperty]
    private string _quantity = "1";

    [ObservableProperty]
    private UnitType _selectedUnit;

    [ObservableProperty]
    private List<UnitType> _units;

    private readonly UnitType _defaultUnit;

    public EnhancedSelectionDialogViewModel(List<SelectionItem> items, string title, UnitType defaultUnit = UnitType.Each)
    {
        _items = items;
        _title = title;
        _defaultUnit = defaultUnit;

        // CRITICAL: Initialize BEFORE setting selected unit
        _units = Enum.GetValues<UnitType>().ToList();

        // CRITICAL: Set the selected unit to the default
        _selectedUnit = defaultUnit;

        System.Diagnostics.Debug.WriteLine($"EnhancedSelectionDialogViewModel initialized - DefaultUnit: {defaultUnit}, SelectedUnit: {_selectedUnit}");
    }

    partial void OnSelectedItemChanged(SelectionItem? value)
    {
        if (value?.Data is Ingredient ingredient)
        {
            // For ingredients: Show units based on ingredient configuration
            if (ingredient.UseAlternateUnit && ingredient.AlternateUnit.HasValue)
            {
                // Get all units from the alternate unit's category (e.g., all volume units)
                var alternateCategory = UnitConverter.GetUnitsInSameCategory(ingredient.AlternateUnit.Value);

                // Get all units from the purchase unit's category (e.g., all weight units)
                var purchaseCategory = UnitConverter.GetUnitsInSameCategory(ingredient.Unit);

                // Combine both categories - user can use ANY volume or weight unit!
                var newUnits = alternateCategory.Union(purchaseCategory).Distinct().ToList();

                // CRITICAL: Update Units first, THEN SelectedUnit to avoid null binding
                var unitsToSet = newUnits.Any() ? newUnits : Enum.GetValues<UnitType>().ToList();
                Units = unitsToSet;
                SelectedUnit = unitsToSet.Contains(ingredient.AlternateUnit.Value)
                    ? ingredient.AlternateUnit.Value
                    : unitsToSet.First();
            }
            else
            {
                // Show units in same category as ingredient's purchase unit
                var categoryUnits = UnitConverter.GetUnitsInSameCategory(ingredient.Unit);

                if (categoryUnits != null && categoryUnits.Any())
                {
                    // CRITICAL: Update Units first, THEN SelectedUnit to avoid null binding
                    Units = categoryUnits;
                    SelectedUnit = categoryUnits.Contains(ingredient.Unit) ? ingredient.Unit : categoryUnits.First();
                }
                else
                {
                    // CRITICAL: Update Units first, THEN SelectedUnit to avoid null binding
                    var allUnits = Enum.GetValues<UnitType>().ToList();
                    Units = allUnits;
                    SelectedUnit = allUnits.Contains(_defaultUnit) ? _defaultUnit : allUnits.First();
                }
            }
        }
        else if (value?.Data is Recipe)
        {
            // For recipes: ALWAYS show ALL units, don't filter!
            // Recipes can use any unit (portions, cups, ounces, etc.)
            // DO NOT call OnSelectedItemChanged again or filter the list

            System.Diagnostics.Debug.WriteLine($"Recipe selected - keeping all units available");
        }
        else
        {
            // No selection - show all units
            // Only set if we don't already have all units
            if (Units == null || Units.Count != Enum.GetValues<UnitType>().Length)
            {
                var allUnits = Enum.GetValues<UnitType>().ToList();
                Units = allUnits;
                SelectedUnit = allUnits.Contains(_defaultUnit) ? _defaultUnit : allUnits.First();
            }
        }
    }

    public bool IsValid()
    {
        return SelectedItem != null && decimal.TryParse(Quantity, out var qty) && qty > 0;
    }

    public decimal GetQuantity()
    {
        return decimal.TryParse(Quantity, out var qty) ? qty : 0;
    }
}