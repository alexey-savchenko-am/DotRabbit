using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Abstract;
using System.Text.Json;

namespace DotRabbit.Core.Settings.Serialize;

internal class JsonBasedEventSerializer
    : IEventSerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonBasedEventSerializer()
    {
        _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
        };
    }
    public TEvent Deserialize<TEvent>(string payload) where TEvent : IEvent
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return JsonSerializer.Deserialize<TEvent>(payload, _options)
            ?? throw new InvalidOperationException($"Cannot deserialize {typeof(TEvent).Name}");
    }

    public IEvent Deserialize(string payload, Type eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        ArgumentNullException.ThrowIfNull(eventType);

        if (!typeof(IEvent).IsAssignableFrom(eventType))
            throw new ArgumentException($"{eventType} is not IEvent");

        return (IEvent)(JsonSerializer.Deserialize(payload, eventType, _options)
            ?? throw new InvalidOperationException($"Cannot deserialize {eventType.Name}"));
    }

    public string Serialize<TEvent>(TEvent @event) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        return JsonSerializer.Serialize(@event, _options);  
    }
}

