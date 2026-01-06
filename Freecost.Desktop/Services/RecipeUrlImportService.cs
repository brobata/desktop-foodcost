using Freecost.Core.Models;
using Freecost.Core.Services;
using Freecost.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Freecost.Desktop.Services;

/// <summary>
/// Imports recipes from URLs using JSON-LD schema parsing
/// Supports major recipe websites that use Schema.org Recipe format
/// </summary>
public class RecipeUrlImportService : IRecipeUrlImportService
{
    private readonly HttpClient _httpClient;
    private static readonly List<string> SupportedDomains = new()
    {
        "allrecipes.com",
        "foodnetwork.com",
        "tasty.co",
        "delish.com",
        "bonappetit.com",
        "epicurious.com",
        "seriouseats.com",
        "simplyrecipes.com",
        "budgetbytes.com",
        "thekitchn.com",
        "cookieandkate.com",
        "minimalistbaker.com",
        "skinnytaste.com",
        "pinchofyum.com"
    };

    public RecipeUrlImportService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    private void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine($"  [URL IMPORT] {message}");
    }

    private void LogError(string message, Exception? ex = null)
    {
        System.Diagnostics.Debug.WriteLine("  ╔═══════════════════════════════════════════════════╗");
        System.Diagnostics.Debug.WriteLine("  ║ [URL IMPORT ERROR]                                ║");
        System.Diagnostics.Debug.WriteLine("  ╠═══════════════════════════════════════════════════╣");
        System.Diagnostics.Debug.WriteLine($"  {message}");
        if (ex != null)
        {
            System.Diagnostics.Debug.WriteLine($"  Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"  Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  Stack Trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"  Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }
        }
        System.Diagnostics.Debug.WriteLine("  ╚═══════════════════════════════════════════════════╝");
    }

    public bool IsSupportedUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return SupportedDomains.Any(domain => uri.Host.Contains(domain, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetSupportedWebsites()
    {
        return SupportedDomains.Select(d => d).ToList();
    }

    public async Task<Recipe?> ImportRecipeFromUrlAsync(string url)
    {
        try
        {
            LogDebug("╔═══════════════════════════════════════════════════╗");
            LogDebug("║ Starting Recipe URL Import                        ║");
            LogDebug("╠═══════════════════════════════════════════════════╣");
            LogDebug($"URL: {url}");

            var uri = new Uri(url);
            LogDebug($"Host: {uri.Host}");
            LogDebug($"Supported: {IsSupportedUrl(url)}");

            // Fetch the HTML content
            LogDebug("Phase 1: Fetching HTML content...");
            var html = await _httpClient.GetStringAsync(url);
            LogDebug($"✓ HTML fetched ({html.Length} characters)");

            // Try to extract JSON-LD schema data (most modern recipe sites use this)
            LogDebug("Phase 2: Attempting JSON-LD extraction...");
            var recipe = await ExtractFromJsonLdAsync(html);

            if (recipe != null)
            {
                LogDebug($"✓ Recipe extracted via JSON-LD: '{recipe.Name}'");
                LogDebug($"  Ingredients: {recipe.RecipeIngredients?.Count ?? 0}");
                LogDebug($"  Yield: {recipe.Yield} {recipe.YieldUnit}");
                LogDebug($"  Prep Time: {recipe.PrepTimeMinutes} minutes");

                recipe.Description = $"Imported from {uri.Host}";

                LogDebug("✓ Recipe import complete");
                LogDebug("╚═══════════════════════════════════════════════════╝");
                return recipe;
            }

            LogDebug("✗ JSON-LD extraction failed, trying HTML fallback...");

            // Fallback: Try to parse HTML directly (basic scraping)
            LogDebug("Phase 3: Attempting HTML extraction...");
            recipe = ExtractFromHtml(html);

            if (recipe != null)
            {
                LogDebug($"✓ Recipe extracted via HTML: '{recipe.Name}'");
                recipe.Description = $"Imported from {uri.Host}";
                LogDebug("✓ Recipe import complete");
                LogDebug("╚═══════════════════════════════════════════════════╝");
                return recipe;
            }

            LogDebug("✗ All extraction methods failed");
            LogDebug("╚═══════════════════════════════════════════════════╝");
            return null;
        }
        catch (Exception ex)
        {
            LogError($"Failed to import recipe from URL: {url}", ex);
            return null;
        }
    }

    private Task<Recipe?> ExtractFromJsonLdAsync(string html)
    {
        try
        {
            // Find JSON-LD script tags containing recipe data
            var jsonLdPattern = @"<script[^>]*type=['""]application/ld\+json['""][^>]*>(.*?)</script>";
            var matches = Regex.Matches(html, jsonLdPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            LogDebug($"Found {matches.Count} JSON-LD script blocks");

            foreach (Match match in matches)
            {
                try
                {
                    var jsonContent = match.Groups[1].Value.Trim();
                    LogDebug($"Parsing JSON-LD block ({jsonContent.Length} chars)...");

                    // Try to parse as single object
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    var root = jsonDoc.RootElement;

                    // Handle arrays (some sites wrap in array)
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        LogDebug($"JSON-LD is array with {root.GetArrayLength()} items");
                        foreach (var item in root.EnumerateArray())
                        {
                            var recipe = ParseRecipeFromJson(item);
                            if (recipe != null)
                            {
                                LogDebug("✓ Recipe found in JSON-LD array");
                                return Task.FromResult<Recipe?>(recipe);
                            }
                        }
                    }
                    else
                    {
                        LogDebug("JSON-LD is single object");
                        var recipe = ParseRecipeFromJson(root);
                        if (recipe != null)
                        {
                            LogDebug("✓ Recipe found in JSON-LD object");
                            return Task.FromResult<Recipe?>(recipe);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDebug($"✗ Failed to parse JSON-LD block: {ex.Message}");
                    // Continue to next match
                    continue;
                }
            }

            LogDebug("No recipe data found in any JSON-LD blocks");
            return Task.FromResult<Recipe?>(null);
        }
        catch (Exception ex)
        {
            LogDebug($"✗ JSON-LD extraction failed: {ex.Message}");
            return Task.FromResult<Recipe?>(null);
        }
    }

    private Recipe? ParseRecipeFromJson(JsonElement element)
    {
        try
        {
            // Check if this is a Recipe type
            if (!element.TryGetProperty("@type", out var typeElement))
                return null;

            var type = typeElement.GetString();
            if (type != "Recipe")
                return null;

            var recipe = new Recipe();

            // Extract name
            if (element.TryGetProperty("name", out var nameElement))
            {
                recipe.Name = nameElement.GetString() ?? "Imported Recipe";
            }

            // Extract description
            if (element.TryGetProperty("description", out var descElement))
            {
                recipe.Description = descElement.GetString();
            }

            // Extract category
            if (element.TryGetProperty("recipeCategory", out var categoryElement))
            {
                if (categoryElement.ValueKind == JsonValueKind.Array)
                {
                    recipe.Category = categoryElement.EnumerateArray().FirstOrDefault().GetString();
                }
                else
                {
                    recipe.Category = categoryElement.GetString();
                }
            }

            // Extract yield/servings
            if (element.TryGetProperty("recipeYield", out var yieldElement))
            {
                var yieldStr = yieldElement.ValueKind == JsonValueKind.Array
                    ? yieldElement.EnumerateArray().FirstOrDefault().GetString()
                    : yieldElement.GetString();

                if (yieldStr != null)
                {
                    var yieldMatch = Regex.Match(yieldStr, @"(\d+)");
                    if (yieldMatch.Success && decimal.TryParse(yieldMatch.Groups[1].Value, out var yieldValue))
                    {
                        recipe.Yield = yieldValue;
                        recipe.YieldUnit = "servings";
                    }
                }
            }

            // Extract prep time (convert ISO 8601 duration to minutes)
            if (element.TryGetProperty("prepTime", out var prepTimeElement))
            {
                var prepTime = prepTimeElement.GetString();
                recipe.PrepTimeMinutes = ParseIsoDuration(prepTime);
            }
            else if (element.TryGetProperty("totalTime", out var totalTimeElement))
            {
                var totalTime = totalTimeElement.GetString();
                recipe.PrepTimeMinutes = ParseIsoDuration(totalTime);
            }

            // Extract ingredients
            if (element.TryGetProperty("recipeIngredient", out var ingredientsElement))
            {
                var ingredients = new List<RecipeIngredient>();
                foreach (var ingredient in ingredientsElement.EnumerateArray())
                {
                    var ingredientText = ingredient.GetString();
                    if (!string.IsNullOrWhiteSpace(ingredientText))
                    {
                        // Parse ingredient text (e.g., "2 cups flour")
                        var parsed = ParseIngredientText(ingredientText);
                        if (parsed != null)
                        {
                            ingredients.Add(parsed);
                        }
                    }
                }
                recipe.RecipeIngredients = ingredients;
            }

            // Extract instructions
            if (element.TryGetProperty("recipeInstructions", out var instructionsElement))
            {
                var instructions = new List<string>();

                if (instructionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var instruction in instructionsElement.EnumerateArray())
                    {
                        if (instruction.ValueKind == JsonValueKind.String)
                        {
                            instructions.Add(instruction.GetString() ?? "");
                        }
                        else if (instruction.TryGetProperty("text", out var textElement))
                        {
                            instructions.Add(textElement.GetString() ?? "");
                        }
                    }
                }
                else if (instructionsElement.ValueKind == JsonValueKind.String)
                {
                    instructions.Add(instructionsElement.GetString() ?? "");
                }

                recipe.Instructions = string.Join("\n\n", instructions.Where(i => !string.IsNullOrWhiteSpace(i)));
            }

            // Extract dietary labels/tags
            var tags = new List<string>();

            if (element.TryGetProperty("keywords", out var keywordsElement))
            {
                if (keywordsElement.ValueKind == JsonValueKind.Array)
                {
                    tags.AddRange(keywordsElement.EnumerateArray().Select(k => k.GetString() ?? "").Where(k => !string.IsNullOrWhiteSpace(k)));
                }
                else if (keywordsElement.ValueKind == JsonValueKind.String)
                {
                    var keywords = keywordsElement.GetString();
                    if (keywords != null)
                    {
                        tags.AddRange(keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrWhiteSpace(k)));
                    }
                }
            }

            if (element.TryGetProperty("suitableForDiet", out var dietElement))
            {
                if (dietElement.ValueKind == JsonValueKind.Array)
                {
                    tags.AddRange(dietElement.EnumerateArray().Select(d => d.GetString() ?? "").Where(d => !string.IsNullOrWhiteSpace(d)));
                }
            }

            if (tags.Any())
            {
                recipe.Tags = string.Join(", ", tags.Distinct());
            }

            return recipe;
        }
        catch
        {
            return null;
        }
    }

    private Recipe? ExtractFromHtml(string html)
    {
        // Basic HTML scraping fallback (very simple, may not work for all sites)
        try
        {
            var recipe = new Recipe();

            // Try to extract title from h1
            var titleMatch = Regex.Match(html, @"<h1[^>]*>(.*?)</h1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                recipe.Name = StripHtmlTags(titleMatch.Groups[1].Value).Trim();
            }

            if (string.IsNullOrWhiteSpace(recipe.Name))
            {
                return null; // Need at least a name
            }

            return recipe;
        }
        catch
        {
            return null;
        }
    }

    private RecipeIngredient? ParseIngredientText(string text)
    {
        try
        {
            // Simple ingredient parsing: "2 cups flour" or "1/2 teaspoon salt"
            // Pattern: [amount] [unit] [ingredient name]
            var pattern = @"^([\d\s\/\.]+)\s*([a-zA-Z]+)?\s+(.+)$";
            var match = Regex.Match(text.Trim(), pattern);

            if (match.Success)
            {
                var quantityStr = match.Groups[1].Value.Trim();
                var unitStr = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "unit";
                var name = match.Groups[3].Value.Trim();

                // Parse fractional quantities (e.g., "1/2" or "1 1/2")
                var quantity = ParseFraction(quantityStr);

                // Convert unit string to UnitType enum
                var unit = ParseUnitType(unitStr);

                return new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    Quantity = quantity,
                    Unit = unit,
                    DisplayText = text,
                    // Note: IngredientId will need to be set later by matching to existing ingredients
                };
            }

            // If pattern doesn't match, create a generic ingredient entry
            return new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                Quantity = 1,
                Unit = UnitType.Each,
                DisplayText = text
            };
        }
        catch
        {
            return null;
        }
    }

    private decimal ParseFraction(string fractionStr)
    {
        try
        {
            fractionStr = fractionStr.Trim();

            // Handle mixed numbers (e.g., "1 1/2")
            var mixedMatch = Regex.Match(fractionStr, @"^(\d+)\s+(\d+)/(\d+)$");
            if (mixedMatch.Success)
            {
                var whole = decimal.Parse(mixedMatch.Groups[1].Value);
                var numerator = decimal.Parse(mixedMatch.Groups[2].Value);
                var denominator = decimal.Parse(mixedMatch.Groups[3].Value);
                return whole + (numerator / denominator);
            }

            // Handle simple fractions (e.g., "1/2")
            var fractionMatch = Regex.Match(fractionStr, @"^(\d+)/(\d+)$");
            if (fractionMatch.Success)
            {
                var numerator = decimal.Parse(fractionMatch.Groups[1].Value);
                var denominator = decimal.Parse(fractionMatch.Groups[2].Value);
                return numerator / denominator;
            }

            // Handle decimals or whole numbers
            if (decimal.TryParse(fractionStr, out var result))
            {
                return result;
            }

            return 1; // Default
        }
        catch
        {
            return 1;
        }
    }

    private int? ParseIsoDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
            return null;

        try
        {
            // Parse ISO 8601 duration format (e.g., "PT30M" = 30 minutes, "PT1H30M" = 90 minutes)
            var pattern = @"PT(?:(\d+)H)?(?:(\d+)M)?";
            var match = Regex.Match(duration, pattern);

            if (match.Success)
            {
                var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                return (hours * 60) + minutes;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string StripHtmlTags(string html)
    {
        return Regex.Replace(html, @"<[^>]*>", "").Trim();
    }

    private UnitType ParseUnitType(string unitStr)
    {
        // Map common cooking units to UnitType enum
        return unitStr.ToLower() switch
        {
            "cup" or "cups" => UnitType.Cup,
            "tbsp" or "tablespoon" or "tablespoons" => UnitType.Tablespoon,
            "tsp" or "teaspoon" or "teaspoons" => UnitType.Teaspoon,
            "oz" or "ounce" or "ounces" or "fl oz" => UnitType.FluidOunce,
            "lb" or "pound" or "pounds" => UnitType.Pound,
            "g" or "gram" or "grams" => UnitType.Gram,
            "kg" or "kilogram" or "kilograms" => UnitType.Kilogram,
            "ml" or "milliliter" or "milliliters" => UnitType.Milliliter,
            "l" or "liter" or "liters" => UnitType.Liter,
            "quart" or "quarts" or "qt" => UnitType.Quart,
            "pint" or "pints" or "pt" => UnitType.Pint,
            "gallon" or "gallons" or "gal" => UnitType.Gallon,
            "dozen" => UnitType.Dozen,
            _ => UnitType.Each // Default to "Each" for generic items
        };
    }
}
