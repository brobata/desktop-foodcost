using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class ReportTemplateService : IReportTemplateService
{
    private readonly string _configPath;
    private List<DetailedReportTemplate> _templates;

    public ReportTemplateService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );

        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "report_templates.json");
        _templates = LoadTemplates();

        // Add built-in templates if none exist
        if (!_templates.Any(t => t.IsBuiltIn))
        {
            _templates.AddRange(GetBuiltInTemplates());
        }
    }

    public async Task<List<DetailedReportTemplate>> GetAllTemplatesAsync()
    {
        return await Task.FromResult(_templates.ToList());
    }

    public async Task<DetailedReportTemplate?> GetTemplateByIdAsync(string id)
    {
        return await Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));
    }

    public async Task<DetailedReportTemplate> CreateTemplateAsync(DetailedReportTemplate template)
    {
        template.Id = Guid.NewGuid().ToString();
        template.IsBuiltIn = false;

        _templates.Add(template);
        await SaveTemplatesAsync();

        return template;
    }

    public async Task UpdateTemplateAsync(DetailedReportTemplate template)
    {
        var existingIndex = _templates.FindIndex(t => t.Id == template.Id);
        if (existingIndex >= 0 && !_templates[existingIndex].IsBuiltIn)
        {
            _templates[existingIndex] = template;
            await SaveTemplatesAsync();
        }
    }

    public async Task DeleteTemplateAsync(string id)
    {
        _templates.RemoveAll(t => t.Id == id && !t.IsBuiltIn);
        await SaveTemplatesAsync();
    }

    public async Task<byte[]> GenerateReportFromTemplateAsync(string templateId, Dictionary<string, object> parameters)
    {
        var template = await GetTemplateByIdAsync(templateId);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID {templateId} not found");
        }

        // TODO: Implement actual report generation based on template
        // This would integrate with existing report services and QuestPDF

        return await Task.FromResult(Array.Empty<byte>());
    }

    private List<DetailedReportTemplate> GetBuiltInTemplates()
    {
        return new List<DetailedReportTemplate>
        {
            new DetailedReportTemplate
            {
                Id = "monthly-cost-summary",
                Name = "Monthly Cost Summary",
                Description = "Comprehensive monthly cost analysis report",
                Category = "Cost",
                IsBuiltIn = true,
                Parameters = new List<ReportParameter>
                {
                    new ReportParameter
                    {
                        Name = "month",
                        Label = "Month",
                        Type = "date",
                        Required = true
                    },
                    new ReportParameter
                    {
                        Name = "includeCharts",
                        Label = "Include Charts",
                        Type = "dropdown",
                        Options = new List<string> { "Yes", "No" },
                        DefaultValue = "Yes"
                    }
                },
                Sections = new List<ReportSection>
                {
                    new ReportSection
                    {
                        Title = "Cost Overview",
                        Type = "summary",
                        DataQuery = "total-costs-by-month"
                    },
                    new ReportSection
                    {
                        Title = "Top 10 Expensive Ingredients",
                        Type = "table",
                        DataQuery = "top-ingredients-by-cost"
                    },
                    new ReportSection
                    {
                        Title = "Cost Trends",
                        Type = "chart",
                        DataQuery = "monthly-cost-trends"
                    }
                }
            },
            new DetailedReportTemplate
            {
                Id = "recipe-profitability",
                Name = "Recipe Profitability Report",
                Description = "Analyze recipe costs and profitability",
                Category = "Cost",
                IsBuiltIn = true,
                Parameters = new List<ReportParameter>
                {
                    new ReportParameter
                    {
                        Name = "category",
                        Label = "Recipe Category",
                        Type = "dropdown",
                        Options = new List<string> { "All", "Appetizers", "Entrees", "Desserts" },
                        DefaultValue = "All"
                    },
                    new ReportParameter
                    {
                        Name = "sortBy",
                        Label = "Sort By",
                        Type = "dropdown",
                        Options = new List<string> { "Cost", "Profitability", "Name" },
                        DefaultValue = "Profitability"
                    }
                },
                Sections = new List<ReportSection>
                {
                    new ReportSection
                    {
                        Title = "Recipe Profitability Analysis",
                        Type = "table",
                        DataQuery = "recipe-profitability-data"
                    }
                }
            },
            new DetailedReportTemplate
            {
                Id = "vendor-spending",
                Name = "Vendor Spending Report",
                Description = "Track spending by vendor",
                Category = "Vendor",
                IsBuiltIn = true,
                Parameters = new List<ReportParameter>
                {
                    new ReportParameter
                    {
                        Name = "dateRange",
                        Label = "Date Range",
                        Type = "dropdown",
                        Options = new List<string> { "Last 30 Days", "Last 90 Days", "Last Year" },
                        DefaultValue = "Last 30 Days"
                    }
                },
                Sections = new List<ReportSection>
                {
                    new ReportSection
                    {
                        Title = "Spending by Vendor",
                        Type = "chart",
                        DataQuery = "vendor-spending-breakdown"
                    },
                    new ReportSection
                    {
                        Title = "Vendor Details",
                        Type = "table",
                        DataQuery = "vendor-spending-details"
                    }
                }
            },
            new DetailedReportTemplate
            {
                Id = "nutritional-compliance",
                Name = "Nutritional Compliance Report",
                Description = "Review nutritional goals and compliance",
                Category = "Nutrition",
                IsBuiltIn = true,
                Parameters = new List<ReportParameter>
                {
                    new ReportParameter
                    {
                        Name = "dietaryFilter",
                        Label = "Dietary Filter",
                        Type = "dropdown",
                        Options = new List<string> { "All", "Vegan", "Vegetarian", "Gluten-Free" },
                        DefaultValue = "All"
                    }
                },
                Sections = new List<ReportSection>
                {
                    new ReportSection
                    {
                        Title = "Compliance Summary",
                        Type = "summary",
                        DataQuery = "diet-compliance-summary"
                    },
                    new ReportSection
                    {
                        Title = "Recipe Compliance Details",
                        Type = "table",
                        DataQuery = "recipe-compliance-details"
                    }
                }
            },
            new DetailedReportTemplate
            {
                Id = "waste-analysis",
                Name = "Waste Analysis Report",
                Description = "Track and analyze food waste",
                Category = "Operations",
                IsBuiltIn = true,
                Parameters = new List<ReportParameter>
                {
                    new ReportParameter
                    {
                        Name = "period",
                        Label = "Time Period",
                        Type = "dropdown",
                        Options = new List<string> { "Last Week", "Last Month", "Last Quarter" },
                        DefaultValue = "Last Month"
                    }
                },
                Sections = new List<ReportSection>
                {
                    new ReportSection
                    {
                        Title = "Waste by Reason",
                        Type = "chart",
                        DataQuery = "waste-by-reason"
                    },
                    new ReportSection
                    {
                        Title = "Top Wasted Ingredients",
                        Type = "table",
                        DataQuery = "top-wasted-ingredients"
                    },
                    new ReportSection
                    {
                        Title = "Recommendations",
                        Type = "list",
                        DataQuery = "waste-recommendations"
                    }
                }
            }
        };
    }

    private List<DetailedReportTemplate> LoadTemplates()
    {
        if (!File.Exists(_configPath))
        {
            return new List<DetailedReportTemplate>();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<List<DetailedReportTemplate>>(json) ?? new List<DetailedReportTemplate>();
        }
        catch
        {
            return new List<DetailedReportTemplate>();
        }
    }

    private async Task SaveTemplatesAsync()
    {
        // Only save custom templates, not built-in ones
        var customTemplates = _templates.Where(t => !t.IsBuiltIn).ToList();
        var json = JsonSerializer.Serialize(customTemplates, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_configPath, json);
    }
}
