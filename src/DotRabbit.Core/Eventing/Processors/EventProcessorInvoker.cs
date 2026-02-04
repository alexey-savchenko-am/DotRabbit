using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;
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
    private readonly IReadOnlyDictionary<DomainDefinition, IEventProcessorFactory> _factories;

    public EventProcessorInvoker(
        ILogger<EventProcessorInvoker> logger, 
        IServiceScopeFactory serviceScopeFactory,
        IEnumerable<IEventProcessorFactory> factories)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _factories = factories.ToDictionary(f => f.Domain);
    }

    public async Task InvokeAsync(IEventContainer<IEvent> @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        if (!_factories.TryGetValue(@event.Domain, out var factory))
        {
            throw new InvalidOperationException(
                $"No processor factory for domain {@event.Domain.Name} registered");
        }

        var processor = factory.Resolve(
            @event.Event.GetType(), 
            scope.ServiceProvider);

        await processor.ProcessAsync(@event).ConfigureAwait(false);
    }
}
