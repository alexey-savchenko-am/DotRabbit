using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Entities;

public class EventContainer<TEvent> : IEventContainer<TEvent>
    where TEvent : class, IEvent
{
    public string Id { get; }
    public Domain Domain { get; }
    public Type EventType => typeof(TEvent);
    public TEvent Event { get; }
    public string? Error { get; }
    public IMessage? Message { get; }

    public EventContainer(EventContainerData data, TEvent @event)
    {
        Id = data.Id;
        Domain = data.Domain;
        Error = data.Error;
        Message = data.Message;
        Event = @event;
    }

    public override string ToString() => $"{Domain.Name}:id-{Id}";
}

