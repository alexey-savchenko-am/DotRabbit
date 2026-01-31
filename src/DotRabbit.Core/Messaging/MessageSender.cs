using DotRabbit.Core.Connection;
using DotRabbit.Core.Messaging.Abstract;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

internal sealed class MessageSender : IMessageSender
{
    private readonly ILogger<MessageSender> _logger;
    private readonly RmqChannelPool _channelPool;

    public MessageSender(
        ILogger<MessageSender> logger,
        RmqChannelPool channelPool)
    {
        _logger = logger;
        _channelPool = channelPool;
    }

    public async Task SendAsync(IMessage msg, CancellationToken ct = default)
    {
        await using var lease = await RmqChannelLease.CreateAsync(_channelPool);
        var channel = lease.Channel;

        var props = new BasicProperties
        {
            Headers = msg.Headers.ToDictionary(),
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        try
        {
            await channel.BasicPublishAsync(
                exchange: msg.Exchange,
                routingKey: msg.RoutingKey,
                basicProperties: props,
                body: msg.Body,
                mandatory: true,
                cancellationToken: ct
            );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send message Exchange={Exchange} RoutingKey={RoutingKey}",
                msg.Exchange, msg.RoutingKey);

            throw;
        }
    }
}
