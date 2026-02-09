using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace AacV1.Core;

public sealed class DwellService : IDwellService
{
    private static readonly TimeSpan FocusDuration = TimeSpan.FromMilliseconds(600);
    private static readonly TimeSpan CommitDuration = TimeSpan.FromMilliseconds(600);

    private readonly DispatcherTimer _timer;
    private DwellState _state = DwellState.Idle;
    private string? _currentKey;

    public DwellService()
    {
        _timer = new DispatcherTimer
        {
            Interval = FocusDuration
        };
        _timer.Tick += HandleTick;
    }

    public event Action<string>? Focused;
    public event Action<string>? Committed;

    public void PointerEnter(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!string.Equals(_currentKey, key, StringComparison.Ordinal))
        {
            ResetInternal();
            RaiseFocusCleared();
            _currentKey = key;
            _timer.Interval = FocusDuration;
            _timer.Start();
            return;
        }

        if (_state == DwellState.Idle && !_timer.IsEnabled)
        {
            _timer.Interval = FocusDuration;
            _timer.Start();
        }
    }

    public void PointerLeave(string key)
    {
        if (_currentKey is null)
        {
            return;
        }

        if (!string.Equals(_currentKey, key, StringComparison.Ordinal))
        {
            return;
        }

        Reset();
    }

    public void Reset()
    {
        ResetInternal();
        RaiseFocusCleared();
    }

    private void HandleTick(object? sender, EventArgs e)
    {
        try
        {
            if (_currentKey is null)
            {
                ResetInternal();
                return;
            }

            if (_state == DwellState.Idle)
            {
                _state = DwellState.Focused;
                Focused?.Invoke(_currentKey);
                _timer.Interval = CommitDuration;
                return;
            }

            if (_state == DwellState.Focused)
            {
                var committedKey = _currentKey;
                ResetInternal();
                Committed?.Invoke(committedKey);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ResetInternal();
        }
    }

    private void ResetInternal()
    {
        _timer.Stop();
        _state = DwellState.Idle;
        _currentKey = null;
    }

    private void RaiseFocusCleared()
    {
        try
        {
            Focused?.Invoke(string.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
