using System.Collections.Generic;
using System.Linq;

namespace Dfc.Core.Models;

public class ValidationFeedback
{
    public bool IsValid => !Errors.Any();
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public List<ValidationInfo> Infos { get; set; } = new();

    public void AddError(string field, string message, string? suggestedFix = null)
    {
        Errors.Add(new ValidationError
        {
            Field = field,
            Message = message,
            SuggestedFix = suggestedFix
        });
    }

    public void AddWarning(string field, string message, string? suggestedAction = null)
    {
        Warnings.Add(new ValidationWarning
        {
            Field = field,
            Message = message,
            SuggestedAction = suggestedAction
        });
    }

    public void AddInfo(string field, string message)
    {
        Infos.Add(new ValidationInfo
        {
            Field = field,
            Message = message
        });
    }

    public string GetFormattedMessage()
    {
        var messages = new List<string>();

        if (Errors.Any())
        {
            messages.Add("❌ ERRORS:");
            foreach (var error in Errors)
            {
                messages.Add($"  • {error.Field}: {error.Message}");
                if (!string.IsNullOrEmpty(error.SuggestedFix))
                {
                    messages.Add($"    💡 Suggestion: {error.SuggestedFix}");
                }
            }
        }

        if (Warnings.Any())
        {
            messages.Add("\n⚠️ WARNINGS:");
            foreach (var warning in Warnings)
            {
                messages.Add($"  • {warning.Field}: {warning.Message}");
                if (!string.IsNullOrEmpty(warning.SuggestedAction))
                {
                    messages.Add($"    💡 Suggestion: {warning.SuggestedAction}");
                }
            }
        }

        if (Infos.Any())
        {
            messages.Add("\nℹ️ INFO:");
            foreach (var info in Infos)
            {
                messages.Add($"  • {info.Field}: {info.Message}");
            }
        }

        return string.Join("\n", messages);
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
    public ValidationSeverity Severity => ValidationSeverity.Error;
}

public class ValidationWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SuggestedAction { get; set; }
    public ValidationSeverity Severity => ValidationSeverity.Warning;
}

public class ValidationInfo
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity => ValidationSeverity.Info;
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}
