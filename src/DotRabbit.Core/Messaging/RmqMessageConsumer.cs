using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed class RmqMessageConsumer
    : AsyncDefaultBasicConsumer
{
    private readonly Domain _domain;
    private readonly MessageWorkerPool _workerPool;
    private readonly ChannelWriter<DeliveryStatus> _deliveryStatusProducer;
    private readonly ILogger _logger;

    public RmqMessageConsumer(
        ILogger<RmqMessageConsumer> logger,
        Domain domain,
        MessageWorkerPool workerPool,
        ChannelWriter<DeliveryStatus> deliveryStatusProducer,
        IChannel channel) 
        : base(channel)
    {
        _domain = domain;
        _workerPool = workerPool;
        _deliveryStatusProducer = deliveryStatusProducer;
        _logger = logger;
    }

    public override async Task HandleBasicDeliverAsync(
        string consumerTag, 
        ulong deliveryTag, 
        bool redelivered, 
        string exchange, 
        string routingKey, 
        IReadOnlyBasicProperties properties, 
        ReadOnlyMemory<byte> body, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = properties.Headers?
                .ToDictionary(k => k.Key, v => v.Value);

            var msg = Message.Create(
                _deliveryStatusProducer, 
                deliveryTag, 
                exchange, 
                routingKey, 
                body, 
                headers);

            await _workerPool.EnqueueAsync(msg);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the message. Message tag {DeliveryTag} is about to REJECT.", deliveryTag);

            await _deliveryStatusProducer.WriteAsync(
              new DeliveryStatus(DeliveryStatusCode.Reject, deliveryTag), 
              cancellationToken);
        }
    }
}
