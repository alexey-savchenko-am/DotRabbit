using DotRabbit.Abstractions;
using DotRabbit.Core.Connection;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;
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
        string service,
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
            return new ServiceInfo(service.ToLowerInvariant());
        });


        services.TryAddSingleton<IEventDefinitionRegistry, EventDefinitionRegistry>();
        services.TryAddSingleton<IMessageToEventTransformer, MessageToEventTransformer>();
        services.TryAddSingleton<IEventProcessorInvoker, EventProcessorInvoker>();
     
        services.TryAddSingleton<ITopologyStrategy, RmqEventPerQueueTopologyStrategy>();
        services.TryAddSingleton<IEventSerializer, JsonBasedEventSerializer>();


        services.TryAddSingleton<IMessageRetryPolicy>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var serviceInfo = provider.GetRequiredService<IServiceInfo>();
            var sender = provider.GetRequiredService<IMessageSender>();

            return new RetryCountBasedExchangePolicy(
                logger: loggerFactory.CreateLogger<RetryCountBasedExchangePolicy>(),
                serviceInfo,
                sender,
                maxRetryCount: 5);
        });

        services.TryAddSingleton<IEventContainerFactory>(provider =>
        {
            var registry = provider.GetRequiredService<IEventDefinitionRegistry>();
            return new EventContainerFactory(registry.GetAll());
        });

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
    }
}
