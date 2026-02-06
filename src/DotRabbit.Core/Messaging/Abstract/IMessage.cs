namespace DotRabbit.Core.Messaging.Abstract;

public interface IMessage
{
    /// <summary>
    /// Delivery identifier assigned by RabbitMQ for this message on a specific channel.
    /// Used for Ack/Nack/Reject operations.
    /// </summary>
    ulong DeliveryTag { get; }

    /// <summary>
    /// The name of the exchange from which the message was received.
    /// </summary>
    string Exchange { get; }

    /// <summary>
    /// The routing key used by RabbitMQ to route the message.
    /// </summary>
    string RoutingKey { get; }

    /// <summary>
    /// Raw binary payload of the message.
    /// </summary>
    ReadOnlyMemory<byte> Body { get; }

    string BodyStr { get; }

    /// <summary>
    /// Message metadata headers (AMQP headers table).
    /// </summary>
    IReadOnlyDictionary<string, string?> Headers { get; }

    /// <summary>
    /// The number of times the message was resent
    /// </summary>
    int RetryCount {  get; }

    string GetRequiredHeader(string headerKey);

    string? GetHeader(string headerKey);

    ValueTask AckAsync();

    ValueTask NackAsync();
}
