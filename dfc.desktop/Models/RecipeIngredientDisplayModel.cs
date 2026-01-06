using CommunityToolkit.Mvvm.ComponentModel;
using Dfc.Core.Models;
using System;

namespace Dfc.Desktop.Models;

public partial class RecipeIngredientDisplayModel : ObservableObject
{
    private readonly RecipeIngredient _recipeIngredient;

    public RecipeIngredientDisplayModel(RecipeIngredient recipeIngredient)
    {
        _recipeIngredient = recipeIngredient;
    }

    public RecipeIngredient Model => _recipeIngredient;

    // Display properties
    public string IngredientName => _recipeIngredient.IngredientDisplayName;

    public string QuantityDisplay => $"{_recipeIngredient.Quantity:F2} {_recipeIngredient.Unit}";

    public string DisplayText => _recipeIngredient.DisplayText ??
                                 $"{_recipeIngredient.Quantity:F2} {_recipeIngredient.Unit}";

    public string Cost => _recipeIngredient.CalculatedCost > 0
        ? $"${_recipeIngredient.CalculatedCost:F2}"
        : "$0.00";

    public Guid Id => _recipeIngredient.Id;

    // Warning indicators for missing ingredients
    public bool IsMissingIngredient => _recipeIngredient.IsMissingIngredient;

    public string? WarningMessage => _recipeIngredient.CostWarningMessage;

    public bool HasWarning => !string.IsNullOrWhiteSpace(WarningMessage);

    // Mapping indicator
    [ObservableProperty]
    private bool _hasMapping;

    [ObservableProperty]
    private Guid? _mappingId;
}