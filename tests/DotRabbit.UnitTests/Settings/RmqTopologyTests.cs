using DotRabbit.Core.Connection;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Settings;
using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Topology;
using FluentAssertions;
using Moq;
using System.Reflection;
using Xunit;

namespace DotRabbit.UnitTests.Settings;

public class RmqTopologyTests
    : TestSeed
{
    [Fact]
    public async Task ProvisionTopology_ReturnsValidTopologyNaming_WhenCorrectDomainAndEventsPassed()
    {
        var topologyManagerMock = new Mock<IRmqTopologyManager>(MockBehavior.Loose);
        var service = new ServiceInfo("UserService");
        var domain = new DomainDefinition("Users");
        var eventType = typeof(UserCreatedTestEvent);
        var eventName = eventType.GetCustomAttribute<EventNameAttribute>()!.EventName;
        var events = new List<EventDefinition>
        {
            new(eventName, eventType, domain)
        };

        var registry = new EventDefinitionRegistry();
        registry.Register(typeof(UserCreatedTestEvent), domain);

        var topologyResolver = new TopologyResolver(service, registry);
        var topologyStrategy = new RmqDomainQueueWithEventRoutingTopologyStrategy(topologyManagerMock.Object, topologyResolver);

        var topology = await topologyStrategy.ProvisionTopologyAsync(domain, events);

        topology.Should().NotBeNull();
        topology.Queues.Count.Should().Be(3);
        topology.Exchanges.Count.Should().Be(3);
        topology.Bindings.Count.Should().Be(3 * events.Count);
        topology.Queues.First(q => q.IsLiveDefinition).Name.Should().Be($"{service.GetInfo().Name}.{domain.Name}.q");
        topology.Exchanges.First(q => q.IsLiveDefinition).Name.Should().Be($"{domain.Name}.topic");
    }
}
