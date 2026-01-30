using DotRabbit.Core.Events.Entities;

namespace DotRabbit.Core.Events.Abstract;

public interface IEventContainerFactory
{
    IEventContainer<IEvent> Create(EventContainerData data, IEvent @event);
}
