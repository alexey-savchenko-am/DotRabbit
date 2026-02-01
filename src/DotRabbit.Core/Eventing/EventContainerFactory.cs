using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Entities;

namespace DotRabbit.Core.Eventing;

internal sealed class EventContainerFactory
    : IEventContainerFactory
{
    private readonly Dictionary<Type, Func<EventContainerData, IEvent, IEventContainer<IEvent>>> _factories = [];

    public EventContainerFactory(IEnumerable<Type> eventTypes)
    {
        foreach (var type in eventTypes)
        {
            Register(type);
        }
    }

    public IEventContainer<IEvent> Create(EventContainerData data, IEvent @event)
    {
        var type = @event.GetType();

        if (!_factories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"Event type {type.FullName} is not registered");
        }

        return factory(data, @event);
    }

    private void Register(Type eventType)
    {
        _factories[eventType] = EventContainerFactoryBuilder.Build(eventType);
    }

}
