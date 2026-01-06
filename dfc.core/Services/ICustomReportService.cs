using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface ICustomReportService
{
    Task<CustomReport> GenerateReportAsync(ReportDefinition definition, Guid locationId);
    Task<List<ReportTemplate>> GetAvailableTemplatesAsync();
    ReportDefinition CreateFromTemplate(ReportTemplate template);
}

/// <summary>
/// Definition of a custom report
/// </summary>
public class ReportDefinition
{
    public string Name { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> SelectedMetrics { get; set; } = new();
    public List<ReportFilter> Filters { get; set; } = new();
    public string GroupBy { get; set; } = string.Empty;
    public string SortBy { get; set; } = string.Empty;
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Generated custom report
/// </summary>
public class CustomReport
{
    public string Name { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public List<string> ColumnHeaders { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public Dictionary<string, decimal> Summary { get; set; } = new();
    public ReportType Type { get; set; }
}

/// <summary>
/// Filter for custom report
/// </summary>
public class ReportFilter
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Predefined report template
/// </summary>
public class ReportTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public List<string> DefaultMetrics { get; set; } = new();
    public string DefaultGroupBy { get; set; } = string.Empty;
    public string DefaultSortBy { get; set; } = string.Empty;
}

public enum ReportType
{
    Ingredients,
    Recipes,
    Entrees,
    CostAnalysis,
    Profitability,
    Waste,
    VendorComparison
}

public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    StartsWith
}
