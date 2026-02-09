using System.Collections.Generic;

namespace AacV1.Core;

public sealed class UndoService : IUndoService
{
    private readonly Stack<UndoAction> _stack = new();

    public bool CanUndo => _stack.Count > 0;

    public void Push(UndoAction action)
    {
        if (action is null)
        {
            return;
        }

        _stack.Push(action);
    }

    public void Undo()
    {
        if (_stack.Count == 0)
        {
            return;
        }

        var action = _stack.Pop();
        action.Execute();
    }
}
