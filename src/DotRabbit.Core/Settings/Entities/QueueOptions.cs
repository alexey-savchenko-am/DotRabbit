namespace DotRabbit.Core.Settings.Entities;

public sealed class QueueOptions
{
    public bool Durable { get; init; } = true;
    public bool Exclusive { get; init; } = false;
    public bool AutoDelete { get; init; } = false;

    public string? DeadLetterExchange { get; init; }
    public string? DeadLetterRoutingKey { get; init; }
    public TimeSpan? MessageTtl { get; init; }

    internal IDictionary<string, object> ToArguments()
    {
        var args = new Dictionary<string, object>();

        if (DeadLetterExchange != null)
            args["x-dead-letter-exchange"] = DeadLetterExchange;

        if (DeadLetterRoutingKey != null)
            args["x-dead-letter-routing-key"] = DeadLetterRoutingKey;

        if (MessageTtl.HasValue)
            args["x-message-ttl"] = (int)MessageTtl.Value.TotalMilliseconds;

        return args;
    }
}