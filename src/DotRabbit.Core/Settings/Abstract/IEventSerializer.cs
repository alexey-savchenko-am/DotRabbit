using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.Core.Settings.Abstract;

public interface IEventSerializer
{
    TEvent Deserialize<TEvent>(string payload) where TEvent : IEvent;
    IEvent Deserialize(string payload, Type eventType);

    string Serialize<TEvent>(TEvent @event) where TEvent : IEvent;
}
