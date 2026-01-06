using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IVendorComparisonService
{
    Task<VendorComparisonReport> CompareVendorsAsync(Guid locationId);
    Task<List<IngredientVendorComparison>> GetIngredientVendorOptionsAsync(Guid ingredientId);
    Task<decimal> CalculatePotentialSavingsAsync(Guid locationId);
}

/// <summary>
/// Complete vendor comparison report
/// </summary>
public class VendorComparisonReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public int TotalIngredients { get; set; }
    public int IngredientsWithMultipleVendors { get; set; }
    public decimal TotalCurrentCost { get; set; }
    public decimal PotentialSavings { get; set; }
    public decimal SavingsPercent { get; set; }
    public List<VendorSummary> VendorSummaries { get; set; } = new();
    public List<SavingsOpportunity> TopSavingsOpportunities { get; set; } = new();
}

/// <summary>
/// Summary stats for a single vendor
/// </summary>
public class VendorSummary
{
    public string VendorName { get; set; } = string.Empty;
    public int IngredientCount { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal AveragePrice { get; set; }
    public string PriceRanking { get; set; } = string.Empty; // "Lowest", "Competitive", "Highest"
}

/// <summary>
/// Specific ingredient where switching vendors would save money
/// </summary>
public class SavingsOpportunity
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string CurrentVendor { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string RecommendedVendor { get; set; } = string.Empty;
    public decimal RecommendedPrice { get; set; }
    public decimal SavingsAmount { get; set; }
    public decimal SavingsPercent { get; set; }
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Vendor pricing options for a specific ingredient
/// </summary>
public class IngredientVendorComparison
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string CurrentVendor { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public List<AlternativeVendorOption> AlternativeVendors { get; set; } = new();
}

/// <summary>
/// Alternative vendor pricing option
/// </summary>
public class AlternativeVendorOption
{
    public string VendorName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PriceDifference { get; set; }
    public decimal PriceDifferencePercent { get; set; }
    public bool IsLowerPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}
