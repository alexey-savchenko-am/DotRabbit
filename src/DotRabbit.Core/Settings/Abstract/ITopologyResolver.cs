using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings.Abstract;

public interface ITopologyResolver
{
    string ResolveExchange(DomainDefinition domain);
    string ResolveExchange<TEvent>() where TEvent : IEvent;
    string ResolveRetryExchange(DomainDefinition domain);
    string ResolveDlxExchange(DomainDefinition domain);
    string ResolveQueue(DomainDefinition domain, EventDefinition @event);
    string ResolveQueue<TEvent>() where TEvent : IEvent;
    string ResolveRetryQueue(DomainDefinition domain, EventDefinition @event);
    string ResolveDlqQueue(DomainDefinition domain, EventDefinition @event);
    string ResolveRoutingKey<TEvent>() where TEvent : IEvent;
    string ResolveRoutingKey<TEvent>(DomainDefinition domain) where TEvent : IEvent;

}
