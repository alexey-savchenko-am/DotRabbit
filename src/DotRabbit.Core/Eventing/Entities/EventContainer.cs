using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Entities;

public class EventContainer<TEvent> : IEventContainer<TEvent>
    where TEvent : class, IEvent
{
    public string Id { get; }
    public DomainDefinition Domain { get; }
    public TEvent Event { get; }
    public IMessage? Message { get; }

    public EventContainer(EventContainerData data, TEvent @event)
    {
        Id = data.Id;
        Domain = data.Domain;
        Message = data.Message;
        Event = @event;
    }

    public override string ToString() => $"{Domain.Name}:id-{Id}";
}

