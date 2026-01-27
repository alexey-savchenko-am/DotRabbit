namespace DotRabbit.Core.Messaging;

internal readonly record struct DeliveryStatus(
    DeliveryStatusCode Status,
    ulong Tag
);

internal enum DeliveryStatusCode
{
    Delivered,
    Ack,
    Nack,
    Reject
}