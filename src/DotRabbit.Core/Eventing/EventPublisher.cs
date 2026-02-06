using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Topology;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DotRabbit.Core.Eventing;

internal sealed class EventPublisher
    : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IEventDefinitionRegistry _eventDefinitionRegistry;
    private readonly IServiceInfo _serviceInfo;
    private readonly IEventSerializer _serializer;
    private readonly IMessageSender _messageSender;

    public EventPublisher(
        ILogger<EventPublisher> logger,
        IEventDefinitionRegistry eventDefinitionRegistry,
        IServiceInfo serviceInfo,
        IEventSerializer serializer,
        IMessageSender messageSender)
    {
        _logger = logger;
        _eventDefinitionRegistry = eventDefinitionRegistry;
        _serviceInfo = serviceInfo;
        _serializer = serializer;
        _messageSender = messageSender;
    }

    public async Task PublishAsync<TEvent>(DomainDefinition domain, TEvent @event, CancellationToken ct = default) where TEvent : IEvent
    {
        try
        {
            var routingKey = _eventDefinitionRegistry
                .GetAllByDomain(domain)
                .FirstOrDefault(e => e.Type == typeof(TEvent))
                ?.Name;

            var service = _serviceInfo.GetInfo();
            var id = Guid.NewGuid().ToString();
            var exchange = RmqTopologyResolver.ResolveExchange(service, domain); // ex: user.users.topic
            //var routingKey = RmqTopologyResolver.ResolveRoutingKey<TEvent>(); // ex: order-created
            var bodyStr = _serializer.Serialize(@event);

            var headers = new Dictionary<string, string?>
            {
                {MessageHeaders.EventId, id },
                {MessageHeaders.Domain, domain.Name },
                {MessageHeaders.EventName, routingKey },
                {MessageHeaders.RetryCount, "0" },
                {MessageHeaders.PublishDateTime, DateTime.UtcNow.ToString() },
            };

            var msg = Message.CreateOutgoing(
                exchange,
                routingKey!,
                body: Encoding.UTF8.GetBytes(bodyStr),
                headers.AsReadOnly()
            );

            await _messageSender.SendAsync(msg, ct);
        }
        catch
        {
            throw;
        }

    }
}
