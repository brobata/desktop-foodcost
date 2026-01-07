using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Microsoft.Extensions.Logging;

namespace Dfc.Desktop.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly ICustomReportService _reportService;
    private readonly ICurrentLocationService _locationService;
    private readonly ILogger<ReportsViewModel>? _logger;

    [ObservableProperty]
    private ObservableCollection<ReportTemplate> _templates = new();

    [ObservableProperty]
    private CustomReport? _generatedReport;

    [ObservableProperty]
    private ObservableCollection<dynamic> _reportData = new();

    [ObservableProperty]
    private bool _isGenerating;

    public ReportsViewModel(
        ICustomReportService reportService,
        ICurrentLocationService locationService,
        ILogger<ReportsViewModel>? logger = null)
    {
        _reportService = reportService;
        _locationService = locationService;
        _logger = logger;

        _ = LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            var templates = await _reportService.GetAvailableTemplatesAsync();
            Templates.Clear();
            foreach (var template in templates)
            {
                Templates.Add(template);
            }
            _logger?.LogInformation("Loaded {Count} report templates", templates.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load report templates");
        }
    }

    [RelayCommand]
    private async Task RefreshTemplates()
    {
        await LoadTemplatesAsync();
    }

    [RelayCommand]
    private async Task GenerateReport(ReportTemplate template)
    {
        if (template == null) return;

        IsGenerating = true;
        try
        {
            _logger?.LogInformation("Generating report: {ReportName}", template.Name);

            var definition = _reportService.CreateFromTemplate(template);
            var report = await _reportService.GenerateReportAsync(definition, _locationService.CurrentLocationId);

            GeneratedReport = report;

            // Convert report data to dynamic objects for DataGrid
            ReportData.Clear();
            foreach (var row in report.Rows)
            {
                var expandoObj = new ExpandoObject();
                var expandoDict = expandoObj as IDictionary<string, object?>;

                foreach (var kvp in row)
                {
                    expandoDict![kvp.Key] = kvp.Value ?? "";
                }

                ReportData.Add(expandoObj);
            }

            _logger?.LogInformation("Report generated successfully with {RowCount} rows", report.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate report");
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
