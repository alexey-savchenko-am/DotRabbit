namespace DotRabbit.Core.Messaging.Entities;

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