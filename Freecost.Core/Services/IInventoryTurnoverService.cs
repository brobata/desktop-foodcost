using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IInventoryTurnoverService
{
    Task<InventoryTurnoverReport> GenerateTurnoverReportAsync(Guid locationId, int daysBack = 90);
    Task<List<InventoryTurnoverMetrics>> GetSlowMovingItemsAsync(Guid locationId, int count = 20);
    Task<List<InventoryTurnoverMetrics>> GetFastMovingItemsAsync(Guid locationId, int count = 20);
}

/// <summary>
/// Complete inventory turnover analysis report
/// </summary>
public class InventoryTurnoverReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalIngredients { get; set; }
    public decimal AverageTurnoverRate { get; set; }
    public List<InventoryTurnoverMetrics> Items { get; set; } = new();
    public InventoryTurnoverSummary Summary { get; set; } = new();
}

/// <summary>
/// Turnover metrics for a single ingredient
/// </summary>
public class InventoryTurnoverMetrics
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int RecipeUsageCount { get; set; }
    public int EntreeUsageCount { get; set; }
    public int TotalUsageCount { get; set; }
    public decimal EstimatedQuantityUsed { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal EstimatedValueUsed { get; set; }
    public TurnoverRate TurnoverRate { get; set; }
    public string Analysis { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Summary statistics for inventory turnover
/// </summary>
public class InventoryTurnoverSummary
{
    public int FastMovingCount { get; set; }
    public int ModerateMovingCount { get; set; }
    public int SlowMovingCount { get; set; }
    public int NotUsedCount { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public List<string> TopCategories { get; set; } = new();
}

public enum TurnoverRate
{
    NotUsed,        // 0 usages
    Slow,           // 1-5 usages
    Moderate,       // 6-15 usages
    Fast            // 16+ usages
}
