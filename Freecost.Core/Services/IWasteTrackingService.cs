using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IWasteTrackingService
{
    Task<WasteRecord> RecordWasteAsync(WasteRecord wasteRecord);
    Task<WasteReport> GenerateWasteReportAsync(Guid locationId, int daysBack = 30);
    Task<List<IngredientWasteSummary>> GetTopWastedIngredientsAsync(Guid locationId, int count = 20);
    Task<Dictionary<WasteReason, decimal>> GetWasteByReasonAsync(Guid locationId, int daysBack = 30);
}

/// <summary>
/// Complete waste analysis report
/// </summary>
public class WasteReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalWasteRecords { get; set; }
    public decimal TotalWasteCost { get; set; }
    public decimal AverageDailyCost { get; set; }
    public List<IngredientWasteSummary> IngredientWaste { get; set; } = new();
    public Dictionary<WasteReason, WasteReasonSummary> WasteByReason { get; set; } = new();
    public WasteTrend Trend { get; set; }
}

/// <summary>
/// Waste summary for a single ingredient
/// </summary>
public class IngredientWasteSummary
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalQuantityWasted { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int WasteCount { get; set; }
    public WasteReason MostCommonReason { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Summary for a specific waste reason
/// </summary>
public class WasteReasonSummary
{
    public WasteReason Reason { get; set; }
    public int Count { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Percentage { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public enum WasteTrend
{
    Improving,
    Stable,
    Worsening
}
