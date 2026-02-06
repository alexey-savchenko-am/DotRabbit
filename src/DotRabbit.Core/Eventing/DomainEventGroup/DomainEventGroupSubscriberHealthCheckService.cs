using DotRabbit.Core.Eventing.Abstract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Eventing.DomainEventGroup;

internal sealed class DomainEventGroupSubscriberHealthCheckService
    : BackgroundService
{
    private readonly ILogger<DomainEventGroupSubscriberHealthCheckService> _logger;
    private readonly TimeSpan _period;
    private readonly IReadOnlyList<IDomainEventGroupSubscriber> _subscribers;

    public DomainEventGroupSubscriberHealthCheckService(
        ILogger<DomainEventGroupSubscriberHealthCheckService> logger,
        IEnumerable<IDomainEventGroupSubscriber> subscribers,
        TimeSpan period)
    {
        _logger = logger;
        _period = period;
        _subscribers = subscribers.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                await Task.WhenAll(
                  _subscribers.Select(s => s.HealthCheckAsync(stoppingToken)));
            }
            catch(OperationCanceledException)
            {
                break;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in event subscriber healt check");
            }
        }
    }
}
