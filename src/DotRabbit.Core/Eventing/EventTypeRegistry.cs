using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;
using System;

namespace DotRabbit.Core.Eventing;

internal sealed class EventTypeRegistry
    : IEventTypeRegistry
{
    private readonly Dictionary<string, Type> _eventTypes = [];

    public bool Register(EventDefinition @event)
    {
        if(_eventTypes.ContainsKey(@event.Name))
        {
            _eventTypes[@event.Name] = @event.Type;
            return true;
        }
          
        return _eventTypes.TryAdd(@event.Name, @event.Type);
    }

    public Type GetByName(string eventName)
        => _eventTypes.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException(
                $"Event type for '{eventName}' not registered");

}
