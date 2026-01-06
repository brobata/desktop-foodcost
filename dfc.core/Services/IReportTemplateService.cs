using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IReportTemplateService
{
    /// <summary>
    /// Get all available report templates
    /// </summary>
    Task<List<DetailedReportTemplate>> GetAllTemplatesAsync();

    /// <summary>
    /// Get template by ID
    /// </summary>
    Task<DetailedReportTemplate?> GetTemplateByIdAsync(string id);

    /// <summary>
    /// Create custom template
    /// </summary>
    Task<DetailedReportTemplate> CreateTemplateAsync(DetailedReportTemplate template);

    /// <summary>
    /// Update template
    /// </summary>
    Task UpdateTemplateAsync(DetailedReportTemplate template);

    /// <summary>
    /// Delete template
    /// </summary>
    Task DeleteTemplateAsync(string id);

    /// <summary>
    /// Generate report from template
    /// </summary>
    Task<byte[]> GenerateReportFromTemplateAsync(string templateId, Dictionary<string, object> parameters);
}

public class DetailedReportTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Cost", "Operations", "Nutrition", etc.
    public List<ReportParameter> Parameters { get; set; } = new();
    public List<ReportSection> Sections { get; set; } = new();
    public string OutputFormat { get; set; } = "PDF"; // PDF, Excel, CSV
    public bool IsBuiltIn { get; set; }
}

public class ReportParameter
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // text, number, date, dropdown
    public bool Required { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; } // For dropdown type
}

public class ReportSection
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "table", "chart", "summary", "list"
    public string DataQuery { get; set; } = string.Empty; // What data to fetch
    public Dictionary<string, string> Formatting { get; set; } = new();
}
