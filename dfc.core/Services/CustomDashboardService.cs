using Dfc.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class CustomDashboardService : ICustomDashboardService
{
    private readonly string _configPath;
    private List<CustomDashboard> _dashboards;

    public CustomDashboardService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );

        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "custom_dashboards.json");
        _dashboards = LoadDashboards();
    }

    public async Task<CustomDashboard> CreateDashboardAsync(CustomDashboard dashboard)
    {
        dashboard.Id = Guid.NewGuid();
        dashboard.CreatedAt = DateTime.UtcNow;
        dashboard.ModifiedAt = DateTime.UtcNow;

        _dashboards.Add(dashboard);
        await SaveDashboardsAsync();

        return dashboard;
    }

    public async Task<List<CustomDashboard>> GetAllDashboardsAsync()
    {
        return await Task.FromResult(_dashboards.OrderBy(d => d.SortOrder).ToList());
    }

    public async Task<CustomDashboard?> GetDashboardByIdAsync(Guid id)
    {
        return await Task.FromResult(_dashboards.FirstOrDefault(d => d.Id == id));
    }

    public async Task<CustomDashboard?> GetDefaultDashboardAsync()
    {
        return await Task.FromResult(_dashboards.FirstOrDefault(d => d.IsDefault));
    }

    public async Task UpdateDashboardAsync(CustomDashboard dashboard)
    {
        var existingIndex = _dashboards.FindIndex(d => d.Id == dashboard.Id);
        if (existingIndex >= 0)
        {
            dashboard.ModifiedAt = DateTime.UtcNow;
            _dashboards[existingIndex] = dashboard;
            await SaveDashboardsAsync();
        }
    }

    public async Task DeleteDashboardAsync(Guid id)
    {
        _dashboards.RemoveAll(d => d.Id == id);
        await SaveDashboardsAsync();
    }

    public async Task SetDefaultDashboardAsync(Guid id)
    {
        foreach (var dashboard in _dashboards)
        {
            dashboard.IsDefault = dashboard.Id == id;
        }

        await SaveDashboardsAsync();
    }

    public async Task<List<CustomDashboard>> GetDashboardTemplatesAsync()
    {
        var templates = new List<CustomDashboard>
        {
            new CustomDashboard
            {
                Name = "Executive Summary",
                Description = "High-level business metrics and KPIs",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        WidgetType = "stat",
                        Title = "Total Recipes",
                        DataSource = "recipe-count",
                        Row = 0, Column = 0, Width = 1, Height = 1
                    },
                    new DashboardWidget
                    {
                        WidgetType = "stat",
                        Title = "Total Entrees",
                        DataSource = "entree-count",
                        Row = 0, Column = 1, Width = 1, Height = 1
                    },
                    new DashboardWidget
                    {
                        WidgetType = "stat",
                        Title = "Avg Food Cost %",
                        DataSource = "avg-food-cost-percent",
                        Row = 0, Column = 2, Width = 1, Height = 1
                    },
                    new DashboardWidget
                    {
                        WidgetType = "chart",
                        Title = "Food Cost Trends",
                        DataSource = "food-cost-trends",
                        Row = 1, Column = 0, Width = 3, Height = 2
                    }
                }
            },
            new CustomDashboard
            {
                Name = "Cost Analysis",
                Description = "Detailed cost breakdown and analysis",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        WidgetType = "chart",
                        Title = "Top 10 Expensive Ingredients",
                        DataSource = "top-expensive-ingredients",
                        Row = 0, Column = 0, Width = 2, Height = 2
                    },
                    new DashboardWidget
                    {
                        WidgetType = "table",
                        Title = "Recent Price Changes",
                        DataSource = "recent-price-changes",
                        Row = 0, Column = 2, Width = 2, Height = 2
                    },
                    new DashboardWidget
                    {
                        WidgetType = "list",
                        Title = "High Food Cost % Entrees",
                        DataSource = "high-food-cost-entrees",
                        Row = 2, Column = 0, Width = 4, Height = 1
                    }
                }
            },
            new CustomDashboard
            {
                Name = "Kitchen Operations",
                Description = "Kitchen efficiency and prep metrics",
                Widgets = new List<DashboardWidget>
                {
                    new DashboardWidget
                    {
                        WidgetType = "chart",
                        Title = "Prep Time Distribution",
                        DataSource = "prep-time-distribution",
                        Row = 0, Column = 0, Width = 2, Height = 2
                    },
                    new DashboardWidget
                    {
                        WidgetType = "list",
                        Title = "Quick Recipes (<30 min)",
                        DataSource = "quick-recipes",
                        Row = 0, Column = 2, Width = 2, Height = 1
                    },
                    new DashboardWidget
                    {
                        WidgetType = "list",
                        Title = "Complex Recipes (Expert)",
                        DataSource = "complex-recipes",
                        Row = 1, Column = 2, Width = 2, Height = 1
                    }
                }
            }
        };

        return await Task.FromResult(templates);
    }

    private List<CustomDashboard> LoadDashboards()
    {
        if (!File.Exists(_configPath))
        {
            return new List<CustomDashboard>();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<List<CustomDashboard>>(json) ?? new List<CustomDashboard>();
        }
        catch
        {
            return new List<CustomDashboard>();
        }
    }

    private async Task SaveDashboardsAsync()
    {
        var json = JsonSerializer.Serialize(_dashboards, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_configPath, json);
    }
}
