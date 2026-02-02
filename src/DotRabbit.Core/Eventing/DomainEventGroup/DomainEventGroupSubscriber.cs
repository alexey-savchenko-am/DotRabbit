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
    private DomainEventGroupSubscription _subscription;
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

    public async ValueTask HealthCheckAsync()
    {
        // already checking 
        if (Interlocked.Exchange(ref _healthChecking, 1) == 1)
            return;

        if (_subscription is null)
            return;

        try
        {
            var corrupted = _subscription
                .CheckForCorruptedSubsriptions()
                .ToArray();

            if (corrupted.Length == 0)
                return;

            await Task.WhenAll(
                corrupted.Select(sub =>
                    _eventConsumer.RestartConsumerAsync(_subscriberDefinition, sub.Queue)))
                        .ConfigureAwait(false);
        }
        finally
        {
            Volatile.Write(ref _healthChecking, 0);
        }
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
