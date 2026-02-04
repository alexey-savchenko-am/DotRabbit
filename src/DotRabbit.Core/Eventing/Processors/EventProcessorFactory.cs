using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Processors;

internal sealed class EventProcessorFactory
    : IEventProcessorFactory
{
    public DomainDefinition Domain { get; }
    private readonly IReadOnlyDictionary<Type, Func<IServiceProvider, IEventProcessor>> _map;

    public EventProcessorFactory(
        DomainDefinition domain,
        IReadOnlyDictionary<Type, Func<IServiceProvider, IEventProcessor>> map)
    {
        Domain = domain;
        _map = map;
    }

    public IEventProcessor Resolve(
        Type eventType, 
        IServiceProvider serviceProvider)
    {
        if (!_map.TryGetValue(eventType, out var factory))
        {
            throw new InvalidOperationException(
                $"No handler for event {eventType.Name} in domain {Domain.Name}"
            );
        }

        return factory(serviceProvider);
    }
}
