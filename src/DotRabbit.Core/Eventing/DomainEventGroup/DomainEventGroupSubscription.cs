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

    public IEnumerable<ConsumerSubscription> CheckForCorruptedSubscriptions()
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

    public async ValueTask UnsubscribeAsync(CancellationToken ct = default)
    {
        var fullyUnsubscribed = true;

        foreach (var (_, sub) in ConsumerSubscriptions)
        {
            try
            {
                await sub.UnsubscribeAsync(ct);
            }
            catch (OperationCanceledException)
            {
                fullyUnsubscribed = false;
                break;
            }
        }

        if (fullyUnsubscribed)
            _onUnsubscribe?.Invoke();
    }

    public ValueTask DisposeAsync()
    {
        return UnsubscribeAsync();
    }
}
