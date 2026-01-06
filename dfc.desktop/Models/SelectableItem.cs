using System;

namespace Dfc.Desktop.Models;

public class SelectableItem<T>
{
    public T Item { get; }
    public string DisplayName { get; }

    public SelectableItem(T item, Func<T, string> nameSelector)
    {
        Item = item;
        DisplayName = nameSelector(item);
    }
}