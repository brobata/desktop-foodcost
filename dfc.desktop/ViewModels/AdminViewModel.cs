using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Dfc.Core.Services;
using Dfc.Core.Repositories;
using Dfc.Core.Models;
using Dfc.Desktop.Views;
using Dfc.Desktop.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSessionService? _sessionService;
    private readonly IBugReportService? _bugReportService;
    private readonly ILogger<AdminViewModel>? _logger;
    private Window? _ownerWindow;

    [ObservableProperty]
    private ObservableCollection<BugReport> _bugReports = new();

    [ObservableProperty]
    private BugReport? _selectedBugReport;

    [ObservableProperty]
    private bool _isLoadingBugReports;

    public AdminViewModel(
        IServiceProvider serviceProvider,
        IUserSessionService? sessionService = null,
        IBugReportService? bugReportService = null,
        ILogger<AdminViewModel>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _sessionService = sessionService;
        _bugReportService = bugReportService;
        _logger = logger;

        _ = LoadBugReportsAsync();
    }

    public void SetOwnerWindow(Window window)
    {
        _ownerWindow = window;
    }

    [RelayCommand]
    private async Task ManageApprovals()
    {
        if (_ownerWindow == null)
        {
            _logger?.LogWarning("Cannot manage approvals: window not set");
            return;
        }

        try
        {
            var workflowRepository = _serviceProvider.GetRequiredService<IApprovalWorkflowRepository>();
            var workflowService = _serviceProvider.GetRequiredService<IApprovalWorkflowService>();
            var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
            var sessionService = _serviceProvider.GetRequiredService<IUserSessionService>();

            ApprovalWorkflowsWindow? window = null;
            var viewModel = new ApprovalWorkflowsViewModel(
                workflowRepository,
                workflowService,
                permissionService,
                sessionService,
                () => window?.Close()
            );

            window = new ApprovalWorkflowsWindow
            {
                DataContext = viewModel
            };

            await window.ShowDialog(_ownerWindow);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error opening approval workflows dialog");
        }
    }

    [RelayCommand]
    private async Task ResetTutorialProgress()
    {
        try
        {
            var progressTracker = _serviceProvider.GetService<ITutorialProgressTracker>();
            if (progressTracker == null)
            {
                _logger?.LogWarning("Tutorial progress tracker service not available");
                return;
            }

            await progressTracker.ResetProgressAsync();
            _logger?.LogInformation("Tutorial progress reset successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error resetting tutorial progress");
        }
    }

    private async Task LoadBugReportsAsync()
    {
        if (_bugReportService == null)
        {
            _logger?.LogWarning("Bug report service not available");
            return;
        }

        IsLoadingBugReports = true;
        try
        {
            var reports = await _bugReportService.GetAllBugReportsAsync();
            BugReports.Clear();
            foreach (var report in reports)
            {
                BugReports.Add(report);
            }
            _logger?.LogInformation("Loaded {Count} bug reports", reports.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load bug reports");
        }
        finally
        {
            IsLoadingBugReports = false;
        }
    }

    [RelayCommand]
    private async Task RefreshBugReports()
    {
        await LoadBugReportsAsync();
    }
}
