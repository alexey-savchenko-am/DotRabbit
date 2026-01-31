namespace DotRabbit.Core.Messaging;

internal static class MessageHeaders
{
    public const string EventId = "event-id";
    public const string Domain = "domain";
    public const string EventName = "event-name";
    public const string EventType = "event-type";
    public const string PublishDateTime = "x-event-date";
    public const string RetryCount = "x-retry-count";
}
