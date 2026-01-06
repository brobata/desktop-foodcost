using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Models;
using Freecost.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Freecost.Desktop.ViewModels;

/// <summary>
/// ViewModel for mapping ingredients to nutritional data from USDA FoodData Central
/// </summary>
public partial class NutritionalDataMapperViewModel : ViewModelBase
{
    private readonly INutritionalDataService _nutritionalDataService;
    private readonly Func<bool, Task> _onComplete;

    [ObservableProperty]
    private ObservableCollection<IngredientMappingItem> _ingredients = new();

    [ObservableProperty]
    private IngredientMappingItem? _selectedIngredient;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _totalIngredients;

    [ObservableProperty]
    private int _mappedIngredients;

    [ObservableProperty]
    private int _unmappedIngredients;

    public string SummaryText =>
        $"{MappedIngredients} of {TotalIngredients} ingredients mapped";

    public NutritionalDataMapperViewModel(
        INutritionalDataService nutritionalDataService,
        Func<bool, Task> onComplete)
    {
        _nutritionalDataService = nutritionalDataService;
        _onComplete = onComplete;
    }

    /// <summary>
    /// Initialize with ingredients to map
    /// </summary>
    public async Task InitializeAsync(ObservableCollection<Ingredient> ingredients)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Initializing nutritional data mapper...";

            // Check if API is available
            var isAvailable = await _nutritionalDataService.IsAvailableAsync();
            if (!isAvailable)
            {
                StatusMessage = "⚠️ USDA API key not configured. Please add your API key to settings.";
                IsLoading = false;
                return;
            }

            // Create mapping items for each ingredient
            Ingredients.Clear();
            foreach (var ingredient in ingredients)
            {
                // Check if ingredient already has nutritional data
                var hasNutritionalData = ingredient.CaloriesPerUnit.HasValue ||
                                        ingredient.ProteinPerUnit.HasValue ||
                                        ingredient.CarbohydratesPerUnit.HasValue ||
                                        ingredient.FatPerUnit.HasValue;

                var mappingItem = new IngredientMappingItem
                {
                    Ingredient = ingredient,
                    IsMapped = hasNutritionalData
                };

                // If already has data, create a "Selected" suggestion showing existing data
                if (hasNutritionalData)
                {
                    mappingItem.SelectedSuggestion = new NutritionalDataResult
                    {
                        Description = $"{ingredient.Name} (Current Data)",
                        FdcId = "existing",
                        MatchScore = 100,
                        NutritionPer100g = new NutritionalInfo
                        {
                            Calories = ingredient.CaloriesPerUnit ?? 0,
                            Protein = ingredient.ProteinPerUnit ?? 0,
                            Carbohydrates = ingredient.CarbohydratesPerUnit ?? 0,
                            Fat = ingredient.FatPerUnit ?? 0,
                            Fiber = ingredient.FiberPerUnit ?? 0,
                            Sugar = ingredient.SugarPerUnit ?? 0,
                            Sodium = ingredient.SodiumPerUnit ?? 0
                        }
                    };
                }

                Ingredients.Add(mappingItem);
            }

            TotalIngredients = Ingredients.Count;
            UpdateCounts();

            // Auto-search one ingredient at a time (progressive loading)
            StatusMessage = "Ready to search for nutritional data. Click 'Search All' or search individual ingredients.";
            IsLoading = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing mapper: {ex.Message}";
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAll()
    {
        IsSearching = true;
        var searchedCount = 0;

        foreach (var item in Ingredients)
        {
            try
            {
                searchedCount++;
                StatusMessage = $"Searching {searchedCount} of {TotalIngredients}: {item.Ingredient.Name}...";

                var results = await _nutritionalDataService.SearchNutritionalDataAsync(item.Ingredient.Name, maxResults: 5);
                item.Suggestions.Clear();
                foreach (var result in results.OrderByDescending(r => r.MatchScore))
                {
                    item.Suggestions.Add(result);
                }

                // Auto-select best match if confidence is very high (95+)
                if (results.Any() && results[0].MatchScore >= 95)
                {
                    item.SelectedSuggestion = results[0];
                    item.IsMapped = true;
                }

                UpdateCounts();

                // Small delay to avoid hitting API rate limits
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching for {item.Ingredient.Name}: {ex.Message}");
            }
        }

        StatusMessage = $"✓ Search complete. Found suggestions for {Ingredients.Count(i => i.Suggestions.Any())} of {TotalIngredients} ingredients.";
        IsSearching = false;
    }

