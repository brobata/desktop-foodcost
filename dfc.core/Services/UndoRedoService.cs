using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<object> _undoStack = new();
    private readonly Stack<object> _redoStack = new();
    private const int MaxStackSize = 50;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        PushToUndoStack(command);
        _redoStack.Clear(); // Clear redo stack when new action is performed
    }

    public async Task ExecuteCommandAsync(IAsyncCommand command)
    {
        await command.ExecuteAsync();
        PushToUndoStack(command);
        _redoStack.Clear(); // Clear redo stack when new action is performed
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var item = _undoStack.Pop();

        if (item is ICommand command)
        {
            command.Undo();
            _redoStack.Push(item);
        }
    }

    public async Task UndoAsync()
    {
        if (!CanUndo) return;

        var item = _undoStack.Pop();

        if (item is IAsyncCommand asyncCommand)
        {
            await asyncCommand.UndoAsync();
            _redoStack.Push(item);
        }
        else if (item is ICommand command)
        {
            command.Undo();
            _redoStack.Push(item);
        }
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var item = _redoStack.Pop();

        if (item is ICommand command)
        {
            command.Execute();
            _undoStack.Push(item);
        }
    }

    public async Task RedoAsync()
    {
        if (!CanRedo) return;

        var item = _redoStack.Pop();

        if (item is IAsyncCommand asyncCommand)
        {
            await asyncCommand.ExecuteAsync();
            _undoStack.Push(item);
        }
        else if (item is ICommand command)
        {
            command.Execute();
            _undoStack.Push(item);
        }
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public string? GetUndoDescription()
    {
        if (!CanUndo) return null;

        var item = _undoStack.Peek();
        return item switch
        {
            ICommand command => command.Description,
            IAsyncCommand asyncCommand => asyncCommand.Description,
            _ => null
        };
    }

    public string? GetRedoDescription()
    {
        if (!CanRedo) return null;

        var item = _redoStack.Peek();
        return item switch
        {
            ICommand command => command.Description,
            IAsyncCommand asyncCommand => asyncCommand.Description,
            _ => null
        };
    }

    private void PushToUndoStack(object item)
    {
        _undoStack.Push(item);

        // Limit stack size
        if (_undoStack.Count > MaxStackSize)
        {
            var tempList = _undoStack.Take(MaxStackSize).ToList();
            _undoStack.Clear();
            foreach (var tempItem in tempList.AsEnumerable().Reverse())
            {
                _undoStack.Push(tempItem);
            }
        }
    }
}
