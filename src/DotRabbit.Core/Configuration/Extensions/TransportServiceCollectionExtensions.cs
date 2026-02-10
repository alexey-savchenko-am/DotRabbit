using DotRabbit.Abstractions;
using DotRabbit.Core.Connection;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.DomainEventGroup;
using DotRabbit.Core.Eventing.Processors;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Serialize;
using DotRabbit.Core.Settings.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Configuration.Extensions;

public static class TransportServiceCollectionExtensions
{
    public static void AddRmqTransport(
        this IServiceCollection services, 
        string serviceName,
        Func<RmqConfigurationBuilder, RmqConfigurationBuilder> configBuilder)
    {
        // ----------------- Connection------------------------------

        var config = configBuilder(RmqConfigurationBuilder.Create()).Build();

        services.AddSingleton<IRmqConnectionFactory>(provider =>
        {
            return new RmqConnectionFactory(config);
        });

        services.AddSingleton<RmqChannelPool>();

        // -----------------------------------------------------------

        services.AddSingleton<IServiceInfo>(_ =>
        {
            return new ServiceInfo(serviceName.ToLowerInvariant());
        });


        services.TryAddSingleton<IEventDefinitionRegistry, EventDefinitionRegistry>();
        services.TryAddSingleton<IMessageToEventTransformer, MessageToEventTransformer>();
        services.TryAddSingleton<IEventProcessorInvoker, EventProcessorInvoker>();
     
        services.TryAddSingleton<ITopologyStrategy, RmqDomainQueueWithEventRoutingTopologyStrategy>();
        services.TryAddSingleton<IEventSerializer, JsonBasedEventSerializer>();
        services.AddSingleton<IRmqTopologyManager, RmqTopologyManager>();
        services.AddSingleton<ITopologyResolver, TopologyResolver>();

        services.TryAddSingleton<IMessageRetryPolicy>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var sender = provider.GetRequiredService<IMessageSender>();
            var topologyResolver = provider.GetRequiredService<ITopologyResolver>();

            return new RetryCountBasedExchangePolicy(
                logger: loggerFactory.CreateLogger<RetryCountBasedExchangePolicy>(),
                topologyResolver,
                sender,
                maxRetryCount: 5);
        });

        services.TryAddSingleton<IEventContainerFactory, EventContainerFactory>();

        services.TryAddSingleton(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var transformer = provider.GetRequiredService<IMessageToEventTransformer>();
            var invoker = provider.GetRequiredService<IEventProcessorInvoker>();
            var retryPolicy = provider.GetRequiredService<IMessageRetryPolicy>();

            return new MessageWorkerPool(
                logger: loggerFactory.CreateLogger<MessageWorkerPool>(),
                transformer,
                invoker,
                retryPolicy,
                workerCount: Environment.ProcessorCount,
                bufferSize: (ushort)(Environment.ProcessorCount * 4));
        });

        services.TryAddSingleton<IEventConsumer, RmqEventConsumer>();

        services.TryAddSingleton<IEventPublisher, EventPublisher>();
        services.TryAddSingleton<IMessageSender, MessageSender>();

        services.AddHostedService<DomainEventGroupSubscriberHostedService>();
        services.AddHostedService(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var subscribers = provider.GetRequiredService<IEnumerable<IDomainEventGroupSubscriber>>();

            return new DomainEventGroupSubscriberHealthCheckService(
                logger: loggerFactory.CreateLogger<DomainEventGroupSubscriberHealthCheckService>(),
                subscribers,
                TimeSpan.FromSeconds(5));
        });
        services.AddHostedService<MessageWorkerPoolHostedService>();
        
    }
}
