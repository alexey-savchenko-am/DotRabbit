using DotRabbit.Core.Messaging.Abstract;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IMessageToEventTransformer
{
    IEventContainer<IEvent> Transform(IMessage message);
}