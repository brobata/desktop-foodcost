using Dfc.Core.Enums;
using Dfc.Core.Models;

namespace Dfc.Core.Services;

public class AllergenDetectionService : IAllergenDetectionService
{
    private readonly IGlobalConfigService _globalConfigService;

    // Hardcoded fallback mapping of ingredient keywords to allergen types
    private readonly Dictionary<AllergenType, List<string>> _fallbackAllergenKeywords = new()
    {
        // FDA Big 9
        [AllergenType.Milk] = new() { "milk", "cream", "butter", "cheese", "yogurt", "cheddar", "mozzarella", "parmesan", "gorgonzola", "brie", "feta", "ricotta", "whey", "casein", "lactose" },
        [AllergenType.Eggs] = new() { "egg", "mayonnaise", "mayo", "aioli" },
        [AllergenType.Fish] = new() { "salmon", "tuna", "cod", "halibut", "anchovies", "anchovy", "fish sauce", "tilapia", "bass", "trout", "sardine" },
        [AllergenType.Shellfish] = new() { "shrimp", "crab", "lobster", "clam", "mussel", "oyster", "scallop", "prawn", "crawfish", "crayfish" },
        [AllergenType.TreeNuts] = new() { "almond", "cashew", "walnut", "pecan", "pistachio", "macadamia", "hazelnut", "pine nut", "brazil nut", "chestnut" },
        [AllergenType.Peanuts] = new() { "peanut", "peanut butter" },
        [AllergenType.Wheat] = new() { "wheat", "flour", "bread", "pasta", "noodle", "couscous", "bulgur", "semolina", "farro", "spelt", "rye", "barley" },
        [AllergenType.Soybeans] = new() { "soy", "soybean", "tofu", "edamame", "soy sauce", "tamari", "miso", "tempeh" },
        [AllergenType.Sesame] = new() { "sesame", "tahini" },

        // Dietary restrictions
        [AllergenType.Vegan] = new(), // Detected by absence of animal products
        [AllergenType.Vegetarian] = new(), // Detected by absence of meat/fish
        [AllergenType.GlutenFree] = new(), // Detected by absence of gluten-containing grains

        // Religious
        [AllergenType.Kosher] = new() { "pork", "ham", "bacon", "shellfish" }, // Non-kosher items
        [AllergenType.Halal] = new() { "pork", "ham", "bacon", "alcohol", "wine", "beer", "vodka", "rum" }, // Non-halal items

        // Additional
        [AllergenType.ContainsAlcohol] = new() { "alcohol", "wine", "beer", "vodka", "rum", "whiskey", "bourbon", "brandy", "liqueur", "sake" },
        [AllergenType.Nightshades] = new() { "tomato", "potato", "pepper", "bell pepper", "chili", "eggplant", "paprika", "cayenne" },
        [AllergenType.Sulfites] = new() { "sulfite", "wine", "dried fruit" },
        [AllergenType.AddedSugar] = new() { "sugar", "syrup", "honey", "corn syrup", "agave", "molasses" }
    };

    public AllergenDetectionService(IGlobalConfigService globalConfigService)
    {
        _globalConfigService = globalConfigService;
    }

    public List<AllergenType> DetectAllergensFromIngredient(string ingredientName)
    {
        var detectedAllergens = new List<AllergenType>();
        var lowerName = ingredientName.ToLower().Trim();

        // FIRST: Try Firebase global allergen keywords
        var globalKeywords = _globalConfigService.GetAllAllergenKeywords();
        var keywordsToUse = globalKeywords.Any() ? globalKeywords : _fallbackAllergenKeywords;

        if (globalKeywords.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[AllergenDetection] Using {globalKeywords.Values.Sum(list => list.Count)} Firebase allergen keywords");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[AllergenDetection] Firebase keywords not available - using hardcoded fallback");
        }

        foreach (var (allergenType, keywords) in keywordsToUse)
        {
            foreach (var keyword in keywords)
            {
                if (lowerName.Contains(keyword))
                {
                    detectedAllergens.Add(allergenType);
                    break; // Found a match for this allergen type, move to next
                }
            }
        }

