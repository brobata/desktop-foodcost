using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Repositories;
using Dfc.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class ApprovalWorkflowsViewModel : ViewModelBase
{
    private readonly IApprovalWorkflowRepository _workflowRepository;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly IPermissionService _permissionService;
    private readonly IUserSessionService _sessionService;
    private readonly Action? _onClose;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<ApprovalWorkflowDisplayModel> _workflows = new();

    [ObservableProperty]
    private ApprovalWorkflowDisplayModel? _selectedWorkflow;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private string _reviewNotes = string.Empty;

    [ObservableProperty]
    private ApprovalStatus? _filterStatus;

    [ObservableProperty]
    private bool _showMySubmissions = false;

    public User? CurrentUser { get; private set; }

    public ObservableCollection<ApprovalStatus?> StatusFilterOptions { get; } = new()
    {
        null, // All
        ApprovalStatus.Pending,
        ApprovalStatus.Approved,
        ApprovalStatus.Rejected,
        ApprovalStatus.NeedsRevision,
        ApprovalStatus.Cancelled
    };

    public ApprovalWorkflowsViewModel(
        IApprovalWorkflowRepository workflowRepository,
        IApprovalWorkflowService workflowService,
        IPermissionService permissionService,
        IUserSessionService sessionService,
        Action? onClose = null)
    {
        _workflowRepository = workflowRepository;
        _workflowService = workflowService;
        _permissionService = permissionService;
        _sessionService = sessionService;
        _onClose = onClose;

        CurrentUser = _sessionService.CurrentUser;

        _ = LoadWorkflowsAsync();
    }

    private async Task LoadWorkflowsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Get workflows based on filter
            List<ApprovalWorkflow> allWorkflows;

            if (FilterStatus.HasValue)
            {
                allWorkflows = await _workflowRepository.GetByStatusAsync(FilterStatus.Value);
            }
            else
            {
                // Get all statuses in parallel for better performance
                var tasks = new[]
                {
                    _workflowRepository.GetByStatusAsync(ApprovalStatus.Pending),
                    _workflowRepository.GetByStatusAsync(ApprovalStatus.Approved),
                    _workflowRepository.GetByStatusAsync(ApprovalStatus.Rejected),
                    _workflowRepository.GetByStatusAsync(ApprovalStatus.NeedsRevision),
                    _workflowRepository.GetByStatusAsync(ApprovalStatus.Cancelled)
                };

                var results = await Task.WhenAll(tasks);

                allWorkflows = new List<ApprovalWorkflow>();
                foreach (var result in results)
                {
                    allWorkflows.AddRange(result);
                }
            }

            // Apply user filter
            var filteredWorkflows = allWorkflows.AsEnumerable();

            if (ShowMySubmissions && CurrentUser != null)
            {
                filteredWorkflows = filteredWorkflows.Where(w => w.SubmittedByUserId == CurrentUser.Id);
            }

            Workflows.Clear();
            foreach (var workflow in filteredWorkflows.OrderByDescending(w => w.SubmittedAt))
            {
                Workflows.Add(new ApprovalWorkflowDisplayModel(workflow));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load workflows: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshWorkflows()
    {
        await LoadWorkflowsAsync();
    }

    [RelayCommand]
    private async Task ApproveWorkflow(ApprovalWorkflowDisplayModel? workflowModel)
    {
        if (workflowModel == null) return;

        if (CurrentUser == null)
        {
            ErrorMessage = "You must be logged in to approve workflows";
            return;
        }

        // Check permission
        if (!await _permissionService.CanApproveAsync(CurrentUser))
        {
            ErrorMessage = "You don't have permission to approve workflows";
            return;
        }

        // Don't allow approving non-pending workflows
        if (workflowModel.Workflow.Status != ApprovalStatus.Pending)
        {
            ErrorMessage = $"Cannot approve a workflow with status '{workflowModel.Status}'";
            return;
        }

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            await _workflowService.ApproveAsync(
                workflowModel.Workflow.Id,
                CurrentUser.Id,
                string.IsNullOrWhiteSpace(ReviewNotes) ? null : ReviewNotes);

            SuccessMessage = $"Approved '{workflowModel.EntityName}'";
            ReviewNotes = string.Empty;

            // Remove from collection if filtering by status
            if (FilterStatus == ApprovalStatus.Pending)
            {
                Workflows.Remove(workflowModel);
            }
            else
            {
                await LoadWorkflowsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to approve workflow: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RejectWorkflow(ApprovalWorkflowDisplayModel? workflowModel)
    {
        if (workflowModel == null) return;

        if (CurrentUser == null)
        {
            ErrorMessage = "You must be logged in to reject workflows";
            return;
        }

        // Check permission
        if (!await _permissionService.CanApproveAsync(CurrentUser))
        {
            ErrorMessage = "You don't have permission to reject workflows";
            return;
        }

        // Don't allow rejecting non-pending workflows
        if (workflowModel.Workflow.Status != ApprovalStatus.Pending)
        {
            ErrorMessage = $"Cannot reject a workflow with status '{workflowModel.Status}'";
            return;
        }

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            await _workflowService.RejectAsync(
                workflowModel.Workflow.Id,
                CurrentUser.Id,
                string.IsNullOrWhiteSpace(ReviewNotes) ? null : ReviewNotes);

            SuccessMessage = $"Rejected '{workflowModel.EntityName}'";
            ReviewNotes = string.Empty;

            // Remove from collection if filtering by status
            if (FilterStatus == ApprovalStatus.Pending)
            {
                Workflows.Remove(workflowModel);
            }
            else
            {
                await LoadWorkflowsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to reject workflow: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RequestRevision(ApprovalWorkflowDisplayModel? workflowModel)
    {
        if (workflowModel == null) return;

        if (CurrentUser == null)
        {
            ErrorMessage = "You must be logged in to request revisions";
            return;
        }

        // Check permission
        if (!await _permissionService.CanApproveAsync(CurrentUser))
        {
            ErrorMessage = "You don't have permission to request revisions";
            return;
        }

        // Don't allow requesting revision on non-pending workflows
        if (workflowModel.Workflow.Status != ApprovalStatus.Pending)
        {
            ErrorMessage = $"Cannot request revision for a workflow with status '{workflowModel.Status}'";
            return;
        }

        if (string.IsNullOrWhiteSpace(ReviewNotes))
        {
            ErrorMessage = "Please provide review notes explaining what needs to be revised";
            return;
        }

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            await _workflowService.RequestRevisionAsync(
                workflowModel.Workflow.Id,
                CurrentUser.Id,
                ReviewNotes);

            SuccessMessage = $"Requested revision for '{workflowModel.EntityName}'";
            ReviewNotes = string.Empty;

            // Remove from collection if filtering by status
            if (FilterStatus == ApprovalStatus.Pending)
            {
                Workflows.Remove(workflowModel);
            }
            else
            {
                await LoadWorkflowsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to request revision: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelWorkflow(ApprovalWorkflowDisplayModel? workflowModel)
    {
        if (workflowModel == null) return;

        if (CurrentUser == null)
        {
            ErrorMessage = "You must be logged in to cancel workflows";
            return;
        }

        // Can only cancel your own submissions
        if (workflowModel.Workflow.SubmittedByUserId != CurrentUser.Id)
        {
            ErrorMessage = "You can only cancel your own submissions";
            return;
        }

        // Can only cancel pending or needs-revision workflows
        if (workflowModel.Workflow.Status != ApprovalStatus.Pending &&
            workflowModel.Workflow.Status != ApprovalStatus.NeedsRevision)
        {
            ErrorMessage = $"Cannot cancel a workflow with status '{workflowModel.Status}'";
            return;
        }

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            await _workflowService.CancelAsync(workflowModel.Workflow.Id);

            SuccessMessage = $"Cancelled workflow for '{workflowModel.EntityName}'";

            // Remove from collection instead of full reload
            Workflows.Remove(workflowModel);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to cancel workflow: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        await LoadWorkflowsAsync();
    }

    [RelayCommand]
    private void Close()
    {
        _onClose?.Invoke();
    }

    partial void OnFilterStatusChanged(ApprovalStatus? value)
    {
        _ = LoadWorkflowsAsync();
    }

    partial void OnShowMySubmissionsChanged(bool value)
    {
        _ = LoadWorkflowsAsync();
    }
}

public partial class ApprovalWorkflowDisplayModel : ObservableObject
{
    public ApprovalWorkflow Workflow { get; }

    public string EntityType => Workflow.EntityType;
    public string EntityName => Workflow.EntityName;
    public string Status => Workflow.Status.ToString();
    public string Priority => Workflow.Priority.ToString();
    public string SubmittedBy => Workflow.SubmittedByUser?.Email ?? "Unknown";
    public string SubmittedAt => Workflow.SubmittedAt.ToString("MM/dd/yyyy HH:mm");
    public string ReviewedBy => Workflow.ReviewedByUser?.Email ?? "—";
    public string ReviewedAt => Workflow.ReviewedAt?.ToString("MM/dd/yyyy HH:mm") ?? "—";
    public string ReviewNotes => Workflow.ReviewNotes ?? "—";
    public string ChangesSummary => Workflow.ChangesSummary ?? "No changes summary";

    public string StatusColor => Workflow.Status switch
    {
        ApprovalStatus.Pending => "#FFF3E0",
        ApprovalStatus.Approved => "#E8F5E9",
        ApprovalStatus.Rejected => "#FFEBEE",
        ApprovalStatus.NeedsRevision => "#FFF9C4",
        ApprovalStatus.Cancelled => "#F5F5F5",
        _ => "#FFFFFF"
    };

    public string StatusTextColor => Workflow.Status switch
    {
        ApprovalStatus.Pending => "#E65100",
        ApprovalStatus.Approved => "#2E7D32",
        ApprovalStatus.Rejected => "#C62828",
        ApprovalStatus.NeedsRevision => "#F57C00",
        ApprovalStatus.Cancelled => "#757575",
        _ => "#000000"
    };

    public string PriorityColor => Workflow.Priority switch
    {
        ApprovalPriority.Low => "#E3F2FD",
        ApprovalPriority.Normal => "#F5F5F5",
        ApprovalPriority.High => "#FFF3E0",
        ApprovalPriority.Urgent => "#FFEBEE",
        _ => "#FFFFFF"
    };

    public ApprovalWorkflowDisplayModel(ApprovalWorkflow workflow)
    {
        Workflow = workflow;
    }
}
