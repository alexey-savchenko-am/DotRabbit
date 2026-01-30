using DotRabbit.Abstractions;
using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Events.Entities;
using DotRabbit.Core.Events.Listeners;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Entities;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DotRabbit.Core.Events;

internal sealed class RmqEventConsumer
    : IEventConsumer
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRmqConnectionFactory _connectionFactory;
    private readonly ILogger<RmqEventConsumer> _logger;
    private readonly MessageWorkerPool _workerPool;
    private readonly ITopologyStrategy _topologyStrategy;
    private readonly ConcurrentDictionary<Listener, ListenerSubscription> _subscriptions = [];
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

    public async Task<ListenerSubscription> SubscribeAsync(
        Listener listener,
        IReadOnlyCollection<Event> events,
        CancellationToken ct = default)
    {
        if (_subscriptions.ContainsKey(listener))
            throw new InvalidOperationException($"Listener {listener.Id} already subscribed");

        var queues = await _topologyStrategy
            .ProvisionTopologyAsync(new Service(""), listener.Domain, events)
            .ConfigureAwait(false);
       
        var connection = await _connectionFactory
            .GetConnectionAsync(ct)
            .ConfigureAwait(false);

        // IConnection is a thread safe, so don't worry about using it like shown below :)
        var subscriptionTasks = queues
            .Where(q => !q.IsDead)
            .Select(q => ConsumeQueueAsync(connection, listener.Domain, q, ct));

        var subscriptions = await Task.WhenAll(subscriptionTasks).ConfigureAwait(false);

        var listenerSubscription = new ListenerSubscription(
            listener, 
            subscriptions: subscriptions.ToDictionary(k => k.Queue), 
            onUnsubscribe: () => _subscriptions.TryRemove(listener, out _));

        if (!_subscriptions.TryAdd(listener, listenerSubscription))
            throw new InvalidOperationException($"Listener {listener.Id} already subscribed");

        return listenerSubscription;
    }


    private async Task<ConsumerSubscription> ConsumeQueueAsync(
        IConnection connection,
        Domain domain,
        QueueDefinition queue,
        CancellationToken ct)
    {
        // I don't use an object from RmqChannelPool for long-live Channels
        var channel = await connection
            .CreateChannelAsync(cancellationToken: ct)
            .ConfigureAwait(false);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)_workerPool.BufferSize,
            global: false,
            ct).ConfigureAwait(false);

        var deliveryStatusQueue = Channel.CreateUnbounded<DeliveryStatus>();

        var ackDispatcher = new AckNackDispatcher(
            _loggerFactory.CreateLogger<AckNackDispatcher>(),
            channel,
            deliveryStatusQueue);

        var consumer = new RmqMessageConsumer(
            _loggerFactory.CreateLogger<RmqMessageConsumer>(),
            domain,
            _workerPool,
            deliveryStatusQueue.Writer,
            channel);

        var consumerTag = await channel.BasicConsumeAsync(
            queue: queue.Name,
            autoAck: false,
            consumer,
            ct).ConfigureAwait(false);

        _logger.LogInformation("Started consumer {Queue} tag={Tag}", queue.Name, consumerTag);

        return new ConsumerSubscription(queue, channel, consumerTag);
    }

    private async Task RestartConsumerAsync(Listener listener, QueueDefinition queue)
    {
        if (!_restarting.TryAdd(queue.Name, 1))
            return;

        try
        {
            await Task.Delay(5000); //backoff

            if (!_subscriptions.TryGetValue(listener, out var existedListenerSub))
                throw new InvalidOperationException("Lister not found on consumer restart");

            var connection = await _connectionFactory
                .GetConnectionAsync()
                .ConfigureAwait(false);

            var subscription = await ConsumeQueueAsync(
                connection, 
                listener.Domain, 
                queue, 
                CancellationToken.None).ConfigureAwait(false);

            // we need to replace DEAD subscription with a fresh one
            existedListenerSub.ReplaceSubscription(queue, subscription);
        }
        finally
        {
            _restarting.TryRemove(queue.Name, out _);
        }
    }
}
