using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Topology;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Messaging;

/// <summary>
/// Retry policy that routes messages to a retry exchange while the retry count
/// is below the configured limit and to a dead-letter exchange (DLX) once the
/// maximum retry count is exceeded.
/// </summary>
/// <remarks>
/// This policy is stateless and is intended to be registered as a Singleton.
/// It relies solely on message metadata (RetryCount) and configuration values
/// and does not keep any per-message or per-request state.
/// </remarks>
internal class RetryCountBasedExchangePolicy
    : IMessageRetryPolicy
{
    public int MaxRetryCount { get; }

    private readonly ILogger<RetryCountBasedExchangePolicy> _logger;
    private readonly ITopologyResolver _topologyResolver;
    private readonly IMessageSender _messageSender;

    public RetryCountBasedExchangePolicy(
        ILogger<RetryCountBasedExchangePolicy> logger,
        ITopologyResolver topologyResolver,
        IMessageSender messageSender,
        int maxRetryCount = 5)
    {
        _logger = logger;
        _topologyResolver = topologyResolver;
        _messageSender = messageSender;
        MaxRetryCount = maxRetryCount;
    }

    public async Task RetryAsync(IMessage msg, CancellationToken ct = default)
    {
        try
        {
            var outgoingMsg = BuildOutgoingMessage(msg, isRetry: msg.RetryCount < MaxRetryCount);

            await _messageSender.SendAsync(outgoingMsg, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send outgoing message");
        }

    }

    private IMessage BuildOutgoingMessage(IMessage msg, bool isRetry)
    {
        var domain = new DomainDefinition(msg.GetRequiredHeader(MessageHeaders.Domain));

        var exchange = isRetry 
            ? _topologyResolver.ResolveRetryExchange(domain) 
            : _topologyResolver.ResolveDlxExchange(domain);

        var headers = new Dictionary<string, string?>
        {
            {MessageHeaders.EventId, msg.GetHeader(MessageHeaders.EventId)},
            {MessageHeaders.Domain, domain.Name },
            {MessageHeaders.EventName, msg.GetRequiredHeader(MessageHeaders.EventName) },
            {MessageHeaders.RetryCount, (msg.RetryCount + 1).ToString() },
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
