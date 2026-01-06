using Dfc.Core.Enums;
using Dfc.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

/// <summary>
/// Loads and manages the built-in conversion database from StandardConversions.json.
/// Provides fast lookup for density profiles and standard conversions.
/// </summary>
public class ConversionDatabaseService
{
    private readonly ILogger<ConversionDatabaseService>? _logger;
    private List<DensityProfile> _densityProfiles = new();
    private List<StandardConversion> _standardConversions = new();
    private bool _isInitialized = false;

    public ConversionDatabaseService(ILogger<ConversionDatabaseService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads conversion data from StandardConversions.json.
    /// Call this once at application startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            _logger?.LogDebug("ConversionDatabaseService already initialized, skipping");
            return;
        }

        try
        {
            // Find the JSON file
            var jsonPath = FindConversionDatabaseFile();
            if (jsonPath == null)
            {
                _logger?.LogWarning("StandardConversions.json not found, conversion database will be empty");
                _isInitialized = true;
                return;
            }

            _logger?.LogInformation("Loading conversion database from: {Path}", jsonPath);

            // Load and parse JSON
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var database = JsonSerializer.Deserialize<ConversionDatabase>(jsonContent, options);
            if (database == null)
            {
                _logger?.LogError("Failed to deserialize StandardConversions.json");
                _isInitialized = true;
                return;
            }

            // Convert to runtime models
            _densityProfiles = database.DensityProfiles?.Select(dto => new DensityProfile
            {
                Category = dto.Category ?? "",
                Keywords = dto.Keywords ?? Array.Empty<string>(),
                GramsPerCup = dto.GramsPerCup,
                GramsPerTablespoon = dto.GramsPerTablespoon,
                GramsPerTeaspoon = dto.GramsPerTeaspoon,
                GramsPerFluidOunce = dto.GramsPerFluidOunce,
                Source = dto.Source ?? "BuiltIn",
                Notes = dto.Notes
            }).ToList() ?? new List<DensityProfile>();

            _standardConversions = database.StandardConversions?.Select(dto => new StandardConversion
            {
                Keywords = dto.Keywords ?? Array.Empty<string>(),
                FromQuantity = dto.FromQuantity,
                FromUnit = (UnitType)dto.FromUnit,
                ToQuantity = dto.ToQuantity,
                ToUnit = (UnitType)dto.ToUnit,
                Source = dto.Source ?? "BuiltIn",
                Notes = dto.Notes,
                IsDefault = dto.IsDefault
            }).ToList() ?? new List<StandardConversion>();

            _isInitialized = true;
            _logger?.LogInformation(
                "Conversion database loaded successfully: {DensityCount} density profiles, {ConversionCount} standard conversions",
                _densityProfiles.Count,
                _standardConversions.Count
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading conversion database");
            _isInitialized = true; // Mark as initialized to prevent repeated attempts
        }
    }

    /// <summary>
    /// Finds the StandardConversions.json file in various possible locations.
    /// </summary>
    private string? FindConversionDatabaseFile()
    {
        var possiblePaths = new[]
        {
            // Running from bin/Debug or bin/Release
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "StandardConversions.json"),

            // Running from project root
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "StandardConversions.json"),

            // Development - Core project
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Dfc.Core", "Data", "StandardConversions.json"),

            // Relative to this assembly
            Path.Combine(Path.GetDirectoryName(typeof(ConversionDatabaseService).Assembly.Location) ?? "", "Data", "StandardConversions.json")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a density profile that matches the ingredient name.
    /// Uses keyword matching (case-insensitive).
    /// </summary>
    public DensityProfile? FindDensityProfile(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || !_isInitialized)
            return null;

