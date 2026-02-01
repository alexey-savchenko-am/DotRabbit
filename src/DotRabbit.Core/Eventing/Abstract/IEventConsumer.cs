using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Eventing.Listeners;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventConsumer
{
    Task<ListenerSubscription> SubscribeAsync(
        Listener listener,
        IReadOnlyCollection<EventDefinition> events,
        CancellationToken ct = default);
}
