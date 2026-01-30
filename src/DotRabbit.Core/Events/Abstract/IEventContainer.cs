using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Abstract;

public interface IEventContainer<out TEvent>
    where TEvent : IEvent
{
    string Id { get; }
    Domain Domain { get; }
    Type EventType { get; }
    TEvent Event { get; }
    string? Error { get; }
}
