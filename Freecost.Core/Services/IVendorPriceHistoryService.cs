using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface IVendorPriceHistoryService
{
    Task<VendorPriceHistoryReport> GetVendorPriceHistoryAsync(Guid ingredientId, int daysBack = 90);
    Task<List<VendorPriceTrend>> GetVendorPriceTrendsAsync(Guid locationId, int daysBack = 90);
    Task<VendorSwitchRecommendation?> GetVendorSwitchRecommendationAsync(Guid ingredientId);
}

/// <summary>
/// Historical price data for an ingredient across multiple vendors
/// </summary>
public class VendorPriceHistoryReport
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string CurrentVendor { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public List<VendorPriceHistory> VendorHistories { get; set; } = new();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Price history for a specific vendor
/// </summary>
public class VendorPriceHistory
{
    public string VendorName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal? AveragePrice { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? PriceStability { get; set; } // Coefficient of variation (%)
    public int PriceChangeCount { get; set; }
    public List<VendorPricePoint> PricePoints { get; set; } = new();
    public PriceTrendDirection Trend { get; set; }
}

/// <summary>
/// Single price data point for a vendor
/// </summary>
public class VendorPricePoint
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public decimal? ChangePercent { get; set; }
}

/// <summary>
/// Price trend analysis for a vendor over time
/// </summary>
public class VendorPriceTrend
{
    public string VendorName { get; set; } = string.Empty;
    public int IngredientsTracked { get; set; }
    public decimal AveragePriceIncrease { get; set; }
    public int PriceIncreaseCount { get; set; }
    public int PriceDecreaseCount { get; set; }
    public int StablePriceCount { get; set; }
    public VendorReliability Reliability { get; set; }
}

/// <summary>
/// Recommendation for switching vendors based on price history
/// </summary>
public class VendorSwitchRecommendation
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string CurrentVendor { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string RecommendedVendor { get; set; } = string.Empty;
    public decimal RecommendedVendorPrice { get; set; }
    public decimal PotentialSavings { get; set; }
    public decimal SavingsPercent { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal RecommendedVendorStability { get; set; }
    public bool IsStrongRecommendation { get; set; }
}

public enum PriceTrendDirection
{
    Unknown,
    Stable,
    Increasing,
    Decreasing,
    Volatile
}

public enum VendorReliability
{
    Unknown,
    Excellent,    // Stable prices, minimal increases
    Good,         // Mostly stable
    Fair,         // Some volatility
    Poor          // High volatility or frequent increases
}
