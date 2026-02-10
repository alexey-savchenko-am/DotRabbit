using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.IntegrationTests.Tools;

namespace DotRabbit.IntegrationTests.EventsAndHandlers;

internal class UserCreatedTestEventHandler : IEventHandler<UserCreatedTestEvent>
{
    private readonly EventProcessingSignal<IEventContainer<UserCreatedTestEvent>> _signal;
    private readonly EventProcessingCounter _counter;

    public UserCreatedTestEventHandler(
        EventProcessingSignal<IEventContainer<UserCreatedTestEvent>> signal,
        EventProcessingCounter counter)
    {
        _signal = signal;
        _counter = counter;
    }

    public Task HandleAsync(IEventContainer<UserCreatedTestEvent> @event)
    {
        Console.WriteLine("Done!");
        _signal.Signal(@event);
        _counter.Signal(_counter.Generation);
        return Task.CompletedTask;
    }
}
