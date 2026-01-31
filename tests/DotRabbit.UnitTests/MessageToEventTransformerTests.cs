using AutoFixture;
using DotRabbit.Core.Messaging;
using FluentAssertions;
using RabbitMQ.Client;
using System.Text;
using Xunit;

namespace DotRabbit.UnitTests;

public class MessageToEventTransformerTests
    : TestSeed
{
    public MessageToEventTransformerTests()
    {
       
    }

    [Fact]
    public void Transform_WhenMessageProvided_ReturnsCorrectEventContainer()
    {
        // ARRANGE
        var fixture = new Fixture();
        var expectedEvent = fixture.Create<UserCreatedTestEvent>();
        var eventJson = _eventSerializer.Serialize(expectedEvent);
        
        var headers = new Dictionary<string, object>
        {
            { MessageHeaders.EventId, "123"},
            { MessageHeaders.EventName, nameof(UserCreatedTestEvent)},
            { MessageHeaders.EventType, typeof(UserCreatedTestEvent).AssemblyQualifiedName!},
            { MessageHeaders.Domain, "users"},
        };

        var msg = Message.CreateIncoming(
           deliveryStatusProducer: null,
           deliveryTag: 1,
           exchange: "users",
           routingKey: "tst",
           body: Encoding.UTF8.GetBytes(eventJson),
           headers: headers!);

        // ACT
        var container = _messageToEventTransformer.Transform(msg);

        // ASSERT
        container.Should().NotBeNull();
        container.Event.Should().BeOfType<UserCreatedTestEvent>();
        container.Event.Should().BeEquivalentTo(expectedEvent);
        container.Domain.Name.Should().Be("users");
        container.Id.Should().Be("123");
    }

}
