using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfc.Desktop.Services;

/// <summary>
/// Tracks user progress through the extended tutorial system
/// </summary>
public interface ITutorialProgressTracker
{
    Task<TutorialProgress> LoadProgressAsync();
    Task SaveProgressAsync(TutorialProgress progress);
    Task MarkStepCompletedAsync(string moduleId, string stepId);
    Task ResetProgressAsync();
    Task<bool> HasCompletedStepAsync(string moduleId, string stepId);
    Task<int> GetTotalStepsCompletedAsync();
}

public class TutorialProgressTracker : ITutorialProgressTracker
{
    private readonly string _progressFilePath;
    private TutorialProgress? _cachedProgress;

    public TutorialProgressTracker()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Desktop Food Cost"
        );

        Directory.CreateDirectory(appDataPath);
        _progressFilePath = Path.Combine(appDataPath, "tutorial_progress.json");
    }

    public async Task<TutorialProgress> LoadProgressAsync()
    {
        if (_cachedProgress != null)
            return _cachedProgress;

        try
        {
            if (File.Exists(_progressFilePath))
            {
                var json = await File.ReadAllTextAsync(_progressFilePath);
                _cachedProgress = JsonSerializer.Deserialize<TutorialProgress>(json) ?? new TutorialProgress();
            }
            else
            {
                _cachedProgress = new TutorialProgress();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tutorial progress: {ex.Message}");
            _cachedProgress = new TutorialProgress();
        }

        return _cachedProgress;
    }

    public async Task SaveProgressAsync(TutorialProgress progress)
    {
        try
        {
            var json = JsonSerializer.Serialize(progress, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_progressFilePath, json);
            _cachedProgress = progress;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving tutorial progress: {ex.Message}");
        }
    }

    public async Task MarkStepCompletedAsync(string moduleId, string stepId)
    {
        var progress = await LoadProgressAsync();

        var completedKey = $"{moduleId}:{stepId}";
        if (!progress.CompletedSteps.Contains(completedKey))
        {
            progress.CompletedSteps.Add(completedKey);
            progress.LastAccessedDate = DateTime.UtcNow;

            if (!progress.StartedModules.Contains(moduleId))
            {
                progress.StartedModules.Add(moduleId);
            }

            await SaveProgressAsync(progress);
        }
    }

    public async Task ResetProgressAsync()
    {
        var progress = new TutorialProgress();
        await SaveProgressAsync(progress);
    }

    public async Task<bool> HasCompletedStepAsync(string moduleId, string stepId)
    {
        var progress = await LoadProgressAsync();
        var completedKey = $"{moduleId}:{stepId}";
        return progress.CompletedSteps.Contains(completedKey);
    }

    public async Task<int> GetTotalStepsCompletedAsync()
    {
        var progress = await LoadProgressAsync();
        return progress.CompletedSteps.Count;
    }
}

/// <summary>
/// Represents user's progress through the tutorial system
/// </summary>
public class TutorialProgress
{
    /// <summary>
    /// Whether the user has opted to take the extended tutorial
    /// </summary>
    public bool OptedInToExtendedTutorial { get; set; }

    /// <summary>
    /// Date when user first accessed the tutorial
    /// </summary>
    public DateTime? FirstAccessedDate { get; set; }

    /// <summary>
    /// Date when user last accessed the tutorial
    /// </summary>
    public DateTime? LastAccessedDate { get; set; }

    /// <summary>
    /// List of completed steps (format: "moduleId:stepId")
    /// </summary>
    public List<string> CompletedSteps { get; set; } = new();

    /// <summary>
    /// List of modules the user has started
    /// </summary>
    public List<string> StartedModules { get; set; } = new();

    /// <summary>
    /// Last module the user was viewing
    /// </summary>
    public string? LastModuleId { get; set; }

    /// <summary>
    /// Last step the user was viewing
    /// </summary>
    public string? LastStepId { get; set; }

    /// <summary>
    /// Whether the user has completed the entire tutorial
    /// </summary>
    public bool HasCompletedFullTutorial { get; set; }

    /// <summary>
    /// Date when user completed the full tutorial
    /// </summary>
    public DateTime? CompletedDate { get; set; }
}
