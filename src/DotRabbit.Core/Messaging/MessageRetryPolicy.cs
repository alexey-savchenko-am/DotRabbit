using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Topology;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Messaging;

internal class MessageRetryPolicy
    : IMessageRetryPolicy
{
    public int MaxRetryCount { get; }

    private readonly ILogger<MessageRetryPolicy> _logger;
    private readonly IServiceInfo _serviceInfo;
    private readonly IMessageSender _messageSender;

    public MessageRetryPolicy(
        ILogger<MessageRetryPolicy> logger,
        IServiceInfo serviceInfo,
        IMessageSender messageSender,
        int maxRetryCount = 5)
    {
        _logger = logger;
        _serviceInfo = serviceInfo;
        _messageSender = messageSender;
        MaxRetryCount = maxRetryCount;
    }

    public async Task RetryAsync(IMessage msg, CancellationToken ct = default)
    {
        try
        {
            var outgoingMsg = BuildOutgoingMessage(msg, isRetry: msg.RetryCount < MaxRetryCount);

            await _messageSender.SendAsync(outgoingMsg, ct).ConfigureAwait(false);
            await msg.AckAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send outgoing message");
        }

    }

    private IMessage BuildOutgoingMessage(IMessage msg, bool isRetry)
    {
        var service = _serviceInfo.GetInfo();

        var domain = new DomainDefinition(msg.GetRequiredHeader(MessageHeaders.Domain));

        var exchange = isRetry 
            ? RmqTopologyResolver.ResolveRetryExchange(service, domain) 
            : RmqTopologyResolver.ResolveDlxExchange(service, domain);

        var headers = new Dictionary<string, object?>
        {
            {MessageHeaders.EventId, msg.GetHeader(MessageHeaders.EventId)},
            {MessageHeaders.Domain, domain },
            {MessageHeaders.EventName, msg.GetRequiredHeader(MessageHeaders.EventName) },
            {MessageHeaders.RetryCount, msg.RetryCount + 1 },
            {MessageHeaders.PublishDateTime, DateTime.UtcNow.ToString() },
        };

        var outgoingMessage = Message.CreateOutgoing(
            exchange,
            routingKey: msg.RoutingKey, // ex: order-created
            body: msg.Body,
            headers.AsReadOnly());

        return outgoingMessage;
    }
}
