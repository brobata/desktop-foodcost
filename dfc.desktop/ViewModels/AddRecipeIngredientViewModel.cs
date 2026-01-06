using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Desktop.ViewModels;

public partial class AddRecipeIngredientViewModel : ViewModelBase
{
    private readonly Action _onSaveSuccess;
    private readonly Action _onCancel;

    [ObservableProperty]
    private List<Ingredient> _availableIngredients;

    [ObservableProperty]
    private Ingredient? _selectedIngredient;

    [ObservableProperty]
    private string _quantity = string.Empty;

    [ObservableProperty]
    private UnitType _selectedUnit = UnitType.Ounce;

    [ObservableProperty]
    private string _displayText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public List<UnitType> AvailableUnits { get; } = Enum.GetValues<UnitType>().ToList();

    public RecipeIngredient? CreatedRecipeIngredient { get; private set; }

    public AddRecipeIngredientViewModel(
        Action onSaveSuccess,
        Action onCancel,
        List<Ingredient> availableIngredients)
    {
        _onSaveSuccess = onSaveSuccess;
        _onCancel = onCancel;
        _availableIngredients = availableIngredients;
    }

    [RelayCommand]
    private void Save()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (SelectedIngredient == null)
        {
            ErrorMessage = "Please select an ingredient";
            return;
        }

        if (string.IsNullOrWhiteSpace(Quantity) || !decimal.TryParse(Quantity, out decimal qty) || qty <= 0)
        {
            ErrorMessage = "Please enter a valid quantity greater than 0";
            return;
        }

        try
        {
            // Create the recipe ingredient
            CreatedRecipeIngredient = new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                IngredientId = SelectedIngredient.Id,
                Ingredient = SelectedIngredient,
                Quantity = qty,
                Unit = SelectedUnit,
                DisplayText = string.IsNullOrWhiteSpace(DisplayText)
                    ? $"{qty:F2} {SelectedUnit}"
                    : DisplayText.Trim()
            };

            _onSaveSuccess();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error adding ingredient: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel();
    }
}