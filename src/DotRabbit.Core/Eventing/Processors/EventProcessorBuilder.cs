using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DotRabbit.Core.Eventing.Processors;


public sealed class EventProcessorBuilder
{
    private readonly Dictionary<Type, Func<IServiceProvider, IEventProcessor>> _map = [];
    private readonly DomainDefinition _domain;

    public IReadOnlyList<Type> EventTypes =>
        [.. _map.Keys];

    public EventProcessorBuilder(DomainDefinition domain)
    {
        _domain = domain;
    }

    public EventProcessorBuilder SubscribeOn<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        _map[typeof(TEvent)] = sp =>
            new EventProcessor<TEvent, THandler>(
                _domain,
                sp.GetRequiredService<THandler>
            );

        return this;
    }

    public IEventProcessorFactory Build()
        => new EventProcessorFactory(_domain, _map);
}