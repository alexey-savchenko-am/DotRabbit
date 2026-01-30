using DotRabbit.Core.Connection;
using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;

namespace DotRabbit.Core.Settings.Topology;

internal sealed class RmqTopologyManager
{
    private readonly RmqChannelPool _channelPool;

    public RmqTopologyManager(RmqChannelPool channelPool)
    {
        _channelPool = channelPool;
    }

    public async Task DeclareExchangeAsync(
        string name,
        string type = ExchangeType.Topic,
        CancellationToken ct = default)
    {
        await using var lease = await RmqChannelLease.CreateAsync(_channelPool);
        var channel = lease.Channel;

        await channel.ExchangeDeclareAsync(
            exchange: name,
            type: type,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);
    }

    public async Task DeclareQueueAsync(
        string name,
        QueueOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new QueueOptions();

        await using var lease = await RmqChannelLease.CreateAsync(_channelPool);
        var channel = lease.Channel;

        await channel.QueueDeclareAsync(
            queue: name,
            durable: options.Durable,
            exclusive: options.Exclusive,
            autoDelete: options.AutoDelete,
            arguments: options.ToArguments(),
            cancellationToken: ct);
    }

    public async Task BindQueueAsync(
        string queue,
        string exchange,
        string routingKey,
        CancellationToken ct = default)
    {
        await using var lease = await RmqChannelLease.CreateAsync(_channelPool);
        var channel = lease.Channel;

        await channel.QueueBindAsync(
            queue: queue,
            exchange: exchange,
            routingKey: routingKey,
            arguments: null,
            cancellationToken: ct);
    }
}
