using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Eventing.DomainEventGroup;

internal class DomainEventGroupSubscriber : IAsyncDisposable
{
    private readonly ILogger<DomainEventGroupSubscriber> _logger;
    private readonly IEventConsumer _eventConsumer;
    private readonly DomainEventGroupSubscriberDefinition _subscriberDefinition;
    private readonly IReadOnlyList<EventDefinition> _handleEvents;
    private DomainEventGroupSubscription? _subscription;
    private int _healthChecking;

    public DomainEventGroupSubscriber(
        ILogger<DomainEventGroupSubscriber> logger,
        IEventConsumer eventConsumer,
        DomainEventGroupSubscriberDefinition subscriberDefinition,
        IReadOnlyList<EventDefinition> handleEvents)
    {
        _logger = logger;
        _eventConsumer = eventConsumer;
        _subscriberDefinition = subscriberDefinition;
        _handleEvents = handleEvents;
    }

    public async Task<bool> SubscribeAsync(CancellationToken ct = default)
    {
        try
        {
            _subscription = await _eventConsumer.SubscribeAsync(
                _subscriberDefinition,
                _handleEvents,
                ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe {Subscriber}", _subscriberDefinition.Id);
            return false;
        }
    }

    public async Task UnsubscribeAsync(CancellationToken ct = default)
    {
        var sub = Interlocked.Exchange(ref _subscription, null);
        if (sub is not null)
        {
            _logger.LogWarning("Unsubscribing subscriptions for {Subscriber} subscriber",
                _subscriberDefinition.Id);

            await sub.UnsubscribeAsync(ct);
        }
    }

    public async Task HealthCheckAsync(CancellationToken ct = default)
    {
        // already checking 
        if (Interlocked.CompareExchange(ref _healthChecking, 1, 0) == 1)
            return;

        try
        {
            var subscription = _subscription;

            if (subscription is null)
                return;

            var corrupted = subscription
                .CheckForCorruptedSubscriptions()
                .ToArray();

            if (corrupted.Length == 0)
                return;

            _logger.LogWarning("Restarting {Count} corrupted subscriptions for {Subscriber}",
                corrupted.Length,
                _subscriberDefinition.Id);

            await Task.WhenAll(
                corrupted.Select(sub =>
                    _eventConsumer.RestartConsumerAsync(_subscriberDefinition, sub.Queue, ct)))
                        .ConfigureAwait(false);
        }
        finally
        {
            Volatile.Write(ref _healthChecking, 0);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeAsync();
    }
}
