using DotRabbit.Abstractions;
using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
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

    public async Task SubscribeAsync(
        Domain domain, 
        IReadOnlyCollection<Event> events,
        CancellationToken ct = default)
    {
        var queues = await _topologyStrategy
            .ProvisionTopologyAsync(new Service(""), domain, events);
       
        var connection = await _connectionFactory.GetConnectionAsync(ct);


        foreach(var queue in queues)
        {
            if(!queue.IsDead)
                await ConsumeQueueAsync(connection, domain, queue, ct);
        }
    }


    private async Task ConsumeQueueAsync(
        IConnection connection,
        Domain domain,
        Queue queue,
        CancellationToken ct)
    {
        var channel = await connection.CreateChannelAsync();

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)_workerPool.BufferSize,
            global: false,
            ct);

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
            ct);

        _logger.LogInformation("Started consumer {Queue} tag={Tag}", queue.Name, consumerTag);

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        finally
        {
            await channel.BasicCancelAsync(consumerTag);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }
    }


}
