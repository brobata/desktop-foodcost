using Avalonia.Controls;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using System;

namespace Freecost.Desktop.Services;

/// <summary>
/// Helper service for improving accessibility (WCAG compliance)
/// </summary>
public static class AccessibilityHelper
{
    /// <summary>
    /// Set accessibility properties for a control
    /// </summary>
    public static void SetAccessibility(Control control, string name, string? helpText = null, AutomationControlType? controlType = null)
    {
        AutomationProperties.SetName(control, name);

        if (helpText != null)
        {
            AutomationProperties.SetHelpText(control, helpText);
        }

        if (controlType.HasValue)
        {
            AutomationProperties.SetAutomationId(control, $"{controlType}_{name.Replace(" ", "_")}");
        }
    }

    /// <summary>
    /// Set up accessibility for a TextBox
    /// </summary>
    public static void SetupTextBox(TextBox textBox, string label, string? placeholder = null)
    {
        AutomationProperties.SetName(textBox, label);
        AutomationProperties.SetAutomationId(textBox, $"TextBox_{label.Replace(" ", "_")}");

        if (placeholder != null)
        {
            textBox.Watermark = placeholder;
            AutomationProperties.SetHelpText(textBox, $"Enter {label.ToLower()}. {placeholder}");
        }
        else
        {
            AutomationProperties.SetHelpText(textBox, $"Enter {label.ToLower()}");
        }
    }

    /// <summary>
    /// Set up accessibility for a Button
    /// </summary>
    public static void SetupButton(Button button, string label, string? action = null)
    {
        AutomationProperties.SetName(button, label);
        AutomationProperties.SetAutomationId(button, $"Button_{label.Replace(" ", "_")}");

        var helpText = action ?? $"Click to {label.ToLower()}";
        AutomationProperties.SetHelpText(button, helpText);
    }

    /// <summary>
    /// Set up accessibility for a DataGrid
    /// </summary>
    public static void SetupDataGrid(DataGrid dataGrid, string contentType, int itemCount = 0)
    {
        AutomationProperties.SetName(dataGrid, $"{contentType} List");
        AutomationProperties.SetAutomationId(dataGrid, $"DataGrid_{contentType}");

        var helpText = itemCount > 0
            ? $"{contentType} list with {itemCount} items. Use arrow keys to navigate, Enter to edit, Delete to remove."
            : $"{contentType} list. Use arrow keys to navigate, Enter to edit, Delete to remove.";

        AutomationProperties.SetHelpText(dataGrid, helpText);
        AutomationProperties.SetItemStatus(dataGrid, $"{itemCount} items");
    }

    /// <summary>
    /// Set up accessibility for a ComboBox
    /// </summary>
    public static void SetupComboBox(ComboBox comboBox, string label, int optionCount = 0)
    {
        AutomationProperties.SetName(comboBox, label);
        AutomationProperties.SetAutomationId(comboBox, $"ComboBox_{label.Replace(" ", "_")}");

        var helpText = optionCount > 0
            ? $"Select {label.ToLower()} from {optionCount} options"
            : $"Select {label.ToLower()}";

        AutomationProperties.SetHelpText(comboBox, helpText);
    }

    /// <summary>
    /// Set up accessibility for a CheckBox
    /// </summary>
    public static void SetupCheckBox(CheckBox checkBox, string label, string? description = null)
    {
        AutomationProperties.SetName(checkBox, label);
        AutomationProperties.SetAutomationId(checkBox, $"CheckBox_{label.Replace(" ", "_")}");

        var helpText = description ?? $"Check to enable {label.ToLower()}";
        AutomationProperties.SetHelpText(checkBox, helpText);
    }

    /// <summary>
    /// Announce a status message to screen readers
    /// </summary>
    public static void AnnounceStatus(Control control, string message, bool isPolite = true)
    {
        // Use live region for announcements
        AutomationProperties.SetLiveSetting(control, isPolite ? AutomationLiveSetting.Polite : AutomationLiveSetting.Assertive);
        AutomationProperties.SetItemStatus(control, message);
    }

    /// <summary>
    /// Set up tab order for a collection of controls
    /// </summary>
    public static void SetupTabOrder(params (Control control, int order)[] controls)
    {
        foreach (var (control, order) in controls)
        {
            control.TabIndex = order;
        }
    }

    /// <summary>
    /// Add access key (mnemonic) to a button
    /// </summary>
    public static void AddAccessKey(Button button, char key)
    {
        var content = button.Content?.ToString() ?? "";
        var keyIndex = content.IndexOf(key, StringComparison.OrdinalIgnoreCase);

        if (keyIndex >= 0)
        {
            // Add underscore before the access key
            button.Content = content.Insert(keyIndex, "_");
        }
        else
        {
            // Append access key if not found in text
            button.Content = $"{content} (_{key})";
        }
    }

    /// <summary>
    /// Set up keyboard hints for a window
    /// </summary>
    public static void SetupKeyboardHints(Window window)
    {
        var helpText = "Keyboard shortcuts: " +
                      "Ctrl+N = New, " +
                      "Ctrl+S = Save, " +
                      "Ctrl+F = Find, " +
                      "F2 = Edit, " +
                      "Delete = Remove, " +
                      "Esc = Cancel, " +
                      "F1 = Help";

        AutomationProperties.SetHelpText(window, helpText);
    }

    /// <summary>
    /// Set ARIA role equivalent for custom controls
    /// </summary>
    public static void SetAriaRole(Control control, string role)
    {
        AutomationProperties.SetAccessibilityView(control, AccessibilityView.Content);

        // Map common ARIA roles to automation control types
        var controlType = role.ToLower() switch
        {
            "button" => AutomationControlType.Button,
            "textbox" or "searchbox" => AutomationControlType.Edit,
            "combobox" or "listbox" => AutomationControlType.ComboBox,
            "checkbox" => AutomationControlType.CheckBox,
            "radiobutton" => AutomationControlType.RadioButton,
            "list" or "grid" => AutomationControlType.List,
            "menu" => AutomationControlType.Menu,
            "menuitem" => AutomationControlType.MenuItem,
            "dialog" => AutomationControlType.Window,
            "alert" => AutomationControlType.Custom,
            _ => AutomationControlType.Custom
        };

        // Note: Avalonia doesn't directly support setting control type at runtime,
        // but we can use AutomationId to indicate the role
        AutomationProperties.SetAutomationId(control, $"Role_{role}_{control.Name}");
    }

    /// <summary>
    /// Mark a control as required field
    /// </summary>
    public static void MarkAsRequired(Control control)
    {
        AutomationProperties.SetIsRequiredForForm(control, true);

        // Add visual indicator (asterisk) if it's a label
        if (control is TextBlock textBlock && textBlock.Text != null)
        {
            if (!textBlock.Text.EndsWith("*"))
            {
                textBlock.Text += " *";
            }
        }
    }

    /// <summary>
    /// Set validation error message for screen readers
    /// </summary>
    public static void SetValidationError(Control control, string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            AutomationProperties.SetItemStatus(control, "Valid");
        }
        else
        {
            AutomationProperties.SetItemStatus(control, $"Error: {errorMessage}");
            AutomationProperties.SetLiveSetting(control, AutomationLiveSetting.Assertive);
        }
    }
}
