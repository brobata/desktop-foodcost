using System;
using System.Collections.Generic;

namespace Freecost.Desktop.ViewModels;

/// <summary>
/// Represents an item that can be selected in a selection dialog
/// </summary>
public class SelectionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public object Data { get; set; } = null!; // ADD THIS LINE - stores the actual Recipe or Ingredient object

    public SelectionItem(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    // Parameterless constructor for object initializer syntax
    public SelectionItem()
    {
    }
}

public class SelectionDialogViewModel
{
    public List<SelectionItem> Items { get; }
    public string Title { get; }

    public SelectionDialogViewModel(List<SelectionItem> items, string title)
    {
        Items = items;
        Title = title;
    }
}