namespace DotRabbit.Core.Messaging.Abstract;

internal interface IMessageSender
{
    Task SendAsync(IMessage msg, CancellationToken ct = default);
}
