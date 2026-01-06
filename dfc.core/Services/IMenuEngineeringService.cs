using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IMenuEngineeringService
{
    Task<MenuEngineeringAnalysis> AnalyzeMenuAsync(Guid locationId);
    MenuItemClassification ClassifyMenuItem(decimal contributionMargin, decimal popularity, decimal avgMargin, decimal avgPopularity);
}

/// <summary>
/// Complete menu engineering analysis results
/// </summary>
public class MenuEngineeringAnalysis
{
    public decimal AverageContributionMargin { get; set; }
    public decimal AveragePopularity { get; set; }
    public List<MenuItemAnalysis> Items { get; set; } = new();
    public MenuEngineeringSummary Summary { get; set; } = new();
}

/// <summary>
/// Analysis for a single menu item (entree)
/// </summary>
public class MenuItemAnalysis
{
    public Guid EntreeId { get; set; }
    public string EntreeName { get; set; } = string.Empty;
    public decimal MenuPrice { get; set; }
    public decimal FoodCost { get; set; }
    public decimal ContributionMargin { get; set; }
    public decimal FoodCostPercent { get; set; }
    public decimal Popularity { get; set; } // This would come from sales data in future
    public MenuItemClassification Classification { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Summary statistics for menu engineering
/// </summary>
public class MenuEngineeringSummary
{
    public int StarsCount { get; set; }
    public int PlowHorsesCount { get; set; }
    public int PuzzlesCount { get; set; }
    public int DogsCount { get; set; }
    public string OverallHealth { get; set; } = string.Empty;
}

/// <summary>
/// Menu Engineering Matrix classification (Miller, 1980)
/// </summary>
public enum MenuItemClassification
{
    /// <summary>
    /// High profitability, High popularity - Keep and promote
    /// </summary>
    Star,

    /// <summary>
    /// Low profitability, High popularity - Increase prices or reduce costs
    /// </summary>
    PlowHorse,

    /// <summary>
    /// High profitability, Low popularity - Reposition, promote, or improve
    /// </summary>
    Puzzle,

    /// <summary>
    /// Low profitability, Low popularity - Consider removal
    /// </summary>
    Dog
}
