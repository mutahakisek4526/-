using System;

namespace AacV1.Core;

public sealed class UndoAction
{
    public UndoAction(Action action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Action Action { get; }

    public void Execute() => Action();
}
