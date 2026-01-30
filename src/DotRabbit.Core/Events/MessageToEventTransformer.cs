using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Events.Entities;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;

namespace DotRabbit.Core.Events;

internal sealed class MessageToEventTransformer : IMessageToEventTransformer
{
    private readonly ILogger<MessageToEventTransformer> _logger;
    private readonly IEventContainerFactory _eventContainerFactory;
    private readonly IEventSerializer _eventSerializer;

    public MessageToEventTransformer(
        ILogger<MessageToEventTransformer> logger,
        IEventContainerFactory eventContainerFactory,
        IEventSerializer eventSerializer)
    {
        _logger = logger;
        _eventContainerFactory = eventContainerFactory;
        _eventSerializer = eventSerializer;
    }

    public IEventContainer<IEvent> Transform(IMessage message)
    {
        var eventName = message.GetRequiredHeader(MessageHeaders.EventName);
        var id = message.GetRequiredHeader(MessageHeaders.EventId);
        var domain = message.GetHeader(MessageHeaders.Domain) ?? message.Exchange;
        var eventError = message.GetHeader(MessageHeaders.EventErrorMessage);

        var eventType = Type.GetType(message.GetRequiredHeader(MessageHeaders.EventType))
            ?? throw new InvalidOperationException("Unknown event type");

        var @event = _eventSerializer.Deserialize(message.BodyStr, eventType);

        var data = new EventContainerData(id, new Domain(domain), eventError, message);

        var container = _eventContainerFactory.Create(data, @event);

        return container;
    }
}
