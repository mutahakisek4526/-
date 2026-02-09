using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AacV1.Core;

public sealed class SpeechService : ISpeechService
{
    private readonly object _sync = new();
    private CancellationTokenSource? _currentCts;
    private dynamic? _voice;
    private bool _useStaThread;
    private StaWorker? _staWorker;

    public async Task SpeakLatestAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CancellationTokenSource linkedCts;
        lock (_sync)
        {
            _currentCts?.Cancel();
            _currentCts?.Dispose();
            _currentCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts = _currentCts;
        }

        Stop();

        if (_useStaThread)
        {
            await EnqueueStaAsync(() => SpeakInternal(text, linkedCts.Token)).ConfigureAwait(false);
            return;
        }

        try
        {
            await Task.Run(() => SpeakInternal(text, linkedCts.Token), linkedCts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            _useStaThread = true;
            await EnqueueStaAsync(() => SpeakInternal(text, linkedCts.Token)).ConfigureAwait(false);
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cts;
        lock (_sync)
        {
            cts = _currentCts;
        }

        cts?.Cancel();

        if (_useStaThread)
        {
            _ = EnqueueStaAsync(StopInternal);
            return;
        }

        try
        {
            Task.Run(StopInternal);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void SpeakInternal(string text, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        try
        {
            EnsureVoice();
            if (ct.IsCancellationRequested)
            {
                return;
            }

            _voice.Speak(text, 0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void StopInternal()
    {
        try
        {
            EnsureVoice();
            _voice.Speak(string.Empty, 2);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void EnsureVoice()
    {
        if (_voice is not null)
        {
            return;
        }

        var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
        if (voiceType is null)
        {
            throw new InvalidOperationException("SAPI.SpVoice is not available.");
        }

        _voice = Activator.CreateInstance(voiceType);
    }

    private Task EnqueueStaAsync(Action action)
    {
        _staWorker ??= new StaWorker();
        return _staWorker.Enqueue(action);
    }

    private sealed class StaWorker
    {
        private readonly ConcurrentQueue<(Action action, TaskCompletionSource<bool> tcs)> _queue = new();
        private readonly AutoResetEvent _signal = new(false);
        private readonly Thread _thread;

        public StaWorker()
        {
            _thread = new Thread(Run)
            {
                IsBackground = true
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public Task Enqueue(Action action)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _queue.Enqueue((action, tcs));
            _signal.Set();
            return tcs.Task;
        }

        private void Run()
        {
            while (true)
            {
                _signal.WaitOne();
                while (_queue.TryDequeue(out var item))
                {
                    try
                    {
                        item.action();
                        item.tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        item.tcs.SetException(ex);
                    }
                }
            }
        }
    }
}
