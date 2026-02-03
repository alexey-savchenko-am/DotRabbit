using DotRabbit.Core.Eventing.DomainEventGroup;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventConsumer
{
    Task<DomainEventGroupSubscription> SubscribeAsync(
        DomainEventGroupSubscriberDefinition subscriberDefinition,
        IReadOnlyCollection<EventDefinition> events,
        CancellationToken ct = default);

    Task RestartConsumerAsync(
        DomainEventGroupSubscriberDefinition subscriberDefinition, 
        QueueDefinition queue,
        CancellationToken ct = default);
}
