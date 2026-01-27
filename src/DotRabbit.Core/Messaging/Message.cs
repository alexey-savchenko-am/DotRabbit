using DotRabbit.Core.Messaging.Abstract;
using System.Text;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed record Message
    : IMessage
{
    public DeliveryStatusCode Status { get; private set; }
    public ulong DeliveryTag { get; }
    public string Exchange { get; }
    public string RoutingKey { get; }
    public ReadOnlyMemory<byte> Body { get; }
    public IReadOnlyDictionary<string, object?> Headers { get; }

    //Lazy body content 
    private string? _bodyStr;
    private readonly ChannelWriter<DeliveryStatus> _deliveryStatusProducer;

    public string  BodyStr => _bodyStr ??= Encoding.UTF8.GetString(Body.Span);

    private Message(
        ChannelWriter<DeliveryStatus> deliveryStatusProducer,
        ulong deliveryTag,
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, object?> headers)
    {
        Status = DeliveryStatusCode.Delivered;
        _deliveryStatusProducer = deliveryStatusProducer;
        DeliveryTag = deliveryTag;
        Exchange = exchange;
        RoutingKey = routingKey;
        Body = body;
        Headers = headers;
    }

    public static Message Create(
        ChannelWriter<DeliveryStatus> deliveryStatusProducer,
        ulong deliveryTag,
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        IReadOnlyDictionary<string, object?>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(exchange);

        ArgumentNullException.ThrowIfNull(routingKey);

        return new Message(
            deliveryStatusProducer,
            deliveryTag,
            exchange,
            routingKey,
            body,
            headers ?? EmptyHeaders.Instance
        );
    }

    public ValueTask Ack()
    {
        Status = DeliveryStatusCode.Ack;
        return _deliveryStatusProducer.WriteAsync(new DeliveryStatus(Status, DeliveryTag));
    }

    public ValueTask Nack()
    {
        Status = DeliveryStatusCode.Nack;
        return _deliveryStatusProducer.WriteAsync(new DeliveryStatus(Status, DeliveryTag));
    }

    public override string ToString()
    {
        return $"{Exchange}:{RoutingKey}#{DeliveryTag}";
    }
}
