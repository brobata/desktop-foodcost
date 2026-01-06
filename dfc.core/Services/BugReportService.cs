using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dfc.Core.Models;

namespace Dfc.Core.Services;

/// <summary>
/// Service for submitting bug reports to Supabase
/// </summary>
public interface IBugReportService
{
    /// <summary>
    /// Submit a bug report with user description and optional exception info
    /// </summary>
    Task<bool> SubmitBugReportAsync(string whatWereYouDoing, string? additionalNotes = null, Exception? exception = null);

    /// <summary>
    /// Get all bug reports (admin only)
    /// </summary>
    Task<System.Collections.Generic.List<BugReport>> GetAllBugReportsAsync();
}

public class BugReportService : IBugReportService
{
    private readonly IAuthenticationService _authService;
    private readonly ICurrentLocationService _currentLocationService;
    private readonly IIngredientService? _ingredientService;
    private readonly IRecipeService? _recipeService;
    private readonly IEntreeService? _entreeService;
    private readonly ILogger<BugReportService>? _logger;
    private readonly Supabase.Client _supabaseClient;

    public BugReportService(
        IAuthenticationService authService,
        ICurrentLocationService currentLocationService,
        Supabase.Client supabaseClient,
        ILogger<BugReportService>? logger = null,
        IIngredientService? ingredientService = null,
        IRecipeService? recipeService = null,
        IEntreeService? entreeService = null)
    {
        _authService = authService;
        _currentLocationService = currentLocationService;
        _supabaseClient = supabaseClient;
        _logger = logger;
        _ingredientService = ingredientService;
        _recipeService = recipeService;
        _entreeService = entreeService;
    }

    public async Task<bool> SubmitBugReportAsync(
        string whatWereYouDoing,
        string? additionalNotes = null,
        Exception? exception = null)
    {
        try
        {
            _logger?.LogInformation("Submitting bug report...");

            // Get current user info
            var currentUser = await _authService.GetCurrentUserAsync();

            // Gather diagnostic data
            var diagnosticData = await GatherDiagnosticsAsync();

            // Create bug report
            var bugReport = new BugReport
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,

                // User info
                UserId = currentUser?.Id,
                UserEmail = currentUser?.Email,
                RestaurantName = currentUser?.Location?.Name,
                LocationId = _currentLocationService.CurrentLocationId,

                // App info
                AppVersion = GetAppVersion(),
                BuildNumber = GetBuildNumber(),
                OsVersion = Environment.OSVersion.ToString(),

                // User input
                WhatWereYouDoing = whatWereYouDoing,
                AdditionalNotes = additionalNotes,

                // Exception info
                ErrorMessage = exception?.Message,
                StackTrace = exception?.StackTrace,

                // Full diagnostics
                DiagnosticJson = JsonSerializer.Serialize(diagnosticData, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            };

            // Submit to Supabase
            await _supabaseClient.From<BugReport>().Insert(bugReport);

            _logger?.LogInformation("Bug report submitted successfully: {Id}", bugReport.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to submit bug report");
            return false;
        }
    }

    private async Task<object> GatherDiagnosticsAsync()
    {
        int ingredientCount = 0;
        int recipeCount = 0;
        int entreeCount = 0;

        try
        {
            if (_ingredientService != null)
            {
                var ingredients = await _ingredientService.GetAllIngredientsAsync(_currentLocationService.CurrentLocationId);
                ingredientCount = ingredients.Count;
            }

            if (_recipeService != null)
            {
                var recipes = await _recipeService.GetAllRecipesAsync(_currentLocationService.CurrentLocationId);
                recipeCount = recipes.Count;
            }

            if (_entreeService != null)
            {
                var entrees = await _entreeService.GetAllEntreesAsync(_currentLocationService.CurrentLocationId);
                entreeCount = entrees.Count;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to gather some diagnostic data");
        }

        return new
        {
            // App info
            AppVersion = GetAppVersion(),
            BuildNumber = GetBuildNumber(),

            // System info
            OSVersion = Environment.OSVersion.ToString(),
            OSArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
            ProcessorCount = Environment.ProcessorCount,
            DotNetVersion = Environment.Version.ToString(),
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            CurrentDirectory = Environment.CurrentDirectory,
            WorkingSetMB = Environment.WorkingSet / 1024 / 1024,

            // App-specific data
            TotalIngredients = ingredientCount,
            TotalRecipes = recipeCount,
            TotalEntrees = entreeCount,
            DatabaseSizeMB = GetDatabaseSizeMB(),

            // State
            IsAuthenticated = _authService.IsAuthenticated,
            CurrentLocationId = _currentLocationService.CurrentLocationId,

            // Timestamp
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetAppVersion()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        return assembly?.GetName().Version?.ToString() ?? "Unknown";
    }

    private string GetBuildNumber()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        var infoVersion = assembly?.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();
        return infoVersion?.InformationalVersion ?? "Unknown";
    }

    private double GetDatabaseSizeMB()
    {
        try
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Desktop Food Cost",
                "freecost.db"
            );

            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                return Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get database size");
        }

        return 0;
    }

    public async Task<System.Collections.Generic.List<BugReport>> GetAllBugReportsAsync()
    {
        try
        {
            _logger?.LogInformation("Fetching all bug reports...");
            var result = await _supabaseClient
                .From<BugReport>()
                .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                .Get();

            return result.Models;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to fetch bug reports");
            return new System.Collections.Generic.List<BugReport>();
        }
    }
}
