using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace Dfc.Desktop.Services;

/// <summary>
/// Centralized keyboard shortcut management service
/// </summary>
public class KeyboardShortcutService
{
    private readonly Dictionary<string, Action> _shortcuts = new();

    public void RegisterShortcut(Key key, KeyModifiers modifiers, Action action)
    {
        var shortcutKey = GetShortcutKey(key, modifiers);
        _shortcuts[shortcutKey] = action;
    }

    public bool HandleKeyDown(KeyEventArgs e)
    {
        var shortcutKey = GetShortcutKey(e.Key, e.KeyModifiers);

        if (_shortcuts.TryGetValue(shortcutKey, out var action))
        {
            action();
            e.Handled = true;
            return true;
        }

        return false;
    }

    private static string GetShortcutKey(Key key, KeyModifiers modifiers)
    {
        return $"{modifiers}+{key}";
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
    }
}

/// <summary>
/// Common keyboard shortcuts across the application
/// </summary>
public static class CommonShortcuts
{
    // File operations
    public static readonly (KeyModifiers Modifiers, Key Key, string Description) New =
        (KeyModifiers.Control, Key.N, "Create new item");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Save =
        (KeyModifiers.Control, Key.S, "Save current item");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Delete =
        (KeyModifiers.None, Key.Delete, "Delete selected item");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Duplicate =
        (KeyModifiers.Control, Key.D, "Duplicate selected item");

    // Search and navigation
    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Find =
        (KeyModifiers.Control, Key.F, "Focus search box");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Refresh =
        (KeyModifiers.None, Key.F5, "Refresh data");

    // Editing
    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Edit =
        (KeyModifiers.None, Key.F2, "Edit selected item");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Escape =
        (KeyModifiers.None, Key.Escape, "Cancel/Close");

    // Export/Print
    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Print =
        (KeyModifiers.Control, Key.P, "Print/Generate PDF");

    public static readonly (KeyModifiers Modifiers, Key Key, string Description) Export =
        (KeyModifiers.Control | KeyModifiers.Shift, Key.E, "Export data");
}
