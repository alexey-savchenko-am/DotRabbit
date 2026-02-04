using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings;
using DotRabbit.Core.Settings.Entities;
using System.Reflection;

namespace DotRabbit.Core.Eventing;

internal sealed class EventDefinitionRegistry : IEventDefinitionRegistry
{
    private readonly Dictionary<(DomainDefinition Domain, string EventName), EventDefinition>
        _registeredEventDefinitions = [];

    public EventDefinition Register(EventDefinition eventDefinition)
    {
        if (eventDefinition is null)
            throw new ArgumentNullException(nameof(eventDefinition));

        var key = (eventDefinition.Domain, eventDefinition.Name);
        _registeredEventDefinitions[key] = eventDefinition;

        return eventDefinition;
    }

    public EventDefinition Register(Type eventType, DomainDefinition domain)
    {
        if (eventType is null)
            throw new ArgumentNullException(nameof(eventType));

        if (!typeof(IEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException(
                $"{eventType.FullName} is not an event type",
                nameof(eventType));
        }

        var eventName = eventType
            .GetCustomAttribute<EventNameAttribute>()
            ?.EventName
            ?? eventType.Name.ToKebabNotation();

        var eventDefinition = new EventDefinition(eventName, eventType, domain);

        var key = (domain, eventName);
        _registeredEventDefinitions[key] = eventDefinition;

        return eventDefinition;
    }

    public IReadOnlyCollection<EventDefinition> GetAllByDomain(DomainDefinition domain)
    {
        if (domain is null)
            throw new ArgumentNullException(nameof(domain));

        return _registeredEventDefinitions
            .Where(x => x.Key.Domain.Equals(domain))
            .Select(x => x.Value)
            .ToList()
            .AsReadOnly();
    }

    public EventDefinition GetByNameWithinDomain(
        string eventName,
        DomainDefinition domain)
            => _registeredEventDefinitions.TryGetValue((domain, eventName), out var definition)
                ? definition
                : throw new InvalidOperationException(
                    $"Event '{eventName}' not registered in domain '{domain.Name}'");
}
