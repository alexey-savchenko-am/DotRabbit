using DotRabbit.Core.Eventing.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace DotRabbit.Core.Eventing.Processors;

// Singleton
internal sealed class EventProcessorInvoker
    : IEventProcessorInvoker
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessorInvoker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task InvokeAsync(IEventContainer<IEvent> @event)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var processors = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IEventProcessor>>();

        var processor = processors.FirstOrDefault(p => p.CanProcess(@event));

        if (processor is null)
        {
            throw new InvalidOperationException(
                $"No processor registered for event {@event.Event.GetType().Name} " +
                $"in domain {@event.Domain.Name}");
        }

        return processor.ProcessAsync(@event);
    }
}
