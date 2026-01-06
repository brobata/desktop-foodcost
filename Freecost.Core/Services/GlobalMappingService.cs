using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Freecost.Core.Models;
using Microsoft.Extensions.Logging;

namespace Freecost.Core.Services;

/// <summary>
/// Service for managing global ingredient/recipe mappings
/// Simplified version using local storage only (Firebase removed)
/// </summary>
public interface IGlobalMappingService
{
    /// <summary>
    /// Load all global mappings (now just returns empty - using local data)
    /// </summary>
    Task<bool> LoadMappingsAsync(string? authToken = null);

    /// <summary>
    /// Get mapping for an import name
    /// </summary>
    GlobalIngredientMapping? GetMapping(string importName);

    /// <summary>
    /// Check if mappings have been loaded
    /// </summary>
    bool IsLoaded { get; }
}

public class GlobalMappingService : IGlobalMappingService
{
    private readonly ILogger<GlobalMappingService>? _logger;
    private Dictionary<string, GlobalIngredientMapping> _mappingCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoaded = false;

    public bool IsLoaded => _isLoaded;

    public GlobalMappingService(ILogger<GlobalMappingService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Load mappings (Firebase removed - using local/empty mappings)
    /// </summary>
    public async Task<bool> LoadMappingsAsync(string? authToken = null)
    {
        await Task.CompletedTask;

        Debug.WriteLine("[GlobalMappingService] Firebase loading removed - using local mappings");
        _logger?.LogInformation("Global mappings using local storage (Firebase removed)");

        _isLoaded = true;
        return true;
    }

    public GlobalIngredientMapping? GetMapping(string importName)
    {
        _mappingCache.TryGetValue(importName, out var mapping);
        return mapping;
    }
}
