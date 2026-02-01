namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventProcessor
{
    Type EventType { get; }

    bool CanProcess(IEventContainer<IEvent> @event);

    Task ProcessAsync(IEventContainer<IEvent> @event);
}
