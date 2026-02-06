using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace DotRabbit.Core.Messaging;

internal sealed class MessageToEventTransformer : IMessageToEventTransformer
{
    private readonly ILogger<MessageToEventTransformer> _logger;
    private readonly IEventDefinitionRegistry _eventDefinitionRegistry;
    private readonly IEventContainerFactory _eventContainerFactory;
    private readonly IEventSerializer _eventSerializer;

    public MessageToEventTransformer(
        ILogger<MessageToEventTransformer> logger,
        IEventDefinitionRegistry eventDefinitionRegistry,
        IEventContainerFactory eventContainerFactory,
        IEventSerializer eventSerializer)
    {
        _logger = logger;
        _eventDefinitionRegistry = eventDefinitionRegistry;
        _eventContainerFactory = eventContainerFactory;
        _eventSerializer = eventSerializer;
    }

    public IEventContainer<IEvent> Transform(IMessage message)
    {
        var sw = Stopwatch.StartNew();
        var id = message.GetRequiredHeader(MessageHeaders.EventId);
        var eventName = message.GetRequiredHeader(MessageHeaders.EventName);
        var domain = message.GetHeader(MessageHeaders.Domain) ?? message.Exchange;

        _logger.LogDebug("Transforming message id={Id} {Message} to event started.", id, message);

        var domainDefinition = new DomainDefinition(domain);

        try
        {
            var eventType = _eventDefinitionRegistry.GetByNameWithinDomain(eventName, domainDefinition).Type;
            var @event = _eventSerializer.Deserialize(message.BodyStr, eventType);
            var data = new EventContainerData(id, domainDefinition, message);

            var container = _eventContainerFactory.Create(data, @event);

            _logger.LogDebug("Transforming message id={Id} to event completed.", id);
            _logger.LogDebug("Transform message to event message id={Id} {Elapsed} ms", id, sw.ElapsedMilliseconds);

            return container;

        }
        catch (JsonException ex)
        {
            // invalid json payload in message
            _logger.LogError(ex, "Message id={Id} has an invalid json payload: {Body}", id, message.BodyStr);
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
