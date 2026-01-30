using DotRabbit.Core.Events.Entities;
using DotRabbit.Core.Messaging.Entities;
using DotRabbit.Core.Settings.Entities;
namespace DotRabbit.Core.Events.Listeners;

public sealed record ListenerSubscription
    : IAsyncDisposable
{
    private readonly Action _onUnsubscribe;
    private Listener Listener { get; }
    private Dictionary<QueueDefinition, ConsumerSubscription> ConsumerSubscriptions { get; }
    private readonly object _lock = new();

    public ListenerSubscription(
        Listener listener,
        Dictionary<QueueDefinition, ConsumerSubscription> subscriptions,
        Action onUnsubscribe)
    {
        Listener = listener ?? throw new ArgumentNullException(nameof(listener));
        ConsumerSubscriptions = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
        _onUnsubscribe = onUnsubscribe;
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
