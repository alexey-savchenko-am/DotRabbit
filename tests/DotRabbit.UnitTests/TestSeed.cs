using AutoFixture;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Settings;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Serialize;
using DotRabbit.Core.Settings.Topology;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotRabbit.UnitTests;

public abstract class TestSeed
{
    public readonly Fixture _fixture = new ();
    protected readonly IServiceInfo _serviceInfo;
    protected readonly IEventContainerFactory _eventContainerFactory;
    protected readonly IEventSerializer _eventSerializer;
    protected readonly IMessageToEventTransformer _messageToEventTransformer;
    protected readonly IEventDefinitionRegistry _eventDefinitionRegistry;
    protected readonly ITopologyResolver _topologyResolver;
    

    protected TestSeed()
    {
        _serviceInfo = new ServiceInfo("TestService");
        _eventDefinitionRegistry = new EventDefinitionRegistry();
        _eventDefinitionRegistry.Register(typeof(UserCreatedTestEvent), new DomainDefinition("users"));
        _eventContainerFactory = new EventContainerFactory();
        _eventContainerFactory.Register(typeof(UserCreatedTestEvent));
        _eventSerializer = new JsonBasedEventSerializer();
        _topologyResolver = new TopologyResolver(_serviceInfo, _eventDefinitionRegistry);

        _messageToEventTransformer = new MessageToEventTransformer(
                NullLogger<MessageToEventTransformer>.Instance,
                _eventDefinitionRegistry,
                _eventContainerFactory,
                _eventSerializer);
    }
}
