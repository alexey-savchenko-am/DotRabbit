using AutoFixture;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Settings.Entities;
using DotRabbit.Core.Settings.Topology;
using FluentAssertions;
using System.Text;
using Xunit;

namespace DotRabbit.UnitTests.Messaging;

public class MessageToEventTransformerTests
    : TestSeed
{
    [Fact]
    public void Transform_WhenMessageProvided_ReturnsCorrectEventContainer()
    {
        // ARRANGE
        var expectedEvent = _fixture.Create<UserCreatedTestEvent>();
        var eventJson = _eventSerializer.Serialize(expectedEvent);

        var eventId = _fixture.Create<string>();
        var domain = new DomainDefinition("users");

        var eventDef = _eventDefinitionRegistry.Register(typeof(UserCreatedTestEvent), domain);

        var headers = new Dictionary<string, string>
        {
            { MessageHeaders.EventId, eventId},
            { MessageHeaders.EventName, eventDef.Name},
            { MessageHeaders.Domain, domain.Name},
        };

        var msg = Message.CreateIncoming(
           deliveryStatusProducer: null!,
           deliveryTag: _fixture.Create<ulong>(),
           exchange: _topologyResolver.ResolveExchange(domain),
           routingKey: _topologyResolver.ResolveRoutingKey<UserCreatedTestEvent>(),
           body: Encoding.UTF8.GetBytes(eventJson),
           headers: headers!);

        // ACT
        var container = _messageToEventTransformer.Transform(msg);

        // ASSERT
        container.Should().NotBeNull();
        container.Event.Should().BeOfType<UserCreatedTestEvent>();
        container.Event.Should().BeEquivalentTo(expectedEvent);
        container.Domain.Should().Be(domain);
        container.Id.Should().Be(eventId);
    }
}
