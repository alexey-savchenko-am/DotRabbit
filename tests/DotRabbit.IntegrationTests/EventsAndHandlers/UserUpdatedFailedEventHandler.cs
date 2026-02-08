using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.IntegrationTests.EventsAndHandlers;

internal class UserUpdatedFailedEventHandler 
    : IEventHandler<UserUpdatedFailedEvent>
{
    private readonly EventProcessingCounter _counter;

    public UserUpdatedFailedEventHandler(EventProcessingCounter counter)
    {
        _counter = counter;
    }

    public async Task HandleAsync(IEventContainer<UserUpdatedFailedEvent> @event)
    {
        _counter.Signal();
        throw new InvalidOperationException("Failed to process event at this point 🥺🥺🥺");
    }
}
