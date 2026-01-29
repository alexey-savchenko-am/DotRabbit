using DotRabbit.Core.Events.Entities;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Abstract;

public interface IEventConsumer
{
    Task<ListenerSubscription> SubscribeAsync(
        Listener listener,
        IReadOnlyCollection<Event> events,
        CancellationToken ct = default);
}
