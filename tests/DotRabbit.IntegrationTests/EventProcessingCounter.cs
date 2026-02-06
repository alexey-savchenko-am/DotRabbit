using System.Diagnostics;
using Xunit.Abstractions;

namespace DotRabbit.IntegrationTests;

public sealed class EventProcessingCounter
{
    public int _remaining;
    private TaskCompletionSource _tcs = null!;

    public EventProcessingCounter()
    {
    }

    public void Reset(int count)
    {
        _remaining = count;
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async Task<long> WaitAsync(TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        await _tcs.Task.WaitAsync(timeout);
        return sw.ElapsedMilliseconds;
    }

    public void Signal()
    {
        if (Interlocked.Decrement(ref _remaining) == 0)
        {
            _tcs.TrySetResult();
        }
    }
}
