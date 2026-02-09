namespace AacV1.Core;

public interface IUndoService
{
    bool CanUndo { get; }
    void Push(UndoAction action);
    void Undo();
}
