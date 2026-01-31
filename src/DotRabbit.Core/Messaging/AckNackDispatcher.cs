using DotRabbit.Core.Messaging.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed class AckNackDispatcher
{
    private readonly ILogger<AckNackDispatcher> _logger;
    private readonly IChannel _channel;
    private readonly Channel<DeliveryStatus> _queue;
    private readonly Task _ackNackTask;

    public AckNackDispatcher(
        ILogger<AckNackDispatcher> logger, 
        IChannel channel, 
        Channel<DeliveryStatus> queue)
    {
        _logger = logger;
        _channel = channel;
        _queue = queue;
        _ackNackTask = Task.Run(Loop);
    }

    private async Task Loop()
    {
        _logger.LogInformation("AckNackDispatcher started");

        await foreach(var s in _queue.Reader.ReadAllAsync())
        {
            try
            {
                switch (s.Status)
                {
                    case DeliveryStatusCode.Ack:
                        await _channel.BasicAckAsync(s.Tag, false);
                        break;
                    case DeliveryStatusCode.Nack:
                        await _channel.BasicNackAsync(s.Tag, false, requeue: false);
                        break;
                }

                _logger.LogDebug("ACK status {Status} tag={Tag}", s.Status, s.Tag);
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, "AckNackDispatcher failed tag={Tag}", s.Tag);
            }
        }
    }
}
