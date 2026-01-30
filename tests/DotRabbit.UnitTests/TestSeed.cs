using AutoFixture;
using DotRabbit.Core.Events;
using DotRabbit.Core.Events.Abstract;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Serialize;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotRabbit.UnitTests;

public abstract class TestSeed
{
    public Fixture _fixture = new Fixture();
    protected List<IEvent> _events = [];
    protected IEventContainerFactory _eventContainerFactory;
    protected IEventSerializer _eventSerializer;
    protected IMessageToEventTransformer _messageToEventTransformer;

    protected TestSeed()
    {
        var userCreatedEvents = _fixture.CreateMany<UserCreatedTestEvent>(20);
        _events.AddRange(userCreatedEvents);

        _eventContainerFactory = new EventContainerFactory([typeof(UserCreatedTestEvent)]);
        _eventSerializer = new JsonBasedEventSerializer();
        _messageToEventTransformer = new MessageToEventTransformer(
                NullLogger<MessageToEventTransformer>.Instance,
                _eventContainerFactory,
                _eventSerializer);
    }
}