    [RelayCommand]
    private async Task SearchIngredient(IngredientMappingItem item)
    {
        if (item == null) return;

        try
        {
            IsSearching = true;

            // Use custom search term if provided, otherwise use ingredient name
            var searchTerm = string.IsNullOrWhiteSpace(item.CustomSearchTerm)
                ? item.Ingredient.Name
                : item.CustomSearchTerm;

            StatusMessage = $"Searching for '{searchTerm}'...";

            var results = await _nutritionalDataService.SearchNutritionalDataAsync(searchTerm, maxResults: 5);
            item.Suggestions.Clear();
            foreach (var result in results.OrderByDescending(r => r.MatchScore))
            {
                item.Suggestions.Add(result);
            }

            StatusMessage = $"Found {results.Count} matches for '{searchTerm}'";
            IsSearching = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error searching: {ex.Message}";
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void SelectSuggestion(object[] parameters)
    {
        if (parameters.Length != 2) return;
        if (parameters[0] is not IngredientMappingItem item) return;
        if (parameters[1] is not NutritionalDataResult suggestion) return;

        item.SelectedSuggestion = suggestion;
        item.IsMapped = true;

        UpdateCounts();
        OnPropertyChanged(nameof(SummaryText));
    }

    [RelayCommand]
    private void ClearMapping(IngredientMappingItem item)
    {
        if (item == null) return;

        item.SelectedSuggestion = null;
        item.IsMapped = false;

        UpdateCounts();
        OnPropertyChanged(nameof(SummaryText));
    }

    [RelayCommand]
    private async Task ApplyMappings()
    {
        // Apply all mapped nutritional data to ingredients
        foreach (var item in Ingredients.Where(i => i.IsMapped && i.SelectedSuggestion != null))
        {
            ApplyNutritionalData(item.Ingredient, item.SelectedSuggestion!);
        }

        await _onComplete(true);
    }

    [RelayCommand]
    private async Task Skip()
    {
        await _onComplete(false);
    }

    private void ApplyNutritionalData(Ingredient ingredient, NutritionalDataResult data)
    {
        if (data.NutritionPer100g != null)
        {
            // Apply nutritional data per 100g
            // Note: These values are per 100g from USDA data
            // The actual per-unit values will depend on the ingredient's Unit property
            ingredient.CaloriesPerUnit = data.NutritionPer100g.Calories;
            ingredient.ProteinPerUnit = data.NutritionPer100g.Protein;
            ingredient.CarbohydratesPerUnit = data.NutritionPer100g.Carbohydrates;
            ingredient.FatPerUnit = data.NutritionPer100g.Fat;
            ingredient.FiberPerUnit = data.NutritionPer100g.Fiber;
            ingredient.SugarPerUnit = data.NutritionPer100g.Sugar;
            ingredient.SodiumPerUnit = data.NutritionPer100g.Sodium;
        }

        // Automatically apply alternate unit conversion from USDA serving sizes
        if (data.ServingSizes.Any())
        {
            var bestServing = SelectBestServingSize(data.ServingSizes, ingredient.Name);
            if (bestServing != null)
            {
                var (alternateUnit, conversionQuantity) = ParseServingSizeToConversion(bestServing);

                if (alternateUnit.HasValue)
                {
                    ingredient.UseAlternateUnit = true;
                    ingredient.AlternateUnit = alternateUnit.Value;
                    ingredient.AlternateConversionQuantity = conversionQuantity;
                    ingredient.AlternateConversionUnit = Core.Enums.UnitType.Gram;

                    System.Diagnostics.Debug.WriteLine($"Auto-applied conversion for {ingredient.Name}: 1 {alternateUnit} = {conversionQuantity}g");
                }
            }
        }

        // Apply detected allergens from USDA data
        if (data.DetectedAllergens != null && data.DetectedAllergens.Any())
        {
            // Initialize allergens list if null
            ingredient.IngredientAllergens ??= new List<IngredientAllergen>();

            // Add detected allergens that aren't already present
            foreach (var allergenType in data.DetectedAllergens)
            {
                var allergenId = Freecost.Core.Helpers.AllergenMapper.GetAllergenId(allergenType);

                // Check if allergen already exists for this ingredient
                var existingAllergen = ingredient.IngredientAllergens.FirstOrDefault(ia => ia.AllergenId == allergenId);
                if (existingAllergen == null)
                {
                    var newAllergen = new IngredientAllergen
                    {
                        Id = Guid.NewGuid(),
                        IngredientId = ingredient.Id,
                        AllergenId = allergenId,
                        IsAutoDetected = true,
                        IsEnabled = true,
                        SourceIngredients = "USDA FoodData Central"
                    };
                    ingredient.IngredientAllergens.Add(newAllergen);

                    System.Diagnostics.Debug.WriteLine($"Auto-applied allergen for {ingredient.Name}: {allergenType}");
                }
            }
        }
    }

    /// <summary>
    /// Selects the best serving size based on ingredient type and common usage patterns
    /// </summary>
    private ServingSize? SelectBestServingSize(List<ServingSize> servingSizes, string ingredientName)
    {
        if (!servingSizes.Any()) return null;

        var nameLower = ingredientName.ToLowerInvariant();

        // Priority 1: Prefer "each", "medium", "whole" for countable items (vegetables, fruits)
        var countableKeywords = new[] { "medium", "whole", "pepper", "piece", "item", "each" };
        var countableServing = servingSizes.FirstOrDefault(s =>
            countableKeywords.Any(k => s.Description.ToLowerInvariant().Contains(k)));
        if (countableServing != null) return countableServing;

        // Priority 2: Prefer "cup" for volume ingredients (mayo, sauces, liquids)
        var cupServing = servingSizes.FirstOrDefault(s =>
            s.Description.ToLowerInvariant().Contains("cup") &&
            !s.Description.ToLowerInvariant().Contains("chopped"));
        if (cupServing != null && IsLikelyVolumeIngredient(nameLower)) return cupServing;

        // Priority 3: Prefer "tablespoon" for condiments and spices
        var tbspServing = servingSizes.FirstOrDefault(s =>
            s.Description.ToLowerInvariant().Contains("tablespoon"));
        if (tbspServing != null && IsLikelyCondiment(nameLower)) return tbspServing;

        // Priority 4: Use preferred serving if marked
        var preferred = servingSizes.FirstOrDefault(s => s.IsPreferred);
        if (preferred != null) return preferred;

        // Priority 5: Use first available
        return servingSizes.First();
    }

    /// <summary>
    /// Parses serving size description to determine alternate unit and conversion quantity
    /// </summary>
    private (Core.Enums.UnitType? unit, decimal grams) ParseServingSizeToConversion(ServingSize serving)
    {
        var desc = serving.Description.ToLowerInvariant();
        var grams = serving.Grams;

        // Parse common serving size patterns
        if (desc.Contains("cup"))
            return (Core.Enums.UnitType.Cup, grams);
        else if (desc.Contains("tablespoon") || desc.Contains("tbsp"))
            return (Core.Enums.UnitType.Tablespoon, grams);
        else if (desc.Contains("teaspoon") || desc.Contains("tsp"))
            return (Core.Enums.UnitType.Teaspoon, grams);
        else if (desc.Contains("ounce") || desc.Contains("oz"))
            return (Core.Enums.UnitType.Ounce, grams);
        else if (desc.Contains("medium") || desc.Contains("whole") || desc.Contains("piece") || desc.Contains("pepper") || desc.Contains("each"))
            return (Core.Enums.UnitType.Each, grams);
        else if (desc.Contains("pound") || desc.Contains("lb"))
            return (Core.Enums.UnitType.Pound, grams);

        // Default to "each" for countable items
        return (Core.Enums.UnitType.Each, grams);
    }

    private bool IsLikelyVolumeIngredient(string name)
    {
        var volumeKeywords = new[] { "mayo", "mayonnaise", "sauce", "dressing", "oil", "vinegar", "milk", "cream", "juice", "syrup" };
        return volumeKeywords.Any(k => name.Contains(k));
    }

    private bool IsLikelyCondiment(string name)
    {
        var condimentKeywords = new[] { "mayo", "mayonnaise", "mustard", "ketchup", "sauce", "dressing", "spice", "seasoning" };
        return condimentKeywords.Any(k => name.Contains(k));
    }

    private void UpdateCounts()
    {
        MappedIngredients = Ingredients.Count(i => i.IsMapped);
        UnmappedIngredients = TotalIngredients - MappedIngredients;
    }
}

/// <summary>
/// Represents an ingredient being mapped to nutritional data
/// </summary>
public partial class IngredientMappingItem : ObservableObject
{
    [ObservableProperty]
    private Ingredient _ingredient = null!;

    [ObservableProperty]
    private ObservableCollection<NutritionalDataResult> _suggestions = new();

    [ObservableProperty]
    private NutritionalDataResult? _selectedSuggestion;

    [ObservableProperty]
    private bool _isMapped;

    [ObservableProperty]
    private string _customSearchTerm = string.Empty;
}
