using DotRabbit.Core.Eventing.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventContainerFactory
{
    IEventContainer<IEvent> Create(EventContainerData data, IEvent @event);

    void Register(Type eventType);
}
