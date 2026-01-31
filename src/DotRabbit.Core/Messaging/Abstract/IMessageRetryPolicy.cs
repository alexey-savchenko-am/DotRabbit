namespace DotRabbit.Core.Messaging.Abstract;

internal interface IMessageRetryPolicy
{
    Task RetryAsync(IMessage msg, CancellationToken ct = default);
}
