using Dfc.Core.Models;
using Dfc.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IIngredientMatchingService
{
    Task<IngredientMatchResult> FindBestMatchAsync(string searchName, Guid locationId, decimal confidenceThreshold = 70);
    Task<List<IngredientMatchSuggestion>> GetSuggestionsAsync(string searchName, Guid locationId, int maxSuggestions = 5);
    Task<RecipeMatchResult> FindBestRecipeMatchAsync(string searchName, Guid locationId, decimal confidenceThreshold = 70);
    Task<List<RecipeMatchSuggestion>> GetRecipeSuggestionsAsync(string searchName, Guid locationId, int maxSuggestions = 5);
}

public class IngredientMatchingService : IIngredientMatchingService
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IIngredientMatchMappingService _mappingService;
    private readonly IGlobalConfigService _globalConfigService;

    public IngredientMatchingService(
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository,
        IIngredientMatchMappingService mappingService,
        IGlobalConfigService globalConfigService)
    {
        _ingredientRepository = ingredientRepository;
        _recipeRepository = recipeRepository;
        _mappingService = mappingService;
        _globalConfigService = globalConfigService;
    }

    public async Task<IngredientMatchResult> FindBestMatchAsync(string searchName, Guid locationId, decimal confidenceThreshold = 70)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return new IngredientMatchResult { IsMatched = false };
        }

        // FIRST: Check if we have a saved mapping for this import name (LOCAL - highest priority)
        var savedMapping = await _mappingService.GetMappingForNameAsync(searchName, locationId);
        if (savedMapping != null)
        {
            // User has previously matched this import name to an ingredient or recipe
            if (savedMapping.MatchedIngredientId.HasValue)
            {
                var ingredient = await _ingredientRepository.GetByIdAsync(savedMapping.MatchedIngredientId.Value);
                if (ingredient != null)
                {
                    return new IngredientMatchResult
                    {
                        IsMatched = true,
                        IngredientId = ingredient.Id,
                        IngredientName = ingredient.Name,
                        Confidence = 100,
                        MatchMethod = "Saved"
                    };
                }
            }
            // If mapping points to a recipe instead of ingredient, fall through to regular matching
        }

        // SECOND: Check global mappings from Firebase (admin-curated, works for all users)
        var globalMapping = _globalConfigService.GetIngredientMapping(searchName);
        if (globalMapping != null && !string.IsNullOrWhiteSpace(globalMapping.MatchedIngredientName))
        {
            // Global mapping found - look up the ingredient by name in local database
            var ingredient = await _ingredientRepository.GetByNameAsync(globalMapping.MatchedIngredientName, locationId);
            if (ingredient != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MATCHING] Global mapping match: '{searchName}' → '{ingredient.Name}' (from Firebase)");
                return new IngredientMatchResult
                {
                    IsMatched = true,
                    IngredientId = ingredient.Id,
                    IngredientName = ingredient.Name,
                    Confidence = 95,
                    MatchMethod = "Global"
                };
            }
        }

        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var normalized = NormalizeName(searchName);

        // Try exact match first
        var exactMatch = ingredients.FirstOrDefault(i =>
            NormalizeName(i.Name).Equals(normalized, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return new IngredientMatchResult
            {
                IsMatched = true,
                IngredientId = exactMatch.Id,
                IngredientName = exactMatch.Name,
                Confidence = 100,
                MatchMethod = "Exact"
            };
        }

        // Try alias match
        foreach (var ingredient in ingredients)
        {
            if (ingredient.Aliases != null && ingredient.Aliases.Any())
            {
                var aliasMatch = ingredient.Aliases.FirstOrDefault(a =>
                    NormalizeName(a.AliasName).Equals(normalized, StringComparison.OrdinalIgnoreCase));

                if (aliasMatch != null)
                {
                    return new IngredientMatchResult
                    {
                        IsMatched = true,
                        IngredientId = ingredient.Id,
                        IngredientName = ingredient.Name,
                        Confidence = 95,
                        MatchMethod = "Alias"
                    };
                }
            }
        }

        // Try fuzzy match
        var fuzzyMatches = ingredients
            .Select(i => new
            {
                Ingredient = i,
                Confidence = CalculateSimilarity(normalized, NormalizeName(i.Name))
            })
            .Where(m => m.Confidence >= confidenceThreshold)
            .OrderByDescending(m => m.Confidence)
            .ToList();

        if (fuzzyMatches.Any())
        {
            var best = fuzzyMatches.First();
            return new IngredientMatchResult
            {
                IsMatched = true,
                IngredientId = best.Ingredient.Id,
                IngredientName = best.Ingredient.Name,
                Confidence = best.Confidence,
                MatchMethod = "Fuzzy"
            };
        }

        return new IngredientMatchResult { IsMatched = false };
    }

    public async Task<List<IngredientMatchSuggestion>> GetSuggestionsAsync(string searchName, Guid locationId, int maxSuggestions = 5)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return new List<IngredientMatchSuggestion>();
        }

        var ingredients = await _ingredientRepository.GetAllAsync(locationId);
        var normalized = NormalizeName(searchName);

        var suggestions = ingredients
            .Select(i => new
            {
                Ingredient = i,
                Confidence = CalculateSimilarity(normalized, NormalizeName(i.Name)),
                HasAlias = i.Aliases?.Any(a => NormalizeName(a.AliasName).Contains(normalized)) ?? false
            })
            .Where(s => s.Confidence >= 50 || s.HasAlias) // Lower threshold for suggestions
            .OrderByDescending(s => s.Confidence)
            .Take(maxSuggestions)
            .Select(s => new IngredientMatchSuggestion
            {
                IngredientId = s.Ingredient.Id,
                IngredientName = s.Ingredient.Name,
                Confidence = s.Confidence,
                Reason = s.Confidence >= 80 ? "Very similar name" :
                         s.HasAlias ? "Matches alias" :
                         s.Confidence >= 60 ? "Similar name" :
                         "Possible match"
            })
            .ToList();

        return suggestions;
    }

    public async Task<RecipeMatchResult> FindBestRecipeMatchAsync(string searchName, Guid locationId, decimal confidenceThreshold = 70)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return new RecipeMatchResult { IsMatched = false };
        }

        // FIRST: Check if we have a saved mapping for this import name (LOCAL - highest priority)
        var savedMapping = await _mappingService.GetMappingForNameAsync(searchName, locationId);
        if (savedMapping != null)
        {
            // User has previously matched this import name to a recipe or ingredient
            if (savedMapping.MatchedRecipeId.HasValue)
            {
                var recipe = await _recipeRepository.GetRecipeByIdAsync(savedMapping.MatchedRecipeId.Value);
                if (recipe != null)
                {
                    return new RecipeMatchResult
                    {
                        IsMatched = true,
                        RecipeId = recipe.Id,
                        RecipeName = recipe.Name,
                        Confidence = 100,
                        MatchMethod = "Saved"
                    };
                }
            }
            // If mapping points to an ingredient instead of recipe, fall through to regular matching
        }

        // SECOND: Check global mappings from Firebase (admin-curated, works for all users)
        var globalMapping = _globalConfigService.GetIngredientMapping(searchName);
        if (globalMapping != null && !string.IsNullOrWhiteSpace(globalMapping.MatchedRecipeName))
        {
            // Global mapping found - look up the recipe by name in local database
            var recipe = await _recipeRepository.GetByNameAsync(globalMapping.MatchedRecipeName, locationId);
            if (recipe != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MATCHING] Global mapping match: '{searchName}' → '{recipe.Name}' (from Firebase)");
                return new RecipeMatchResult
                {
                    IsMatched = true,
                    RecipeId = recipe.Id,
                    RecipeName = recipe.Name,
                    Confidence = 95,
                    MatchMethod = "Global"
                };
            }
        }

        var recipes = (await _recipeRepository.GetAllRecipesAsync(locationId)).ToList();
        var normalized = NormalizeName(searchName);

        // Try exact match first
        var exactMatch = recipes.FirstOrDefault(r =>
            NormalizeName(r.Name).Equals(normalized, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return new RecipeMatchResult
            {
                IsMatched = true,
                RecipeId = exactMatch.Id,
                RecipeName = exactMatch.Name,
                Confidence = 100,
                MatchMethod = "Exact"
            };
        }

        // Try fuzzy match
        var fuzzyMatches = recipes
            .Select(r => new
            {
                Recipe = r,
                Confidence = CalculateSimilarity(normalized, NormalizeName(r.Name))
            })
            .Where(m => m.Confidence >= confidenceThreshold)
            .OrderByDescending(m => m.Confidence)
            .ToList();

        if (fuzzyMatches.Any())
        {
            var best = fuzzyMatches.First();
            return new RecipeMatchResult
            {
                IsMatched = true,
                RecipeId = best.Recipe.Id,
                RecipeName = best.Recipe.Name,
                Confidence = best.Confidence,
                MatchMethod = "Fuzzy"
            };
        }

        return new RecipeMatchResult { IsMatched = false };
    }

    public async Task<List<RecipeMatchSuggestion>> GetRecipeSuggestionsAsync(string searchName, Guid locationId, int maxSuggestions = 5)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return new List<RecipeMatchSuggestion>();
        }

        var recipes = (await _recipeRepository.GetAllRecipesAsync(locationId)).ToList();
        var normalized = NormalizeName(searchName);

        var suggestions = recipes
            .Select(r => new
            {
                Recipe = r,
                Confidence = CalculateSimilarity(normalized, NormalizeName(r.Name))
            })
            .Where(s => s.Confidence >= 50) // Lower threshold for suggestions
            .OrderByDescending(s => s.Confidence)
            .Take(maxSuggestions)
            .Select(s => new RecipeMatchSuggestion
            {
                RecipeId = s.Recipe.Id,
                RecipeName = s.Recipe.Name,
                Confidence = s.Confidence,
                Reason = s.Confidence >= 80 ? "Very similar name" :
                         s.Confidence >= 60 ? "Similar name" :
                         "Possible match"
            })
            .ToList();

        return suggestions;
    }

    /// <summary>
    /// Normalize name for comparison: lowercase, trim, remove extra spaces
    /// </summary>
    private string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return string.Join(" ", name.Trim().ToLowerInvariant().Split(
            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Calculate similarity between two strings using multiple matching strategies
    /// Returns percentage 0-100
    /// </summary>
    private decimal CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
            return 100;

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0;

        // Strategy 1: Exact substring containment (85% confidence)
        // e.g., "salt" in "kosher salt" or "kosher salt" in "salt"
        if (target.Contains(source) || source.Contains(target))
        {
            return 85;
        }

        // Strategy 2: Word-level matching
        var sourceWords = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var targetWords = target.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Count matching words
        int matchingWords = 0;
        foreach (var sourceWord in sourceWords)
        {
            // Skip very short words (articles, prepositions) that cause false matches
            if (sourceWord.Length <= 2)
                continue;

            foreach (var targetWord in targetWords)
            {
                if (sourceWord.Equals(targetWord, StringComparison.OrdinalIgnoreCase))
                {
                    matchingWords++;
                    break; // Only count each source word once
                }
            }
        }

        // Require significant word overlap for high confidence
        var sourceSignificantWords = sourceWords.Count(w => w.Length > 2);
        if (sourceSignificantWords > 0)
        {
            var matchRatio = (decimal)matchingWords / sourceSignificantWords;

            // 100% of significant words match -> 85% confidence
            if (matchRatio >= 0.9m)
                return 85;

            // 50%+ of significant words match -> 75% confidence
            if (matchRatio >= 0.5m)
                return 75;

            // Some words match -> 65% confidence (below default threshold)
            if (matchingWords > 0)
                return 65;
        }

        // Strategy 3: Partial word matching (60% confidence - below default threshold)
        // e.g., "salt" contained in any word of "kosher salt"
        // Lower confidence to avoid false matches
        foreach (var sourceWord in sourceWords)
        {
            if (sourceWord.Length <= 3) // Skip very short words
                continue;

            foreach (var targetWord in targetWords)
            {
                if (targetWord.Contains(sourceWord) || sourceWord.Contains(targetWord))
                {
                    return 60; // Below threshold - won't auto-match
                }
            }
        }

        // Strategy 4: Levenshtein distance (fallback)
        var distance = LevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        if (maxLength == 0)
            return 100;

        var similarity = (1.0m - (decimal)distance / maxLength) * 100;
        return Math.Max(0, Math.Min(100, similarity));
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings
    /// </summary>
    private int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        // Initialize first column and row
        for (int i = 0; i <= source.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= target.Length; j++)
            matrix[0, j] = j;

        // Calculate distances
        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}

/// <summary>
/// Result of ingredient matching
/// </summary>
public class IngredientMatchResult
{
    public bool IsMatched { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal Confidence { get; set; } // 0-100%
    public string MatchMethod { get; set; } = string.Empty; // "Exact", "Fuzzy", "Alias", "Manual"
}

/// <summary>
/// Result of recipe matching
/// </summary>
public class RecipeMatchResult
{
    public bool IsMatched { get; set; }
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal Confidence { get; set; } // 0-100%
    public string MatchMethod { get; set; } = string.Empty; // "Exact", "Fuzzy", "Manual"
}
