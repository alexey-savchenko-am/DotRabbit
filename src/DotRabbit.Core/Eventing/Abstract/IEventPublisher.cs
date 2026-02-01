using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(DomainDefinition domain, TEvent @event, CancellationToken ct = default) where TEvent : IEvent;
}
