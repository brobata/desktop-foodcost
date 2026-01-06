using Dfc.Core.Enums;
using Dfc.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Service for looking up nutritional data from USDA FoodData Central API
/// Free API: https://fdc.nal.usda.gov/api-guide.html
/// </summary>
public class NutritionalDataService : INutritionalDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NutritionalDataService> _logger;
        private readonly USDAConversionExtractor _conversionExtractor;
    private const string BaseUrl = "https://api.nal.usda.gov/fdc/v1";

    // Simple in-memory cache for search results (cleared when app restarts)
    private static readonly Dictionary<string, List<NutritionalDataResult>> _searchCache = new();
    private static readonly Dictionary<string, NutritionalDataResult> _detailCache = new();

    // Allergen keyword mappings for ingredient analysis
    private static readonly Dictionary<AllergenType, List<string>> AllergenKeywords = new()
    {
        { AllergenType.Milk, new() { "milk", "dairy", "cheese", "cream", "butter", "yogurt", "whey", "casein", "lactose" } },
        { AllergenType.Eggs, new() { "egg", "albumin", "mayonnaise" } },
        { AllergenType.Fish, new() { "fish", "salmon", "tuna", "cod", "halibut", "anchovy" } },
        { AllergenType.Shellfish, new() { "shellfish", "shrimp", "crab", "lobster", "clam", "oyster", "mussel", "scallop" } },
        { AllergenType.TreeNuts, new() { "almond", "walnut", "cashew", "pecan", "pistachio", "macadamia", "hazelnut", "pine nut" } },
        { AllergenType.Peanuts, new() { "peanut", "groundnut" } },
        { AllergenType.Wheat, new() { "wheat", "flour", "bread", "pasta", "semolina", "durum" } },
        { AllergenType.Soybeans, new() { "soy", "tofu", "tempeh", "edamame", "miso" } },
        { AllergenType.Sesame, new() { "sesame", "tahini" } },
        { AllergenType.GlutenFree, new() { "gluten" } } // inverted - presence means NOT gluten-free
    };

    public NutritionalDataService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<NutritionalDataService> logger,
        UnitConversionService unitConversionService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _conversionExtractor = new USDAConversionExtractor(unitConversionService, null);
    }

    public Task<bool> IsAvailableAsync()
    {
        try
        {
            _logger.LogInformation("========== USDA API AVAILABILITY CHECK ==========");

            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("USDA FoodData Central API key not configured");
                _logger.LogWarning("To use USDA nutritional data, set environment variable: USDA_API_KEY");
                _logger.LogWarning("Get free API key at: https://fdc.nal.usda.gov/api-key-signup.html");
                _logger.LogInformation("=================================================");
                return Task.FromResult(false);
            }

            _logger.LogInformation($"USDA API key found (length: {apiKey.Length})");
            _logger.LogInformation("=================================================");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking USDA API availability");
            return Task.FromResult(false);
        }
    }

    public async Task<List<NutritionalDataResult>> SearchNutritionalDataAsync(string ingredientName, int maxResults = 3)
    {
        try
        {
            // Check cache first
            var cacheKey = $"{ingredientName.ToLowerInvariant()}_{maxResults}";
            if (_searchCache.TryGetValue(cacheKey, out var cachedResults))
            {
                _logger.LogDebug("Returning cached results for: {IngredientName}", ingredientName);
                return cachedResults;
            }

            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("USDA API key not configured - returning empty results");
                return new List<NutritionalDataResult>();
            }

            var client = _httpClientFactory.CreateClient();
            var searchUrl = $"{BaseUrl}/foods/search?query={Uri.EscapeDataString(ingredientName)}&pageSize={maxResults}&api_key={apiKey}";

            _logger.LogDebug("Searching USDA FoodData Central for: {IngredientName}", ingredientName);

            var response = await client.GetAsync(searchUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("USDA API returned {StatusCode}: {Reason}", response.StatusCode, response.ReasonPhrase);
                return new List<NutritionalDataResult>();
            }

            var json = await response.Content.ReadAsStringAsync();

            // DEBUG: Log the first 500 characters of the response
            System.Diagnostics.Debug.WriteLine("========== USDA API RESPONSE DEBUG ==========");
            System.Diagnostics.Debug.WriteLine($"Ingredient: {ingredientName}");
            System.Diagnostics.Debug.WriteLine($"Response (first 500 chars): {(json.Length > 500 ? json.Substring(0, 500) : json)}");

            var searchResult = JsonSerializer.Deserialize<UsdaSearchResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResult?.Foods == null || !searchResult.Foods.Any())
            {
                System.Diagnostics.Debug.WriteLine("No results found");
                System.Diagnostics.Debug.WriteLine("=============================================");
                _logger.LogInformation("No results found for: {IngredientName}", ingredientName);
                return new List<NutritionalDataResult>();
            }

            // DEBUG: Log first food item structure
            if (searchResult.Foods.Count > 0)
            {
                var firstFood = searchResult.Foods[0];
                System.Diagnostics.Debug.WriteLine($"First food: FdcId={firstFood.FdcId}, Description={firstFood.Description}");
                System.Diagnostics.Debug.WriteLine($"Nutrient Count: {firstFood.FoodNutrients?.Count ?? 0}");

                if (firstFood.FoodNutrients?.Any() == true)
                {
                    System.Diagnostics.Debug.WriteLine("ALL NUTRIENTS (Search Response - ABBREVIATED DATA):");
                    foreach (var nutrient in firstFood.FoodNutrients.Take(20))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - Id={nutrient.Nutrient?.Id}, Name={nutrient.Nutrient?.Name}, Amount={nutrient.Amount}, Unit={nutrient.Nutrient?.UnitName}");
                    }

                    // Check for the specific nutrients we're looking for
                    var calories = firstFood.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 2047);
                    var protein = firstFood.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 1003);
                    var carbs = firstFood.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 1005);
                    var fat = firstFood.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 1004);

                    System.Diagnostics.Debug.WriteLine("MACRONUTRIENTS FOUND (in search response):");
                    System.Diagnostics.Debug.WriteLine($"  - Calories (2047): {(calories != null ? $"{calories.Amount} {calories.Nutrient?.UnitName}" : "NOT FOUND")}");
                    System.Diagnostics.Debug.WriteLine($"  - Protein (1003): {(protein != null ? $"{protein.Amount} {protein.Nutrient?.UnitName}" : "NOT FOUND")}");
                    System.Diagnostics.Debug.WriteLine($"  - Carbs (1005): {(carbs != null ? $"{carbs.Amount} {carbs.Nutrient?.UnitName}" : "NOT FOUND")}");
                    System.Diagnostics.Debug.WriteLine($"  - Fat (1004): {(fat != null ? $"{fat.Amount} {fat.Nutrient?.UnitName}" : "NOT FOUND")}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: No nutrients found in first food item!");
                }
            }
            System.Diagnostics.Debug.WriteLine("=============================================");

            // NOTE: The search endpoint returns abridged data with nutrient amounts = 0
            // We need to fetch full details for each food using the /food/{fdcId} endpoint
            // Use parallel API calls for better performance
            var fdcIds = searchResult.Foods.Take(maxResults)
                .Select(f => f.FdcId?.ToString() ?? "")
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            var detailTasks = fdcIds.Select(fdcId => GetNutritionalDataByIdAsync(fdcId)).ToArray();
            var detailResults = await Task.WhenAll(detailTasks);

            var results = detailResults.Where(r => r != null).Cast<NutritionalDataResult>().ToList();

            _logger.LogInformation("Found {Count} results for: {IngredientName}", results.Count, ingredientName);

            // Cache the results
            _searchCache[cacheKey] = results;

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching nutritional data for: {IngredientName}", ingredientName);
            return new List<NutritionalDataResult>();
        }
    }

    public async Task<NutritionalDataResult?> GetNutritionalDataByIdAsync(string fdcId)
    {
        try
        {
            // Check cache first
            if (_detailCache.TryGetValue(fdcId, out var cachedDetail))
            {
                _logger.LogDebug("Returning cached detail for FDC ID: {FdcId}", fdcId);
                return cachedDetail;
            }

            var apiKey = GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"{BaseUrl}/food/{fdcId}?api_key={apiKey}";

            System.Diagnostics.Debug.WriteLine($"========== FETCHING FULL FOOD DETAILS: {fdcId} ==========");

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: API returned {response.StatusCode}");
                _logger.LogWarning("USDA API returned {StatusCode} for FDC ID {FdcId}", response.StatusCode, fdcId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response length: {json.Length} chars");
            System.Diagnostics.Debug.WriteLine($"Response preview (first 300 chars): {(json.Length > 300 ? json.Substring(0, 300) : json)}");

            var food = JsonSerializer.Deserialize<UsdaFood>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (food != null)
            {
                System.Diagnostics.Debug.WriteLine($"Deserialized successfully. FoodNutrients count: {food.FoodNutrients?.Count ?? 0}");
                if (food.FoodNutrients?.Any() == true)
                {
                    var calories = food.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 2047);
                    var protein = food.FoodNutrients.FirstOrDefault(n => n.Nutrient?.Id == 1003);
                    System.Diagnostics.Debug.WriteLine($"Sample nutrients - Calories (2047): {calories?.Amount ?? 0}, Protein (1003): {protein?.Amount ?? 0}");

                    // Log first 5 nutrients for debugging
                    System.Diagnostics.Debug.WriteLine("First 5 nutrients:");
                    foreach (var n in food.FoodNutrients.Take(5))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {n.Nutrient?.Name} (ID {n.Nutrient?.Id}): {n.Amount} {n.Nutrient?.UnitName}");
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("=========================================");

            var result = food != null ? MapToNutritionalDataResult(food, "") : null;

            // Cache the result if successful
            if (result != null)
            {
                _detailCache[fdcId] = result;
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EXCEPTION in GetNutritionalDataByIdAsync for {fdcId}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            _logger.LogError(ex, "Error getting nutritional data for FDC ID: {FdcId}", fdcId);
            return null;
        }
    }

    private string? GetApiKey()
    {
        try
        {
            // Try to get API key from configuration (appsettings.json or environment variable)
            // Users can get a free API key from: https://fdc.nal.usda.gov/api-key-signup.html

            // Method 1: Check IConfiguration (which should have environment variables loaded)
            var configKey = _configuration["USDA:ApiKey"];

            // Method 2: Check environment variable directly with single underscore (standard naming)
            var envKeySingle = Environment.GetEnvironmentVariable("USDA_API_KEY");

            // Method 3: Check environment variable with double underscore (IConfiguration convention)
            var envKeyDouble = Environment.GetEnvironmentVariable("USDA__ApiKey");

            _logger.LogDebug("========== GetApiKey() Diagnostic ==========");
            _logger.LogDebug("IConfiguration[USDA:ApiKey]: {HasConfig}",
                !string.IsNullOrEmpty(configKey) ? $"FOUND (length: {configKey.Length})" : "NOT FOUND");
            _logger.LogDebug("Environment.GetEnvironmentVariable(USDA_API_KEY): {HasEnv}",
                !string.IsNullOrEmpty(envKeySingle) ? $"FOUND (length: {envKeySingle.Length})" : "NOT FOUND");
            _logger.LogDebug("Environment.GetEnvironmentVariable(USDA__ApiKey): {HasEnvDouble}",
                !string.IsNullOrEmpty(envKeyDouble) ? $"FOUND (length: {envKeyDouble.Length})" : "NOT FOUND");

            // Try all three sources in order of preference
            var result = configKey ?? envKeyDouble ?? envKeySingle;

            if (!string.IsNullOrEmpty(result))
            {
                _logger.LogDebug("API Key selected from: {Source}",
                    !string.IsNullOrEmpty(configKey) ? "IConfiguration[USDA:ApiKey]" :
                    !string.IsNullOrEmpty(envKeyDouble) ? "Environment Variable (USDA__ApiKey)" :
                    "Environment Variable (USDA_API_KEY)");
            }
            else
            {
                _logger.LogWarning("No USDA API key found in any source");
            }

            _logger.LogDebug("===========================================");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving USDA API key");
            return null;
        }
    }

    private NutritionalDataResult MapToNutritionalDataResult(UsdaFood food, string searchTerm)
    {
        var result = new NutritionalDataResult
        {
            FdcId = food.FdcId?.ToString() ?? "",
            Description = food.Description ?? "",
            Category = food.FoodCategory ?? "",
            MatchScore = CalculateMatchScore(food.Description ?? "", searchTerm)
        };

        // Extract nutritional data per 100g
        if (food.FoodNutrients != null && food.FoodNutrients.Any())
        {
            // Try Energy (Atwater General Factors) first (2047), then Energy (kcal) (1008) as fallback
            var calories = GetNutrientValue(food.FoodNutrients, 2047);
            if (calories == 0)
            {
                calories = GetNutrientValue(food.FoodNutrients, 1008);
            }

            result.NutritionPer100g = new NutritionalInfo
            {
                Calories = calories,
                Protein = GetNutrientValue(food.FoodNutrients, 1003), // Protein
                Carbohydrates = GetNutrientValue(food.FoodNutrients, 1005), // Carbohydrate
                Fat = GetNutrientValue(food.FoodNutrients, 1004), // Total lipid (fat)
                Fiber = GetNutrientValue(food.FoodNutrients, 1079), // Fiber
                Sugar = GetNutrientValue(food.FoodNutrients, 2000), // Total sugars
                Sodium = GetNutrientValue(food.FoodNutrients, 1093) / 1000m // Sodium (mg -> g)
            };
        }

        // Extract serving sizes
        if (food.FoodPortions != null && food.FoodPortions.Any())
        {
            result.ServingSizes = food.FoodPortions
                .Where(p => p.GramWeight.HasValue && p.GramWeight.Value > 0)
                .Select(p => new ServingSize
                {
                    Description = GetServingSizeDescription(p),
                    Grams = p.GramWeight!.Value,
                    IsPreferred = p.PortionDescription?.Contains("medium", StringComparison.OrdinalIgnoreCase) == true ||
                                  p.PortionDescription?.Contains("whole", StringComparison.OrdinalIgnoreCase) == true
                })
                .ToList();
        }

        // Detect allergens based on description and category
        result.DetectedAllergens = DetectAllergens(food.Description ?? "", food.Ingredients ?? "");

        return result;
    }

    private decimal GetNutrientValue(List<UsdaFoodNutrient> nutrients, int nutrientId)
    {
        var nutrient = nutrients.FirstOrDefault(n => n.Nutrient?.Id == nutrientId);
        return nutrient?.Amount ?? 0;
    }

    private string GetServingSizeDescription(UsdaPortion portion)
    {
        // Prefer Modifier if available (e.g., "1 cup", "1 medium")
        if (!string.IsNullOrWhiteSpace(portion.Modifier))
        {
            return portion.Modifier;
        }

        // Use PortionDescription if available
        if (!string.IsNullOrWhiteSpace(portion.PortionDescription))
        {
            return portion.PortionDescription;
        }

        // Build from Amount and MeasureUnit, but skip if MeasureUnit is "RACC" (Reference Amount)
        // or if Amount is absurdly large (> 10000)
        var measureName = portion.MeasureUnit?.Name ?? "";
        if (!string.IsNullOrWhiteSpace(measureName) &&
            !measureName.Equals("RACC", StringComparison.OrdinalIgnoreCase) &&
            portion.Amount.HasValue &&
            portion.Amount.Value > 0 &&
            portion.Amount.Value < 10000)
        {
            return $"{portion.Amount.Value:F0} {measureName}";
        }

        // Fallback: Just describe the gram weight
        return $"{portion.GramWeight:F0}g serving";
    }

    private int CalculateMatchScore(string foodDescription, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return 100;

        var desc = foodDescription.ToLowerInvariant();
        var search = searchTerm.ToLowerInvariant();

        // Exact match
        if (desc == search)
            return 100;

        // Starts with search term
        if (desc.StartsWith(search))
            return 95;

        // Contains exact search term
        if (desc.Contains(search))
            return 90;

        // Contains all words from search term
        var searchWords = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (searchWords.All(word => desc.Contains(word)))
            return 80;

        // Contains some words
        var matchingWords = searchWords.Count(word => desc.Contains(word));
        return 50 + (matchingWords * 30 / searchWords.Length);
    }

    private List<AllergenType> DetectAllergens(string description, string ingredients)
    {
        var detected = new List<AllergenType>();
        var combinedText = $"{description} {ingredients}".ToLowerInvariant();

        foreach (var (allergenType, keywords) in AllergenKeywords)
        {
            if (allergenType == AllergenType.GlutenFree)
            {
                // Gluten-free is inverted - if we find gluten keywords, it's NOT gluten-free
                if (keywords.Any(keyword => combinedText.Contains(keyword)))
                {
                    // Don't add GlutenFree - item contains gluten
                }
            }
            else if (keywords.Any(keyword => combinedText.Contains(keyword)))
            {
                detected.Add(allergenType);
            }
        }

        return detected;
    }

    // USDA API Response Models
    private class UsdaSearchResponse
    {
        public List<UsdaFood>? Foods { get; set; }
    }

    private class UsdaFood
    {
        public int? FdcId { get; set; }
        public string? Description { get; set; }

        // FoodCategory can be either a string (search endpoint) or an object (detail endpoint)
        [System.Text.Json.Serialization.JsonConverter(typeof(FoodCategoryConverter))]
        public string? FoodCategory { get; set; }

        public string? Ingredients { get; set; }
        public List<UsdaFoodNutrient>? FoodNutrients { get; set; }
        public List<UsdaPortion>? FoodPortions { get; set; }
    }

    private class UsdaFoodNutrient
    {
        public string? Type { get; set; }
        public UsdaNutrient? Nutrient { get; set; }
        public decimal? Amount { get; set; }
    }

    private class UsdaNutrient
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public int? Rank { get; set; }
        public string? UnitName { get; set; }
    }

    private class UsdaPortion
    {
        public decimal? Amount { get; set; }
        public string? Modifier { get; set; }
        public decimal? GramWeight { get; set; }
        public string? PortionDescription { get; set; }
        public UsdaMeasureUnit? MeasureUnit { get; set; }
    }

    private class UsdaMeasureUnit
    {
        public string? Name { get; set; }
    }

    /// <summary>
    /// Custom JSON converter to handle foodCategory being either a string or an object
    /// Search endpoint returns it as a string, detail endpoint returns it as an object
    /// </summary>
    private class FoodCategoryConverter : System.Text.Json.Serialization.JsonConverter<string?>
    {
        public override string? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.String)
            {
                // Search endpoint: "foodCategory": "Vegetables and Vegetable Products"
                return reader.GetString();
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.StartObject)
            {
                // Detail endpoint: "foodCategory": {"id":11,"code":"100","description":"Vegetables..."}
                using var doc = JsonDocument.ParseValue(ref reader);
                if (doc.RootElement.TryGetProperty("description", out var desc))
                {
                    return desc.GetString();
                }
                return null;
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException($"Unexpected token type for foodCategory: {reader.TokenType}");
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    /// <summary>
    /// Extracts ingredient conversions from USDA nutritional data.
    /// Searches for the ingredient, gets the best match, and extracts serving size conversions.
    /// </summary>
    public async Task<List<IngredientConversion>> ExtractConversionsAsync(
        string ingredientName,
        Guid? ingredientId = null,
        Guid? locationId = null)
    {
        try
        {
            _logger.LogInformation("Extracting conversions from USDA for: {IngredientName}", ingredientName);

            // Search for the ingredient in USDA database
            var searchResults = await SearchNutritionalDataAsync(ingredientName, maxResults: 1);
            
            if (!searchResults.Any())
            {
                _logger.LogWarning("No USDA results found for: {IngredientName}", ingredientName);
                return new List<IngredientConversion>();
            }

            // Use the best match (first result)
            var bestMatch = searchResults.First();
            
            _logger.LogDebug("Using USDA match: {Description} (FDC ID: {FdcId})", 
                bestMatch.Description, 
                bestMatch.FdcId);

            // Extract conversions using the extractor
            var conversions = _conversionExtractor.ExtractOptimalConversions(
                bestMatch, 
                ingredientId, 
                locationId
            );

            _logger.LogInformation("Extracted {Count} conversions for {IngredientName}", 
                conversions.Count, 
                ingredientName);

            return conversions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting conversions for: {IngredientName}", ingredientName);
            return new List<IngredientConversion>();
        }
    }

}
