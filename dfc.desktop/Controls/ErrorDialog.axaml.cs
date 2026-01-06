using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dfc.Desktop.Controls;

public partial class ErrorDialog : Window, INotifyPropertyChanged
{
    private string _title = "Error";
    private string _message = string.Empty;
    private string? _technicalDetails;
    private List<string> _suggestedActions = new();

    public new string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public string? TechnicalDetails
    {
        get => _technicalDetails;
        set { _technicalDetails = value; OnPropertyChanged(); }
    }

    public List<string> SuggestedActions
    {
        get => _suggestedActions;
        set { _suggestedActions = value; OnPropertyChanged(); }
    }

    public ErrorDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public static ErrorDialog Create(string title, string message, Exception? exception = null, List<string>? suggestedActions = null)
    {
        var dialog = new ErrorDialog
        {
            Title = title,
            Message = message,
            TechnicalDetails = exception?.ToString(),
            SuggestedActions = suggestedActions ?? new List<string>()
        };

        return dialog;
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnCopyError(object? sender, RoutedEventArgs e)
    {
        try
        {
            var errorText = $"Error: {Title}\n\nMessage: {Message}";

            if (!string.IsNullOrEmpty(TechnicalDetails))
            {
                errorText += $"\n\nTechnical Details:\n{TechnicalDetails}";
            }

            if (SuggestedActions.Count > 0)
            {
                errorText += $"\n\nSuggested Actions:\n";
                foreach (var action in SuggestedActions)
                {
                    errorText += $"- {action}\n";
                }
            }

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(errorText);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
