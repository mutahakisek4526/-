using System;

namespace AacV1.Core;

public interface IDwellService
{
    event Action<string>? Focused;
    event Action<string>? Committed;
    void PointerEnter(string key);
    void PointerLeave(string key);
    void Reset();
}

public interface IDwellHost
{
    IDwellService Dwell { get; }
}
