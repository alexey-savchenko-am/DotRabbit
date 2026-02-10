namespace DotRabbit.IntegrationTests.Tools;

public sealed class EventProcessingSignal<TEvent>
{
    private readonly TaskCompletionSource<TEvent> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<TEvent> WaitAsync(TimeSpan timeout)
        => _tcs.Task.WaitAsync(timeout);

    public void Signal(TEvent @event)
        => _tcs.TrySetResult(@event);
}
