using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventContainer<out TEvent>
    where TEvent : IEvent
{
    string Id { get; }
    DomainDefinition Domain { get; }
    TEvent Event { get; }
}