        return detectedAllergens;
    }

    public Dictionary<AllergenType, List<string>> DetectAllergensFromRecipe(Recipe recipe)
    {
        var allergenSources = new Dictionary<AllergenType, List<string>>();

        if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
            return allergenSources;

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            if (recipeIngredient.Ingredient == null) continue;

            var ingredientName = recipeIngredient.Ingredient.Name;

            // Detect from ingredient name
            var detectedAllergens = DetectAllergensFromIngredient(ingredientName);

            foreach (var allergen in detectedAllergens)
            {
                if (!allergenSources.ContainsKey(allergen))
                {
                    allergenSources[allergen] = new List<string>();
                }

                if (!allergenSources[allergen].Contains(ingredientName))
                {
                    allergenSources[allergen].Add(ingredientName);
                }
            }

            // Also include manually-set allergens from the ingredient
            if (recipeIngredient.Ingredient.IngredientAllergens != null)
            {
                foreach (var ingredientAllergen in recipeIngredient.Ingredient.IngredientAllergens.Where(ia => ia.IsEnabled))
                {
                    var allergenType = ingredientAllergen.Allergen.Type;

                    if (!allergenSources.ContainsKey(allergenType))
                    {
                        allergenSources[allergenType] = new List<string>();
                    }

                    if (!allergenSources[allergenType].Contains(ingredientName))
                    {
                        allergenSources[allergenType].Add(ingredientName);
                    }
                }
            }
        }

        // Also include manually-set allergens from the recipe itself
        if (recipe.RecipeAllergens != null)
        {
            foreach (var recipeAllergen in recipe.RecipeAllergens.Where(ra => ra.IsEnabled))
            {
                var allergenType = recipeAllergen.Allergen.Type;

                if (!allergenSources.ContainsKey(allergenType))
                {
                    allergenSources[allergenType] = new List<string>();
                }

                // Add recipe name as source for manually-added allergens
                var recipeName = recipe.Name ?? "Recipe";
                if (!allergenSources[allergenType].Contains(recipeName))
                {
                    allergenSources[allergenType].Add(recipeName);
                }
            }
        }

        return allergenSources;
    }

    public Dictionary<AllergenType, List<string>> DetectAllergensFromEntree(Entree entree)
    {
        var allergenSources = new Dictionary<AllergenType, List<string>>();

        // Detect from direct ingredients
        if (entree.EntreeIngredients != null)
        {
            foreach (var entreeIngredient in entree.EntreeIngredients)
            {
                if (entreeIngredient.Ingredient == null) continue;

                var ingredientName = entreeIngredient.Ingredient.Name;

                // Detect from ingredient name
                var detectedAllergens = DetectAllergensFromIngredient(ingredientName);

                foreach (var allergen in detectedAllergens)
                {
                    if (!allergenSources.ContainsKey(allergen))
                    {
                        allergenSources[allergen] = new List<string>();
                    }

                    if (!allergenSources[allergen].Contains(ingredientName))
                    {
                        allergenSources[allergen].Add(ingredientName);
                    }
                }

                // Also include manually-set allergens from the ingredient
                if (entreeIngredient.Ingredient.IngredientAllergens != null)
                {
                    foreach (var ingredientAllergen in entreeIngredient.Ingredient.IngredientAllergens.Where(ia => ia.IsEnabled))
                    {
                        var allergenType = ingredientAllergen.Allergen.Type;

                        if (!allergenSources.ContainsKey(allergenType))
                        {
                            allergenSources[allergenType] = new List<string>();
                        }

                        if (!allergenSources[allergenType].Contains(ingredientName))
                        {
                            allergenSources[allergenType].Add(ingredientName);
                        }
                    }
                }
            }
        }

        // Detect from recipes
        if (entree.EntreeRecipes != null)
        {
            foreach (var entreeRecipe in entree.EntreeRecipes)
            {
                if (entreeRecipe.Recipe == null) continue;

                var recipeAllergens = DetectAllergensFromRecipe(entreeRecipe.Recipe);

                foreach (var (allergen, sources) in recipeAllergens)
                {
                    if (!allergenSources.ContainsKey(allergen))
                    {
                        allergenSources[allergen] = new List<string>();
                    }

                    // Format: "RecipeName (Ingredient1, Ingredient2)"
                    var recipeSource = $"{entreeRecipe.Recipe.Name} ({string.Join(", ", sources)})";
                    if (!allergenSources[allergen].Contains(recipeSource))
                    {
                        allergenSources[allergen].Add(recipeSource);
                    }
                }
            }
        }

        return allergenSources;
    }
}
