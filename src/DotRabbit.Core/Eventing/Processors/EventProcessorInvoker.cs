using DotRabbit.Core.Eventing.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Eventing.Processors;

/// <summary>
/// Singleton service.
/// Responsible for dispatching events to registered processors.
/// Thread-safe.
/// </summary>
internal sealed class EventProcessorInvoker
    : IEventProcessorInvoker
{
    private readonly ILogger<EventProcessorInvoker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessorInvoker(
        ILogger<EventProcessorInvoker> logger, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(IEventContainer<IEvent> @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var processors = scope.ServiceProvider
          .GetRequiredService<IEnumerable<IEventProcessor>>()
          .Where(p => p.CanProcess(@event))
          .ToList();

        if (processors.Count == 0)
        {
            _logger.LogWarning(
                "No processors registered for event {Event} in domain {Domain}",
                @event.Event.GetType().Name,
                @event.Domain.Name);

            return;
        }

        await Task.WhenAll(
            processors.Select(p => p.ProcessAsync(@event))
        );
    }
}
