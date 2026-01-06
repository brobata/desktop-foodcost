using System.Collections.Generic;

namespace Dfc.Core.Models;

public class CustomDashboard : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DashboardWidget> Widgets { get; set; } = new();
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class DashboardWidget
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string WidgetType { get; set; } = string.Empty; // "chart", "stat", "table", "list"
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty; // What data to display
    public int Row { get; set; }
    public int Column { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public Dictionary<string, string> Settings { get; set; } = new();
}
