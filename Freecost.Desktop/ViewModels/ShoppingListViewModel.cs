// Location: Freecost.Desktop/ViewModels/ShoppingListViewModel.cs
// Action: CREATE - Shopping list generator from selected recipes

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

public partial class ShoppingListViewModel : ViewModelBase
{
    private readonly ILogger<ShoppingListViewModel>? _logger;
    private readonly IRecipeService _recipeService;
    private readonly IIngredientService _ingredientService;
    private readonly Guid _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly Action _onClose;

    public ShoppingListViewModel(
        IRecipeService recipeService,
        IIngredientService ingredientService,
        List<Guid> selectedRecipeIds,
        Action onClose,
        ILogger<ShoppingListViewModel>? logger = null)
    {
        _logger = logger;
        _recipeService = recipeService;
        _ingredientService = ingredientService;
        _onClose = onClose;
        ShoppingListItems = new ObservableCollection<ShoppingListItemModel>();
        SelectedRecipeIds = selectedRecipeIds;
    }

    [ObservableProperty]
    private ObservableCollection<ShoppingListItemModel> _shoppingListItems;

    [ObservableProperty]
    private List<Guid> _selectedRecipeIds = new();

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private string _recipeNames = string.Empty;

    [ObservableProperty]
    private bool _isLoading = true;

    public async Task LoadShoppingListAsync()
    {
        try
        {
            IsLoading = true;
            ShoppingListItems.Clear();

            // Load all selected recipes with their ingredients
            var recipes = new List<Recipe>();
            foreach (var recipeId in SelectedRecipeIds)
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
                if (recipe != null)
                {
                    recipes.Add(recipe);
                }
            }

            // Set recipe names for display
            RecipeNames = string.Join(", ", recipes.Select(r => r.Name));

            // Aggregate ingredients across all recipes
            var aggregatedIngredients = new Dictionary<Guid, ShoppingListItemModel>();

            foreach (var recipe in recipes)
            {
                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    // Skip ingredients that aren't matched to database yet
                    if (!recipeIngredient.IngredientId.HasValue)
                        continue;

                    var ingredientId = recipeIngredient.IngredientId.Value;

                    if (aggregatedIngredients.ContainsKey(ingredientId))
                    {
                        // Add to existing ingredient
                        var existing = aggregatedIngredients[ingredientId];
                        existing.Quantity += recipeIngredient.Quantity;
                        existing.RecipeNames.Add(recipe.Name);
                        existing.TotalCost = existing.Quantity * existing.UnitCost;
                    }
                    else
                    {
                        // Create new shopping list item
                        var ingredient = await _ingredientService.GetIngredientByIdAsync(ingredientId);
                        if (ingredient != null)
                        {
                            aggregatedIngredients[ingredientId] = new ShoppingListItemModel
                            {
                                IngredientId = ingredientId,
                                IngredientName = ingredient.Name,
                                Quantity = recipeIngredient.Quantity,
                                Unit = recipeIngredient.Unit.ToString(),
                                UnitCost = ingredient.CurrentPrice,
                                TotalCost = recipeIngredient.Quantity * ingredient.CurrentPrice,
                                Category = ingredient.Category ?? "Uncategorized",
                                VendorName = ingredient.VendorName ?? "Unknown",
                                RecipeNames = new List<string> { recipe.Name }
                            };
                        }
                    }
                }
            }

            // Add items to collection, sorted by category then name
            foreach (var item in aggregatedIngredients.Values.OrderBy(i => i.Category).ThenBy(i => i.IngredientName))
            {
                ShoppingListItems.Add(item);
            }

            // Calculate totals
            TotalItems = ShoppingListItems.Count;
            TotalCost = ShoppingListItems.Sum(i => i.TotalCost);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading shopping list");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void PrintShoppingList()
    {
        // TODO: Implement PDF export for shopping list
    }

    [RelayCommand]
    private void Close()
    {
        _onClose();
    }
}

public partial class ShoppingListItemModel : ObservableObject
{
    [ObservableProperty]
    private Guid _ingredientId;

    [ObservableProperty]
    private string _ingredientName = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private decimal _unitCost;

    [ObservableProperty]
    private decimal _totalCost;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private List<string> _recipeNames = new();

    [ObservableProperty]
    private bool _isChecked;

    public string RecipeNamesDisplay => string.Join(", ", RecipeNames);
}