        return _densityProfiles.FirstOrDefault(profile => profile.MatchesIngredient(ingredientName));
    }

    /// <summary>
    /// Gets a standard conversion for an ingredient from one unit to another.
    /// Returns the default conversion if multiple matches exist.
    /// </summary>
    public StandardConversion? GetStandardConversion(string ingredientName, UnitType fromUnit, UnitType toUnit)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || !_isInitialized)
            return null;

        var lowerName = ingredientName.ToLowerInvariant();

        // Find all matching conversions
        var matches = _standardConversions
            .Where(c => c.FromUnit == fromUnit && c.ToUnit == toUnit)
            .Where(c => c.Keywords.Any(keyword =>
                lowerName.Contains(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (!matches.Any())
            return null;

        // Prefer default conversion if available
        return matches.FirstOrDefault(c => c.IsDefault) ?? matches.First();
    }

    /// <summary>
    /// Finds all standard conversions that match the ingredient name.
    /// Uses keyword matching (case-insensitive).
    /// </summary>
    public List<StandardConversion> FindStandardConversions(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || !_isInitialized)
            return new List<StandardConversion>();

        var lowerName = ingredientName.ToLowerInvariant();

        // Find all conversions where at least one keyword matches the ingredient name
        return _standardConversions
            .Where(c => c.Keywords.Any(keyword =>
                lowerName.Contains(keyword.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Calculates volume-to-weight conversion using density profiles.
    /// Example: 1 Cup flour → 120 grams
    /// </summary>
    public decimal? ConvertVolumeToWeight(
        string ingredientName,
        decimal quantity,
        UnitType volumeUnit,
        UnitType weightUnit)
    {
        var profile = FindDensityProfile(ingredientName);
        if (profile == null)
            return null;

        // Get grams for the volume unit
        var gramsPerUnit = profile.GetDensityForUnit(volumeUnit);
        if (!gramsPerUnit.HasValue)
            return null;

        // Calculate total grams
        var totalGrams = quantity * gramsPerUnit.Value;

        // Convert from grams to target weight unit
        var unitConverter = new UnitConversionService();
        if (unitConverter.CanConvert(UnitType.Gram, weightUnit))
        {
            return unitConverter.Convert(totalGrams, UnitType.Gram, weightUnit);
        }

        return null;
    }

    /// <summary>
    /// Gets all density profiles (for display in settings).
    /// </summary>
    public IReadOnlyList<DensityProfile> GetAllDensityProfiles()
    {
        return _densityProfiles.AsReadOnly();
    }

    /// <summary>
    /// Gets all standard conversions (for display in settings).
    /// </summary>
    public IReadOnlyList<StandardConversion> GetAllStandardConversions()
    {
        return _standardConversions.AsReadOnly();
    }

    /// <summary>
    /// Returns statistics about the loaded database.
    /// </summary>
    public (int DensityProfiles, int StandardConversions) GetStatistics()
    {
        return (_densityProfiles.Count, _standardConversions.Count);
    }

    // DTO classes for JSON deserialization
    private class ConversionDatabase
    {
        [JsonPropertyName("densityProfiles")]
        public List<DensityProfileDto>? DensityProfiles { get; set; }

        [JsonPropertyName("standardConversions")]
        public List<StandardConversionDto>? StandardConversions { get; set; }
    }

    private class DensityProfileDto
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("keywords")]
        public string[]? Keywords { get; set; }

        [JsonPropertyName("gramsPerCup")]
        public decimal? GramsPerCup { get; set; }

        [JsonPropertyName("gramsPerTablespoon")]
        public decimal? GramsPerTablespoon { get; set; }

        [JsonPropertyName("gramsPerTeaspoon")]
        public decimal? GramsPerTeaspoon { get; set; }

        [JsonPropertyName("gramsPerFluidOunce")]
        public decimal? GramsPerFluidOunce { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    private class StandardConversionDto
    {
        [JsonPropertyName("keywords")]
        public string[]? Keywords { get; set; }

        [JsonPropertyName("fromQuantity")]
        public decimal FromQuantity { get; set; }

        [JsonPropertyName("fromUnit")]
        public int FromUnit { get; set; }

        [JsonPropertyName("toQuantity")]
        public decimal ToQuantity { get; set; }

        [JsonPropertyName("toUnit")]
        public int ToUnit { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
    }
}

/// <summary>
/// Represents a standard conversion from the built-in database.
/// Similar to IngredientConversion but in-memory only.
/// </summary>
public class StandardConversion
{
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public decimal FromQuantity { get; set; }
    public UnitType FromUnit { get; set; }
    public decimal ToQuantity { get; set; }
    public UnitType ToUnit { get; set; }
    public string Source { get; set; } = "BuiltIn";
    public string? Notes { get; set; }
    public bool IsDefault { get; set; }

    /// <summary>
    /// Applies this conversion to a quantity.
    /// </summary>
    public decimal ApplyConversion(decimal quantity)
    {
        return quantity * (ToQuantity / FromQuantity);
    }

    /// <summary>
    /// Display text for UI.
    /// </summary>
    public string DisplayText => $"{FromQuantity} {FromUnit} = {ToQuantity} {ToUnit}";
}
