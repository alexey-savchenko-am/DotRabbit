using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.DomainEventGroup;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Eventing.Processors;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Configuration.Extensions;

public static class ConsumerServiceCollectionExtensions
{
    public static void AddEventSubscriber(
        this IServiceCollection services, 
        DomainDefinition domain,
        Func<EventProcessorBuilder, EventProcessorBuilder> builder)
    {
        var eventProcessorBuilder = builder(new EventProcessorBuilder(domain));

        // one factory per each Domain
        services.AddSingleton<IEventProcessorFactory>(
            eventProcessorBuilder.Build()
        );

        // one subscriber per each Domain
        services.AddSingleton<IDomainEventGroupSubscriber>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var consumer = provider.GetRequiredService<IEventConsumer>();
            var registry = provider.GetRequiredService<IEventDefinitionRegistry>();

            foreach (var eventType in eventProcessorBuilder.EventTypes)
                registry.Register(eventType, domain);

            var subscriberInfo = new DomainEventGroupSubscriberDefinition(Guid.NewGuid(), domain);
            
            var subscriber = new DomainEventGroupSubscriber(
               logger: loggerFactory.CreateLogger<DomainEventGroupSubscriber>(),
               consumer,
               subscriberInfo,
               registry
            );

            return subscriber;
        });
    }
}
