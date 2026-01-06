using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.Desktop.Services;

/// <summary>
/// Focus management service for enhanced keyboard navigation
/// </summary>
public class FocusManager
{
    /// <summary>
    /// Move focus to the next focusable control
    /// </summary>
    public static void FocusNext(Control currentControl)
    {
        var next = GetNextFocusable(currentControl, forward: true);
        next?.Focus();
    }

    /// <summary>
    /// Move focus to the previous focusable control
    /// </summary>
    public static void FocusPrevious(Control currentControl)
    {
        var previous = GetNextFocusable(currentControl, forward: false);
        previous?.Focus();
    }

    /// <summary>
    /// Get the next or previous focusable control in tab order
    /// </summary>
    private static Control? GetNextFocusable(Control current, bool forward)
    {
        var root = GetTopLevel(current);
        if (root == null) return null;

        var focusables = GetFocusableControls(root).ToList();
        if (focusables.Count == 0) return null;

        var currentIndex = focusables.IndexOf(current);
        if (currentIndex == -1) return focusables.FirstOrDefault();

        var nextIndex = forward
            ? (currentIndex + 1) % focusables.Count
            : (currentIndex - 1 + focusables.Count) % focusables.Count;

        return focusables[nextIndex];
    }

    /// <summary>
    /// Get all focusable controls in the visual tree
    /// </summary>
    private static IEnumerable<Control> GetFocusableControls(Visual root)
    {
        var queue = new Queue<Visual>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current is Control control && control.Focusable && control.IsEnabled && control.IsVisible)
            {
                yield return control;
            }

            foreach (var child in current.GetVisualChildren())
            {
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// Get the top-level control (Window or UserControl)
    /// </summary>
    private static Visual? GetTopLevel(Visual? control)
    {
        while (control != null)
        {
            if (control is Window or UserControl)
                return control;

            control = control.GetVisualParent();
        }

        return null;
    }

    /// <summary>
    /// Focus the first focusable control in a container
    /// </summary>
    public static void FocusFirst(Control container)
    {
        var first = GetFocusableControls(container).FirstOrDefault();
        first?.Focus();
    }

    /// <summary>
    /// Focus a control by name within a container
    /// </summary>
    public static bool FocusByName(Control container, string name)
    {
        var target = container.FindControl<Control>(name);
        if (target?.Focusable == true && target.IsEnabled && target.IsVisible)
        {
            target.Focus();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Find and focus the nearest search/filter TextBox
    /// </summary>
    public static bool FocusSearchBox(Control container)
    {
        // Try to find by common names
        var searchBoxNames = new[] { "SearchBox", "FilterBox", "SearchTextBox", "txtSearch", "txtFilter" };

        foreach (var name in searchBoxNames)
        {
            if (FocusByName(container, name))
                return true;
        }

        // Fallback: find first TextBox with "search" or "filter" in watermark
        var searchBox = GetFocusableControls(container)
            .OfType<TextBox>()
            .FirstOrDefault(tb =>
            {
                var watermark = tb.Watermark?.ToString()?.ToLower();
                return watermark?.Contains("search") == true || watermark?.Contains("filter") == true;
            });

        if (searchBox != null)
        {
            searchBox.Focus();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handle arrow key navigation in a DataGrid
    /// </summary>
    public static void HandleDataGridArrowKeys(DataGrid dataGrid, Key key)
    {
        if (dataGrid.SelectedIndex == -1)
        {
            dataGrid.SelectedIndex = 0;
            return;
        }

        var currentIndex = dataGrid.SelectedIndex;
        var itemCount = dataGrid.ItemsSource?.Cast<object>().Count() ?? 0;

        switch (key)
        {
            case Key.Up:
                if (currentIndex > 0)
                    dataGrid.SelectedIndex = currentIndex - 1;
                break;

            case Key.Down:
                if (currentIndex < itemCount - 1)
                    dataGrid.SelectedIndex = currentIndex + 1;
                break;

            case Key.Home:
                dataGrid.SelectedIndex = 0;
                break;

            case Key.End:
                dataGrid.SelectedIndex = itemCount - 1;
                break;

            case Key.PageUp:
                dataGrid.SelectedIndex = Math.Max(0, currentIndex - 10);
                break;

            case Key.PageDown:
                dataGrid.SelectedIndex = Math.Min(itemCount - 1, currentIndex + 10);
                break;
        }

        // Scroll to selected item
        if (dataGrid.SelectedItem != null)
        {
            dataGrid.ScrollIntoView(dataGrid.SelectedItem, null);
        }
    }

    /// <summary>
    /// Set up common keyboard shortcuts for a window
    /// </summary>
    public static void SetupWindowShortcuts(Window window, Action? onClose = null, Action? onHelp = null)
    {
        window.KeyDown += (sender, e) =>
        {
            // ESC to close
            if (e.Key == Key.Escape && onClose != null)
            {
                onClose();
                e.Handled = true;
            }

            // F1 for help
            if (e.Key == Key.F1 && onHelp != null)
            {
                onHelp();
                e.Handled = true;
            }

            // Ctrl+W to close
            if (e.Key == Key.W && e.KeyModifiers == KeyModifiers.Control && onClose != null)
            {
                onClose();
                e.Handled = true;
            }
        };
    }
}

/// <summary>
/// Extension methods for focus management
/// </summary>
public static class FocusExtensions
{
    /// <summary>
    /// Focus with delay (useful for newly created controls)
    /// </summary>
    public static async System.Threading.Tasks.Task FocusDelayedAsync(this Control control, int delayMs = 100)
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(delayMs);
            control.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in FocusDelayedAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Select all text in a TextBox when focused
    /// </summary>
    public static void SelectAllOnFocus(this TextBox textBox)
    {
        textBox.GotFocus += (s, e) => textBox.SelectAll();
    }
}
