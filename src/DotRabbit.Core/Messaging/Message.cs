using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Messaging.Entities;
using System.Text;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed record Message
    : IMessage
{
    public MessageType Type { get; }
    public DeliveryStatusCode Status { get; private set; }
    public ulong DeliveryTag { get; }
    public string Exchange { get; }
    public string RoutingKey { get; }
    public ReadOnlyMemory<byte> Body { get; }
    public IReadOnlyDictionary<string, string?> Headers { get; }

    public int RetryCount =>
        Headers.TryGetValue(MessageHeaders.RetryCount, out var v)
            ? int.Parse(v ?? "0")
            : 0;

    //Lazy body content 
    private string? _bodyStr;
    private readonly ChannelWriter<DeliveryStatus> _deliveryStatusProducer;

    public string BodyStr => _bodyStr ??= Encoding.UTF8.GetString(Body.Span);

    private Message(
        MessageType type,
        ChannelWriter<DeliveryStatus> deliveryStatusProducer,
        ulong deliveryTag,
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, string?> headers)
    {
        Status = DeliveryStatusCode.Delivered;
        Type = type;
        _deliveryStatusProducer = deliveryStatusProducer;
        DeliveryTag = deliveryTag;
        Exchange = exchange;
        RoutingKey = routingKey;
        Body = body;
        Headers = headers;
    }

    public static Message CreateIncoming(
        ChannelWriter<DeliveryStatus> deliveryStatusProducer,
        ulong deliveryTag,
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, string?>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(exchange);

        ArgumentNullException.ThrowIfNull(routingKey);

        return new Message(
            MessageType.Incoming,
            deliveryStatusProducer,
            deliveryTag,
            exchange,
            routingKey,
            body,
            headers ?? EmptyHeaders.Instance
        );
    }

    public static Message CreateOutgoing(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, string?>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(exchange);

        ArgumentNullException.ThrowIfNull(routingKey);

        return new Message(
            MessageType.Outgoing,
            null!,
            0,
            exchange,
            routingKey,
            body,
            headers ?? EmptyHeaders.Instance
        );
    }


    public ValueTask AckAsync()
    {
        Status = DeliveryStatusCode.Ack;
        return _deliveryStatusProducer.WriteAsync(new DeliveryStatus(Status, DeliveryTag));
    }

    public ValueTask NackAsync()
    {
        Status = DeliveryStatusCode.Nack;
        return _deliveryStatusProducer.WriteAsync(new DeliveryStatus(Status, DeliveryTag));
    }

    public string GetRequiredHeader(string headerKey)
    {
        if (!Headers.TryGetValue(headerKey, out var header) || header is null)
            throw new KeyNotFoundException(headerKey);

        return header.ToString() ?? throw new KeyNotFoundException(headerKey);
    }

    public string? GetHeader(string headerKey)
    {
        if (!Headers.ContainsKey(headerKey))
            return null;

        return Headers[headerKey]?.ToString();
    }

    public override string ToString()
    {
        return $"{Exchange}:{RoutingKey}#{DeliveryTag}";
    }

    internal enum MessageType
    {
        Incoming = 0,
        Outgoing = 1
    }
}
