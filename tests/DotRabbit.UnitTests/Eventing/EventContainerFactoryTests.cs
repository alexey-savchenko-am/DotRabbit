using AutoFixture;
using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Entities;
using DotRabbit.Core.Messaging;
using DotRabbit.Core.Settings.Entities;
using FluentAssertions;
using Xunit;

namespace DotRabbit.UnitTests.Eventing;

public class EventContainerFactoryTests
    : TestSeed
{
    [Fact]
    public void Create_ReturnsEventContainer_WithRegisteredEventType()
    {
        var domain = _fixture.Create<DomainDefinition>();
        var registry = new EventDefinitionRegistry();
        registry.Register(typeof(UserCreatedTestEvent), domain);

        var factory = new EventContainerFactory([.. registry.GetAll()]);

        var msg = _fixture.Create<Message>();
   
        var id = _fixture.Create<string>();
        var data = new EventContainerData(id, domain, msg);
        var evt = _fixture.Create<UserCreatedTestEvent>();

        var containedEvent = factory.Create(data, evt);

        containedEvent.Should().NotBeNull();
        containedEvent.Event.Should().BeOfType<UserCreatedTestEvent>();
        containedEvent.Id.Should().Be(id);  
        containedEvent.Domain.Should().Be(domain);
    }
}
