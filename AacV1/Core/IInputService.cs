using System;
using System.Windows.Input;

namespace AacV1.Core;

public interface IInputService
{
    event Action? SelectPressed;
    event Action? BackPressed;
    void HandleKeyDown(Key key);
}
