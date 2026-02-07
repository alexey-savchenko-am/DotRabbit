using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DotRabbit.Core.Eventing;

internal sealed class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IEventDefinitionRegistry _eventDefinitionRegistry;
    private readonly IEventSerializer _serializer;
    private readonly ITopologyResolver _topologyResolver;
    private readonly IMessageSender _messageSender;

    public EventPublisher(
        ILogger<EventPublisher> logger,
        IEventDefinitionRegistry eventDefinitionRegistry,
        IEventSerializer serializer,
        ITopologyResolver topologyResolver,
        IMessageSender messageSender)
    {
        _logger = logger;
        _eventDefinitionRegistry = eventDefinitionRegistry;
        _serializer = serializer;
        _topologyResolver = topologyResolver;
        _messageSender = messageSender;
    }

    public Task PublishAsync<TEvent>(
        DomainDefinition domain,
        TEvent @event,
        CancellationToken ct = default)
        where TEvent : IEvent
    {
        var exchange = _topologyResolver.ResolveExchange(domain);
        var routingKey = _topologyResolver.ResolveRoutingKey<TEvent>(domain);

        return PublishInternalAsync(domain, exchange, routingKey, @event, ct);
    }

    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken ct = default)
        where TEvent : IEvent
    {
        var domain = _eventDefinitionRegistry.GetDomainByEventType(typeof(TEvent))
            ?? throw new InvalidOperationException(
                $"Cannot get domain by event type {typeof(TEvent).Name}");

        var exchange = _topologyResolver.ResolveExchange<TEvent>()
            ?? throw new InvalidOperationException(
                $"Cannot get exchange by event type {typeof(TEvent).Name}");

        var routingKey = _topologyResolver.ResolveRoutingKey<TEvent>()
            ?? throw new InvalidOperationException(
                $"Cannot get routing key by event type {typeof(TEvent).Name}");

        return PublishInternalAsync(domain, exchange, routingKey, @event, ct);
    }

    private async Task PublishInternalAsync<TEvent>(
        DomainDefinition domain,
        string exchange,
        string routingKey,
        TEvent @event,
        CancellationToken ct)
        where TEvent : IEvent
    {
        var eventId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Publishing event {@EventType} with EventId={EventId}, Domain={Domain}, Exchange={Exchange}, RoutingKey={RoutingKey}",
            typeof(TEvent).Name,
            eventId,
            domain.Name,
            exchange,
            routingKey);

        try
        {
            var body = _serializer.Serialize(@event);

            var headers = new Dictionary<string, string?>
            {
                [MessageHeaders.EventId] = eventId,
                [MessageHeaders.Domain] = domain.Name,
                [MessageHeaders.EventName] = routingKey,
                [MessageHeaders.RetryCount] = "0",
                [MessageHeaders.PublishDateTime] = DateTime.UtcNow.ToString("O"),
            };

            var message = Message.CreateOutgoing(
                exchange,
                routingKey,
                Encoding.UTF8.GetBytes(body),
                headers.AsReadOnly());

            await _messageSender.SendAsync(message, ct);

            _logger.LogInformation(
                "Event published successfully. EventId={EventId}",
                eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event {@EventType} with EventId={EventId}, Domain={Domain}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                typeof(TEvent).Name,
                eventId,
                domain.Name,
                exchange,
                routingKey);

            throw;
        }
    }
}
