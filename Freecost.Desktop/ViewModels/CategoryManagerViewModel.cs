using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

public partial class CategoryManagerViewModel : ViewModelBase
{
    private readonly IIngredientService _ingredientService;
    private readonly IRecipeService _recipeService;
    private readonly IEntreeService _entreeService;
    private readonly Guid _currentLocationId;

    [ObservableProperty]
    private ObservableCollection<CategoryItem> _ingredientCategories = new();

    [ObservableProperty]
    private ObservableCollection<CategoryItem> _recipeCategories = new();

    [ObservableProperty]
    private ObservableCollection<CategoryItem> _entreeCategories = new();

    [ObservableProperty]
    private string _newIngredientCategory = string.Empty;

    [ObservableProperty]
    private string _newRecipeCategory = string.Empty;

    [ObservableProperty]
    private string _newEntreeCategory = string.Empty;

    [ObservableProperty]
    private CategoryItem? _selectedIngredientCategory;

    [ObservableProperty]
    private CategoryItem? _selectedRecipeCategory;

    [ObservableProperty]
    private CategoryItem? _selectedEntreeCategory;

    public CategoryManagerViewModel(
        IIngredientService ingredientService,
        IRecipeService recipeService,
        IEntreeService entreeService)
    {
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
        _currentLocationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationId);
        var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationId);
        var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationId);

        var ingredientCats = ingredients
            .Where(i => !string.IsNullOrWhiteSpace(i.Category))
            .GroupBy(i => i.Category)
            .Select(g => new CategoryItem
            {
                Name = g.Key!,
                Count = g.Count()
            })
            .OrderBy(c => c.Name);

        var recipeCats = recipes
            .Where(r => !string.IsNullOrWhiteSpace(r.Category))
            .GroupBy(r => r.Category)
            .Select(g => new CategoryItem
            {
                Name = g.Key!,
                Count = g.Count()
            })
            .OrderBy(c => c.Name);

        var entreeCats = entrees
            .Where(e => !string.IsNullOrWhiteSpace(e.Category))
            .GroupBy(e => e.Category)
            .Select(g => new CategoryItem
            {
                Name = g.Key!,
                Count = g.Count()
            })
            .OrderBy(c => c.Name);

        IngredientCategories.Clear();
        foreach (var cat in ingredientCats)
            IngredientCategories.Add(cat);

        RecipeCategories.Clear();
        foreach (var cat in recipeCats)
            RecipeCategories.Add(cat);

        EntreeCategories.Clear();
        foreach (var cat in entreeCats)
            EntreeCategories.Add(cat);
    }

    [RelayCommand]
    private void AddIngredientCategory()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientCategory)) return;

        if (!IngredientCategories.Any(c => c.Name.Equals(NewIngredientCategory, StringComparison.OrdinalIgnoreCase)))
        {
            IngredientCategories.Add(new CategoryItem { Name = NewIngredientCategory, Count = 0 });
        }

        NewIngredientCategory = string.Empty;
    }

    [RelayCommand]
    private void AddRecipeCategory()
    {
        if (string.IsNullOrWhiteSpace(NewRecipeCategory)) return;

        if (!RecipeCategories.Any(c => c.Name.Equals(NewRecipeCategory, StringComparison.OrdinalIgnoreCase)))
        {
            RecipeCategories.Add(new CategoryItem { Name = NewRecipeCategory, Count = 0 });
        }

        NewRecipeCategory = string.Empty;
    }

    [RelayCommand]
    private void AddEntreeCategory()
    {
        if (string.IsNullOrWhiteSpace(NewEntreeCategory)) return;

        if (!EntreeCategories.Any(c => c.Name.Equals(NewEntreeCategory, StringComparison.OrdinalIgnoreCase)))
        {
            EntreeCategories.Add(new CategoryItem { Name = NewEntreeCategory, Count = 0 });
        }

        NewEntreeCategory = string.Empty;
    }

    [RelayCommand]
    private async Task RenameIngredientCategory()
    {
        if (SelectedIngredientCategory == null) return;
        // This would need to update all ingredients with this category
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task RenameRecipeCategory()
    {
        if (SelectedRecipeCategory == null) return;
        // This would need to update all recipes with this category
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task RenameEntreeCategory()
    {
        if (SelectedEntreeCategory == null) return;
        // This would need to update all entrees with this category
        await LoadCategoriesAsync();
    }
}

public partial class CategoryItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _count;
}
