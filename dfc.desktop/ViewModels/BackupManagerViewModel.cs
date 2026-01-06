using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class BackupManagerViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private ObservableCollection<BackupFileInfo> _backups = new();

    [ObservableProperty]
    private BackupFileInfo? _selectedBackup;

    [ObservableProperty]
    private bool _isCreatingBackup;

    [ObservableProperty]
    private bool _isRestoring;

    [ObservableProperty]
    private bool _includePhotos = true;

    [ObservableProperty]
    private string _backupNotes = string.Empty;

    [ObservableProperty]
    private int _progressPercent;

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    [ObservableProperty]
    private bool _showProgress;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _canCancel;

    public string DefaultBackupDirectory { get; }

    public BackupManagerViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        DefaultBackupDirectory = _backupService.GetDefaultBackupDirectory();
    }

    public async Task InitializeAsync()
    {
        await RefreshBackupsAsync();
    }

    [RelayCommand]
    private async Task RefreshBackupsAsync()
    {
        try
        {
            var backups = await _backupService.GetAvailableBackupsAsync();
            Backups.Clear();
            foreach (var backup in backups)
            {
                Backups.Add(backup);
            }

            StatusMessage = $"Found {Backups.Count} backup(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading backups: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        try
        {
            IsCreatingBackup = true;
            ShowProgress = true;
            CanCancel = true;
            _cancellationTokenSource = new CancellationTokenSource();

            var backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.freecost";
            var backupPath = Path.Combine(DefaultBackupDirectory, backupFileName);

            var progress = new Progress<BackupProgress>(p =>
            {
                ProgressPercent = p.PercentComplete;
                ProgressMessage = p.CurrentOperation;
            });

            await _backupService.CreateBackupAsync(
                backupPath,
                IncludePhotos,
                string.IsNullOrWhiteSpace(BackupNotes) ? null : BackupNotes,
                progress,
                _cancellationTokenSource.Token);

            StatusMessage = $"Backup created successfully: {backupFileName}";
            BackupNotes = string.Empty;

            await RefreshBackupsAsync();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Backup cancelled by user";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating backup: {ex.Message}";
        }
        finally
        {
            IsCreatingBackup = false;
            ShowProgress = false;
            CanCancel = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    private async Task CreateBackupToLocationAsync()
    {
        try
        {
            // This will be called from the UI with a file picker
            // For now, we'll create in default location
            await CreateBackupAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestoreBackup))]
    private async Task RestoreBackupAsync()
    {
        if (SelectedBackup == null)
            return;

        try
        {
            IsRestoring = true;
            ShowProgress = true;
            CanCancel = true;
            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<BackupProgress>(p =>
            {
                ProgressPercent = p.PercentComplete;
                ProgressMessage = p.CurrentOperation;
            });

            await _backupService.RestoreBackupAsync(
                SelectedBackup.FilePath,
                mergeData: false,
                progress,
                _cancellationTokenSource.Token);

            StatusMessage = "Restore completed successfully! Please restart the application.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Restore cancelled by user";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error restoring backup: {ex.Message}";
        }
        finally
        {
            IsRestoring = false;
            ShowProgress = false;
            CanCancel = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private bool CanRestoreBackup() => SelectedBackup != null && SelectedBackup.IsValid && !IsRestoring;

    [RelayCommand(CanExecute = nameof(CanDeleteBackup))]
    private async Task DeleteBackupAsync()
    {
        if (SelectedBackup == null)
            return;

        try
        {
            await _backupService.DeleteBackupAsync(SelectedBackup.FilePath);
            StatusMessage = $"Deleted backup: {SelectedBackup.FileName}";
            await RefreshBackupsAsync();
            SelectedBackup = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting backup: {ex.Message}";
        }
    }

    private bool CanDeleteBackup() => SelectedBackup != null;

    [RelayCommand]
    private async Task CreateAutomaticBackupAsync()
    {
        try
        {
            IsCreatingBackup = true;
            ShowProgress = true;
            ProgressMessage = "Creating automatic backup...";

            var backupPath = await _backupService.CreateAutomaticBackupAsync(maxBackupsToKeep: 5);

            if (backupPath != null)
            {
                StatusMessage = "Automatic backup created successfully";
                await RefreshBackupsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating automatic backup: {ex.Message}";
        }
        finally
        {
            IsCreatingBackup = false;
            ShowProgress = false;
        }
    }

    [RelayCommand]
    private void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
        ProgressMessage = "Cancelling...";
    }

    [RelayCommand]
    private void OpenBackupDirectory()
    {
        try
        {
            if (Directory.Exists(DefaultBackupDirectory))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = DefaultBackupDirectory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening directory: {ex.Message}";
        }
    }

    partial void OnSelectedBackupChanged(BackupFileInfo? value)
    {
        RestoreBackupCommand.NotifyCanExecuteChanged();
        DeleteBackupCommand.NotifyCanExecuteChanged();
    }
}
