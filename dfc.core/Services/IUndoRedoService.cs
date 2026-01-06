using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface IUndoRedoService
{
    void ExecuteCommand(ICommand command);
    Task ExecuteCommandAsync(IAsyncCommand command);
    bool CanUndo { get; }
    bool CanRedo { get; }
    void Undo();
    Task UndoAsync();
    void Redo();
    Task RedoAsync();
    void Clear();
    string? GetUndoDescription();
    string? GetRedoDescription();
}

public interface ICommand
{
    void Execute();
    void Undo();
    string Description { get; }
}

public interface IAsyncCommand
{
    Task ExecuteAsync();
    Task UndoAsync();
    string Description { get; }
}
