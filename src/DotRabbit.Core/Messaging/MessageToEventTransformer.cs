using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Messaging;

internal sealed class MessageToEventTransformer : IMessageToEventTransformer
{
    private readonly ILogger<MessageToEventTransformer> _logger;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly IEventContainerFactory _eventContainerFactory;
    private readonly IEventSerializer _eventSerializer;

    public MessageToEventTransformer(
        ILogger<MessageToEventTransformer> logger,
        IEventTypeRegistry eventTypeRegistry,
        IEventContainerFactory eventContainerFactory,
        IEventSerializer eventSerializer)
    {
        _logger = logger;
        _eventTypeRegistry = eventTypeRegistry;
        _eventContainerFactory = eventContainerFactory;
        _eventSerializer = eventSerializer;
    }

    public IEventContainer<IEvent> Transform(IMessage message)
    {
        var id = message.GetRequiredHeader(MessageHeaders.EventId);
        var eventTypeStr = message.GetRequiredHeader(MessageHeaders.EventType);
        var eventName = message.GetHeader(MessageHeaders.EventName);
        var domain = message.GetHeader(MessageHeaders.Domain) ?? message.Exchange;

        _logger.LogDebug("Transforming message id={Id} {Message} to event started.", id, message);

        var eventType = _eventTypeRegistry.GetByName(eventTypeStr);

        var @event = _eventSerializer.Deserialize(message.BodyStr, eventType);

        var data = new EventContainerData(id, new DomainDefinition(domain), message);

        var container = _eventContainerFactory.Create(data, @event);

        _logger.LogDebug("Transforming message id={Id} to event completed.", id);

        return container;
    }
}
