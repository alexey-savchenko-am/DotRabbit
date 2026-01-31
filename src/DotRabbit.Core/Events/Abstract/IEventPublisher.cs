using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Abstract;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(DomainDefinition domain, TEvent @event, CancellationToken ct = default) where TEvent : IEvent;
}
