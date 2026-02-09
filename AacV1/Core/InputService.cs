using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AacV1.Core;

public sealed class InputService : IInputService
{
    private readonly TimeSpan _cooldown = TimeSpan.FromMilliseconds(150);
    private DateTime _lastHandled = DateTime.MinValue;

    public event Action? SelectPressed;
    public event Action? BackPressed;

    public void HandleKeyDown(Key key)
    {
        var now = DateTime.UtcNow;
        if (now - _lastHandled < _cooldown)
        {
            return;
        }

        if (key == Key.Enter || key == Key.Return)
        {
            _lastHandled = now;
            TryInvoke(SelectPressed);
        }
        else if (key == Key.Escape)
        {
            _lastHandled = now;
            TryInvoke(BackPressed);
        }
    }

    private static void TryInvoke(Action? handler)
    {
        try
        {
            handler?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
