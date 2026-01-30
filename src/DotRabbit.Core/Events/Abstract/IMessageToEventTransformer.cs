using DotRabbit.Core.Messaging.Abstract;

namespace DotRabbit.Core.Events.Abstract;

public interface IMessageToEventTransformer
{
    IEventContainer<IEvent> Transform(IMessage message);
}