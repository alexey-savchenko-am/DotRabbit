using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Messaging.Entities;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.DomainEventGroup;

public sealed record DomainEventGroupSubscription
    : IAsyncDisposable
{
    private readonly Action _onUnsubscribe;
    private DomainEventGroupSubscriberDefinition SubscriberDefinition { get; }
    private Dictionary<QueueDefinition, ConsumerSubscription> ConsumerSubscriptions { get; }
    private readonly object _lock = new();

    public DomainEventGroupSubscription(
        DomainEventGroupSubscriberDefinition subscriberDefinition,
        Dictionary<QueueDefinition, ConsumerSubscription> subscriptions,
        Action onUnsubscribe)
    {
        SubscriberDefinition = subscriberDefinition ?? throw new ArgumentNullException(nameof(subscriberDefinition));
        ConsumerSubscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
        _onUnsubscribe = onUnsubscribe;
    }

    public IEnumerable<ConsumerSubscription> CheckForCorruptedSubsriptions()
    {
        ConsumerSubscription[] snapshot;

        lock (_lock)
        {
            snapshot = ConsumerSubscriptions.Values.ToArray();
        }

        foreach (var sub in snapshot)
            if (sub.IsFaulted)
                yield return sub;
    }

    /// <summary>
    /// The method is called very rare (only on consumer restart).
    /// So, no need to use ConcurrentDictionary for ConsumerSubscriptions 
    /// </summary>
    public void ReplaceSubscription(QueueDefinition queue, ConsumerSubscription newSubscription)
    {
        lock (_lock)
        {
            ConsumerSubscriptions[queue] = newSubscription;
        }
    }

    public async ValueTask UnsubscribeAsync()
    {
        foreach (var (_, sub) in ConsumerSubscriptions)
        {
            await sub.UnsubscribeAsync();
        }

        _onUnsubscribe?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeAsync();
    }
}
