using System.Diagnostics;

namespace DotRabbit.IntegrationTests.Tools;

public sealed class EventProcessingCounter
{
    private int _remaining;
    private int _generation;
    private TaskCompletionSource<bool> _tcs = null!;

    public int Remaining => Volatile.Read(ref _remaining);
    public int Generation => Volatile.Read(ref _generation);

    /// <summary>
    /// Resets the counter. Must be called before publishing the event.
    /// </summary>
    public void Reset(int expectedCount)
    {
        if (expectedCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(expectedCount));

        Interlocked.Increment(ref _generation);
        Volatile.Write(ref _remaining, expectedCount);

        _tcs = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <summary>
    /// Signals successful processing attempt.
    /// Must be called with the generation captured after Reset().
    /// </summary>
    public void Signal(int generation)
    {
        if (generation != Generation)
            return; // ignore stale signals from previous test runs

        var remaining = Interlocked.Decrement(ref _remaining);

        if (remaining == 0)
        {
            _tcs.TrySetResult(true);
            return;
        }

        if (remaining < 0)
        {
            _tcs.TrySetException(
                new InvalidOperationException(
                    "EventProcessingCounter.Signal() was called more times than expected."));
        }
    }

    /// <summary>
    /// Waits until all expected signals are received or timeout expires.
    /// Returns elapsed milliseconds.
    /// </summary>
    public async Task<long> WaitAsync(TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _tcs.Task.WaitAsync(timeout);
            return sw.ElapsedMilliseconds;
        }
        catch (TimeoutException)
        {
            throw new TimeoutException(
                $"Timeout waiting for event processing. Remaining attempts: {Remaining}");
        }
    }
}
