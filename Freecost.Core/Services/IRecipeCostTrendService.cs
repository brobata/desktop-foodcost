using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IRecipeCostTrendService
{
    Task<RecipeCostTrend> GetCostTrendAsync(Guid recipeId, int daysBack = 90);
    Task<List<RecipeCostDataPoint>> GetHistoricalCostDataAsync(Guid recipeId, int daysBack = 90);
}

public class RecipeCostTrend
{
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public decimal CurrentCost { get; set; }
    public decimal? AverageCost { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
    public decimal? CostChangePercent { get; set; }
    public TrendDirection Direction { get; set; }
    public List<RecipeCostDataPoint> DataPoints { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class RecipeCostDataPoint
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
}

public enum TrendDirection
{
    Stable,
    Increasing,
    Decreasing,
    Volatile
}
