using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface ISeasonalTrendService
{
    Task<SeasonalTrendReport> AnalyzeSeasonalTrendsAsync(Guid locationId, int yearsBack = 2);
    Task<List<IngredientSeasonalPattern>> GetSeasonalPatternsAsync(Guid ingredientId);
    Task<List<SeasonalRecommendation>> GetSeasonalRecommendationsAsync(Guid locationId);
}

/// <summary>
/// Complete seasonal trend analysis
/// </summary>
public class SeasonalTrendReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public int YearsAnalyzed { get; set; }
    public List<IngredientSeasonalAnalysis> Ingredients { get; set; } = new();
    public SeasonalSummary Summary { get; set; } = new();
}

/// <summary>
/// Seasonal analysis for a single ingredient
/// </summary>
public class IngredientSeasonalAnalysis
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<SeasonalData> SeasonalData { get; set; } = new();
    public Season LowestPriceSeason { get; set; }
    public Season HighestPriceSeason { get; set; }
    public decimal SeasonalVariance { get; set; } // % difference between high and low seasons
    public bool HasStrongSeasonalPattern { get; set; }
}

/// <summary>
/// Price data for a season
/// </summary>
public class SeasonalData
{
    public Season Season { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int DataPoints { get; set; }
}

/// <summary>
/// Pattern of price changes across seasons
/// </summary>
public class IngredientSeasonalPattern
{
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Season> BestBuyingSeasons { get; set; } = new();
    public List<Season> AvoidSeasons { get; set; } = new();
}

/// <summary>
/// Recommendation based on seasonal trends
/// </summary>
public class SeasonalRecommendation
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public Season CurrentSeason { get; set; }
    public Season BestBuyingSeason { get; set; }
    public decimal PotentialSavings { get; set; }
    public string ActionItem { get; set; } = string.Empty;
}

/// <summary>
/// Summary of seasonal trends
/// </summary>
public class SeasonalSummary
{
    public int IngredientsWithStrongSeasonality { get; set; }
    public int IngredientsWithWeakSeasonality { get; set; }
    public Season MostExpensiveSeason { get; set; }
    public Season LeastExpensiveSeason { get; set; }
    public decimal AverageSeasonalVariance { get; set; }
}

public enum Season
{
    Spring,  // Mar, Apr, May
    Summer,  // Jun, Jul, Aug
    Fall,    // Sep, Oct, Nov
    Winter   // Dec, Jan, Feb
}
