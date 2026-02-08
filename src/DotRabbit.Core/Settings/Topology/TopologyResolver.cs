using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings.Topology;

internal sealed class TopologyResolver: ITopologyResolver
{
    private readonly IServiceInfo _serviceInfo;
    private readonly IEventDefinitionRegistry _eventDefinitionRegistry;

    public TopologyResolver(
        IServiceInfo serviceInfo,
        IEventDefinitionRegistry eventDefinitionRegistry)
    {
        _serviceInfo = serviceInfo;
        _eventDefinitionRegistry = eventDefinitionRegistry;
    }

    #region Exchange
    public string ResolveExchange(DomainDefinition domain) => 
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.topic";

    public string ResolveExchange<TEvent>() where TEvent : IEvent
    {
        var eventDef = GetEventDefinition(typeof(TEvent));

        if (eventDef is null)
            throw new InvalidOperationException($"Event defenition for type {typeof(TEvent).Name} not found");

        return $"{_serviceInfo.GetInfo().Name}.{eventDef.Domain.Name}.topic";
    }

    public string ResolveRetryExchange(DomainDefinition domain) =>
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.retry";

    public string ResolveDlxExchange(DomainDefinition domain) =>
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.dlx";
    #endregion

    #region Queue
    public string ResolveQueue(DomainDefinition domain, EventDefinition @event) =>
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.{@event.Name}.q";

    public string ResolveQueue<TEvent>() where TEvent : IEvent
    {
        var eventDef = GetEventDefinition(typeof(TEvent));

        if (eventDef is null)
            throw new InvalidOperationException($"Event defenition for type {typeof(TEvent).Name} not found");

        return $"{_serviceInfo.GetInfo().Name}.{eventDef.Domain.Name}.{eventDef.Name}.q";
    }
     
    public string ResolveRetryQueue(DomainDefinition domain, EventDefinition @event) =>
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.{@event.Name}.retry";

    public string ResolveDlqQueue(DomainDefinition domain, EventDefinition @event) =>
        $"{_serviceInfo.GetInfo().Name}.{domain.Name}.{@event.Name}.dlq";
    #endregion

    #region RoutingKey
    public string ResolveRoutingKey<TEvent>() where TEvent : IEvent
    {
        var eventDef = GetEventDefinition(typeof(TEvent));

        return eventDef is null
            ? throw new InvalidOperationException($"Event defenition for type {typeof(TEvent).Name} not found")
            : eventDef.Name;
    }

    public string ResolveRoutingKey<TEvent>(DomainDefinition domain) where TEvent : IEvent
    {
        var eventDef = GetEventDefinition(typeof(TEvent), domain);

        return eventDef is null
            ? throw new InvalidOperationException($"Event defenition for type {typeof(TEvent).Name} not found")
            : eventDef.Name;
    }
    #endregion

    private EventDefinition? GetEventDefinition(Type eventType)
    {
        var eventDef = _eventDefinitionRegistry
          .GetAll()
          .SingleOrDefault(e => e.Type == eventType);

        return eventDef;
    }

    private EventDefinition? GetEventDefinition(Type eventType, DomainDefinition domain)
    {
        var eventDef = _eventDefinitionRegistry
          .GetAllByDomain(domain)
          .FirstOrDefault(e => e.Type == eventType);

        return eventDef;
    }
}
