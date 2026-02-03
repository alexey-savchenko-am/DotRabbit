using DotRabbit.Core.Configuration.Builders;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.DomainEventGroup;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Eventing.Processors;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Configuration.Extensions;

public static class ConsumerServiceCollectionExtensions
{
    public static void AddEventSubscriber(
        this IServiceCollection services, 
        DomainDefinition domain,
        Func<EventProcessorBuilder, EventProcessorBuilder> builder)
    {
        var factories = builder(new EventProcessorBuilder(domain)).Build();

        services.TryAddSingleton(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var consumer = provider.GetRequiredService<IEventConsumer>();
            var subscriberInfo = new DomainEventGroupSubscriberDefinition(Guid.NewGuid(), domain);

            var processors = new List<IEventProcessor>(factories.Count);
            foreach (var factory in factories)
                processors.Add(factory(provider));

            var subscriber = new DomainEventGroupSubscriber(
               logger: loggerFactory.CreateLogger<DomainEventGroupSubscriber>(),
               consumer,
               subscriberInfo,
               processors
            );
            return subscriber;
        });

    }
}
