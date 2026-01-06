using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IRecipeProfitabilityService
{
    Task<RecipeProfitabilityReport> GenerateProfitabilityReportAsync(Guid locationId);
    Task<List<RecipeProfitabilityMetrics>> GetTopProfitableRecipesAsync(Guid locationId, int count = 20);
    Task<List<RecipeProfitabilityMetrics>> GetLeastProfitableRecipesAsync(Guid locationId, int count = 20);
}

/// <summary>
/// Complete profitability report for all recipes
/// </summary>
public class RecipeProfitabilityReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public int TotalRecipes { get; set; }
    public decimal AverageFoodCostPercent { get; set; }
    public decimal AverageContributionMargin { get; set; }
    public List<RecipeProfitabilityMetrics> Recipes { get; set; } = new();
    public RecipeProfitabilitySummary Summary { get; set; } = new();
}

/// <summary>
/// Profitability metrics for a single recipe
/// </summary>
public class RecipeProfitabilityMetrics
{
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal CostPerServing { get; set; }
    public decimal? SuggestedPrice { get; set; }
    public decimal? ProfitMargin { get; set; }
    public decimal? ContributionMargin { get; set; }
    public decimal FoodCostPercent { get; set; }
    public ProfitabilityLevel ProfitabilityLevel { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Rank { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Summary statistics for profitability report
/// </summary>
public class RecipeProfitabilitySummary
{
    public int HighProfitabilityCount { get; set; }
    public int ModerateProfitabilityCount { get; set; }
    public int LowProfitabilityCount { get; set; }
    public int NoPricingDataCount { get; set; }
    public decimal TotalRecipeCosts { get; set; }
    public decimal? TotalPotentialRevenue { get; set; }
    public decimal? TotalProfitMargin { get; set; }
}

/// <summary>
/// Profitability classification levels
/// </summary>
public enum ProfitabilityLevel
{
    NoPricingData,      // No menu price available
    Low,                // Food cost > 35%
    Moderate,           // Food cost 25-35%
    High                // Food cost < 25%
}
