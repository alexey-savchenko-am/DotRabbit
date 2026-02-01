namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    Task HandleAsync(IEventContainer<TEvent> @event);
}
