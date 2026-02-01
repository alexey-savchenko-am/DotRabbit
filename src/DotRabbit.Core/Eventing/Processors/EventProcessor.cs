using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Processors;

/// <summary>
/// Scoped service.
/// Created once per message processing scope.
/// Delegates event handling to the corresponding event handler.
/// </summary>
internal sealed class EventProcessor<TEvent, THandler>
    : IEventProcessor
    where TEvent : IEvent
    where THandler: IEventHandler<TEvent>
{
    public DomainDefinition Domain { get; }
    private readonly Func<THandler> _handlerFactory;
    public Type EventType { get; } = typeof(TEvent);

    public EventProcessor(
        DomainDefinition domain,
        Func<THandler> handlerFactory)
    {
        Domain = domain;
        _handlerFactory = handlerFactory;
    }

    public bool CanProcess(IEventContainer<IEvent> @event)
    {
        return @event.Domain == Domain
         && @event.Event.GetType() == EventType;
    }

    public Task ProcessAsync(IEventContainer<IEvent> @event)
    {
        if (@event is not IEventContainer<TEvent> typed)
        {
            throw new InvalidOperationException(
                $"Invalid event type. {typeof(TEvent).Name} expected.");
        }

        var handler = _handlerFactory();
        return handler.HandleAsync(typed);
    }
}
