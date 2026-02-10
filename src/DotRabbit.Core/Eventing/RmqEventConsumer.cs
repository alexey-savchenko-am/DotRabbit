using DotRabbit.Abstractions;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.DomainEventGroup;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Entities;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace DotRabbit.Core.Eventing;

internal sealed class RmqEventConsumer
    : IEventConsumer
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRmqConnectionFactory _connectionFactory;
    private readonly ILogger<RmqEventConsumer> _logger;
    private readonly MessageWorkerPool _workerPool;
    private readonly ITopologyStrategy _topologyStrategy;
    private readonly ConcurrentDictionary<DomainEventGroupSubscriberDefinition, DomainEventGroupSubscription> _subscriptions = [];
    private readonly ConcurrentDictionary<string, int> _restarting = [];

    public RmqEventConsumer(
        ILoggerFactory loggerFactory,
        IRmqConnectionFactory connectionFactory,
        MessageWorkerPool workerPool,
        ITopologyStrategy topologyStrategy)
    {
        _loggerFactory = loggerFactory;
        _connectionFactory = connectionFactory;
        _logger = loggerFactory.CreateLogger<RmqEventConsumer>();
        _workerPool = workerPool;
        _topologyStrategy = topologyStrategy;
    }

    public async Task<DomainEventGroupSubscription> SubscribeAsync(
        DomainEventGroupSubscriberDefinition subscriberDefinition,
        IReadOnlyCollection<EventDefinition> events,
        CancellationToken ct = default)
    {
        if (_subscriptions.ContainsKey(subscriberDefinition))
            throw new InvalidOperationException($"Listener {subscriberDefinition.Id} already subscribed");

        var topology = await _topologyStrategy
            .ProvisionTopologyAsync(subscriberDefinition.Domain, events)
            .ConfigureAwait(false);

        var connection = await _connectionFactory.GetConnectionAsync(ct).ConfigureAwait(false);

        // IConnection is a thread safe, so don't worry about using it like shown below :)
        var subscriptionTasks = topology
            .Queues
            .Where(q => q.IsLiveDefinition)
            .Select(q => ConsumeQueueAsync(connection, subscriberDefinition.Domain, q, ct));

        var subscriptions = await Task.WhenAll(subscriptionTasks).ConfigureAwait(false);

        var listenerSubscription = new DomainEventGroupSubscription(
            subscriberDefinition,
            subscriptions: subscriptions.ToDictionary(k => k.Queue),
            onUnsubscribe: () => _subscriptions.TryRemove(subscriberDefinition, out _));

        if (!_subscriptions.TryAdd(subscriberDefinition, listenerSubscription))
            throw new InvalidOperationException($"Listener {subscriberDefinition.Id} already subscribed");

        return listenerSubscription;
    }

    public async Task RestartConsumerAsync(
        DomainEventGroupSubscriberDefinition subscriberDefinition,
        QueueDefinition queue,
        CancellationToken ct = default)
    {
        if (!_restarting.TryAdd(queue.Name, 1))
            return;

        try
        {
            if (!_subscriptions.TryGetValue(subscriberDefinition, out var existedListenerSub))
                throw new InvalidOperationException("Lister not found on consumer restart");

            var connection = await _connectionFactory
                .GetConnectionAsync(ct)
                .ConfigureAwait(false);

            var subscription = await ConsumeQueueAsync(
                connection,
                subscriberDefinition.Domain,
                queue,
                ct).ConfigureAwait(false);

            // we need to replace DEAD subscription with a fresh one
            existedListenerSub.ReplaceSubscription(queue, subscription);
        }
        finally
        {
            _restarting.TryRemove(queue.Name, out _);
        }
    }

    private async Task<ConsumerSubscription> ConsumeQueueAsync(
        IConnection connection,
        DomainDefinition domain,
        QueueDefinition queue,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // I don't use an object from RmqChannelPool for long-live Channels
        var channel = await connection
            .CreateChannelAsync(cancellationToken: ct)
            .ConfigureAwait(false);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _workerPool.BufferSize,
            global: false,
            ct).ConfigureAwait(false);

        var consumer = new RmqMessageConsumer(
            _loggerFactory.CreateLogger<RmqMessageConsumer>(),
            domain,
            _workerPool,
            channel);

        var consumerTag = await channel.BasicConsumeAsync(
            queue: queue.Name,
            autoAck: false,
            consumer,
            ct).ConfigureAwait(false);

        var subscription = new ConsumerSubscription(
            queue, 
            channel, 
            consumerTag);

        consumer.Faulted += subscription.MakeFaulted;

        _logger.LogInformation(
            "Started consumer for domain {Domain}, queue {Queue}, tag={Tag}",
            domain.Name,
            queue.Name,
            consumerTag);

        return subscription;
    }
}
