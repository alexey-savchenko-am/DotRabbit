using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings.Topology;

internal static class RmqTopologyResolver
{
    public static string ResolveExchange(Service service, DomainDefinition domain)
    {
        return $"{service.Name}.{domain.Name}.topic";
    }

    public static string ResolveRetryExchange(Service service, DomainDefinition domain)
    {
        return $"{service.Name}.{domain.Name}.retry";
    }

    public static string ResolveDlxExchange(Service service, DomainDefinition domain)
    {
        return $"{service.Name}.{domain.Name}.dlx";
    }

    public static string ResolveQueue(Service service, DomainDefinition domain, EventDefinition @event)
    {
        return $"{service.Name}.{domain.Name}.{@event.Name}.q";
    }

    public static string ResolveRetryQueue(Service service, DomainDefinition domain, EventDefinition @event)
    {
        return $"{service.Name}.{domain.Name}.{@event.Name}.retry";
    }

    public static string ResolveDeadQueue(Service service, DomainDefinition domain, EventDefinition @event)
    {
        return $"{service.Name}.{domain.Name}.{@event.Name}.dlq";
    }

    public static string ResolveRoutingKey<TEvent>() where TEvent : IEvent
    {
        return typeof(TEvent).GetType().Name.ToKebabNotation();
    }
}
