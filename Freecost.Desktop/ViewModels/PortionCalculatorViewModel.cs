using CommunityToolkit.Mvvm.ComponentModel;
using Freecost.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Freecost.Desktop.ViewModels;

public partial class PortionCalculatorViewModel : ViewModelBase
{
    private readonly Recipe _recipe;

    [ObservableProperty]
    private string _recipeName = string.Empty;

    [ObservableProperty]
    private decimal _originalYield;

    [ObservableProperty]
    private string _yieldUnit = string.Empty;

    [ObservableProperty]
    private string _desiredYield = string.Empty;

    [ObservableProperty]
    private decimal _scalingFactor;

    [ObservableProperty]
    private ObservableCollection<IngredientScalingItem> _ingredients = new();

    public bool HasScalingFactor => ScalingFactor > 0;
    public bool HasIngredients => Ingredients.Count > 0;

    public PortionCalculatorViewModel(Recipe recipe)
    {
        _recipe = recipe;
        RecipeName = recipe.Name;
        OriginalYield = recipe.Yield;
        YieldUnit = recipe.YieldUnit;

        // Load ingredients
        if (recipe.RecipeIngredients != null)
        {
            foreach (var ri in recipe.RecipeIngredients.Where(ri => ri.Ingredient != null))
            {
                Ingredients.Add(new IngredientScalingItem
                {
                    Name = ri.Ingredient!.Name,
                    OriginalQuantity = ri.Quantity,
                    NewQuantity = ri.Quantity,
                    Unit = ri.Unit.ToString()
                });
            }
        }
    }

    partial void OnDesiredYieldChanged(string value)
    {
        if (decimal.TryParse(value, out var desired) && desired > 0 && OriginalYield > 0)
        {
            ScalingFactor = desired / OriginalYield;

            // Update all ingredient quantities
            foreach (var ingredient in Ingredients)
            {
                ingredient.NewQuantity = ingredient.OriginalQuantity * ScalingFactor;
            }

            OnPropertyChanged(nameof(HasScalingFactor));
        }
        else
        {
            ScalingFactor = 0;
            OnPropertyChanged(nameof(HasScalingFactor));
        }
    }
}

public partial class IngredientScalingItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _originalQuantity;

    [ObservableProperty]
    private decimal _newQuantity;

    [ObservableProperty]
    private string _unit = string.Empty;
}
