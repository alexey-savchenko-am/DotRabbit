using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventDefinitionRegistry
{
    EventDefinition Register(EventDefinition @event);

    EventDefinition Register(Type eventType, DomainDefinition domain);

    IReadOnlyCollection<EventDefinition> GetAllByDomain(DomainDefinition domain);

    IReadOnlyCollection<EventDefinition> GetAll();

    EventDefinition GetByNameWithinDomain(string eventName, DomainDefinition domain);
}
