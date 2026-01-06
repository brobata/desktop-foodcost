using Freecost.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freecost.Core.Services;

public interface ICostVarianceService
{
    Task<CostVarianceReport> GenerateVarianceReportAsync(Guid locationId, int daysBack = 30);
    Task<List<IngredientVariance>> GetTopVariancesAsync(Guid locationId, int count = 20);
}

/// <summary>
/// Complete cost variance analysis report
/// </summary>
public class CostVarianceReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int IngredientsAnalyzed { get; set; }
    public decimal TotalExpectedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal TotalVariancePercent { get; set; }
    public int FavorableCount { get; set; }
    public int UnfavorableCount { get; set; }
    public List<IngredientVariance> Variances { get; set; } = new();
}

/// <summary>
/// Cost variance for a single ingredient
/// </summary>
public class IngredientVariance
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal ExpectedCost { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
    public VarianceType VarianceType { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime LastPriceChange { get; set; }
}

/// <summary>
/// Type of variance
/// </summary>
public enum VarianceType
{
    Favorable,      // Actual cost is lower than expected
    Unfavorable,    // Actual cost is higher than expected
    OnTarget        // Actual cost matches expected (within 2%)
}
