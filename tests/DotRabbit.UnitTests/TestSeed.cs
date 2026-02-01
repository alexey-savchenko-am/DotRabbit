using AutoFixture;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Settings;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Serialize;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotRabbit.UnitTests;

public abstract class TestSeed
{
    public readonly Fixture _fixture = new Fixture();
    protected readonly IServiceInfo _serviceInfo;
    protected readonly IEventContainerFactory _eventContainerFactory;
    protected readonly IEventSerializer _eventSerializer;
    protected readonly IMessageToEventTransformer _messageToEventTransformer;

    protected TestSeed()
    {
        _serviceInfo = new ServiceInfo("TestService");
        _eventContainerFactory = new EventContainerFactory([typeof(UserCreatedTestEvent)]);
        _eventSerializer = new JsonBasedEventSerializer();
        _messageToEventTransformer = new MessageToEventTransformer(
                NullLogger<MessageToEventTransformer>.Instance,
                _eventContainerFactory,
                _eventSerializer);
    }
}
