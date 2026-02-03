using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Eventing.DomainEventGroup;

internal sealed class DomainEventGroupSubscriberHostedService
    : IHostedService
{
    private readonly ILogger<DomainEventGroupSubscriberHostedService> _logger;
    private readonly IReadOnlyList<DomainEventGroupSubscriber> _subscribers;
    private int _stopped = 0;

    public DomainEventGroupSubscriberHostedService(
        ILogger<DomainEventGroupSubscriberHostedService> logger,
        IEnumerable<DomainEventGroupSubscriber> subscribers)
    {
        _logger = logger;
        _subscribers = subscribers.ToList();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {Count} event subscribers...", _subscribers.Count);

        var results = await Task.WhenAll(
            _subscribers.Select(s => s.SubscribeAsync(cancellationToken)));

        if (results.Any(r => !r))
            throw new InvalidOperationException("One or more subscribers failed to start");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _stopped, 1, 0) == 1)
            return;

        try
        {
            await Task.WhenAll(
                _subscribers.Select(s => s.UnsubscribeAsync(cancellationToken)));
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }
}
