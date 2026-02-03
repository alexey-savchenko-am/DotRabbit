
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Processors;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DotRabbit.Core.Configuration.Builders;

public sealed class EventProcessorBuilder
{
    private readonly List<Func<IServiceProvider, IEventProcessor>> _factories = [];
    private readonly DomainDefinition _domain;

    public EventProcessorBuilder(DomainDefinition domain)
    {
        _domain = domain;
    }

    public EventProcessorBuilder SubscribeOn<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        _factories.Add(sp =>
            new EventProcessor<TEvent, THandler>(
                _domain,
                sp.GetRequiredService<THandler>
            )
        );

        return this;
    }

    public IReadOnlyList<Func<IServiceProvider, IEventProcessor>> Build()
        => _factories;
}
