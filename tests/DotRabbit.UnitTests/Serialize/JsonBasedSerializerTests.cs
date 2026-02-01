using AutoFixture;
using FluentAssertions;
using Xunit;

namespace DotRabbit.UnitTests.Serialize;

public class JsonBasedSerializerTests
    : TestSeed
{
    [Fact]
    public void Serialize_Then_Deserialize_ReturnsEquivalentEvent()
    {
        var testEvent = _fixture.Create<UserCreatedTestEvent>();
        var json = _eventSerializer.Serialize(testEvent);
        var result = _eventSerializer.Deserialize<UserCreatedTestEvent>(json);

        result.UserId.Should().Be(testEvent.UserId);
        result.CreatedOnUtc.Should().Be(testEvent.CreatedOnUtc);
    }

    [Fact]
    public void Deserialize_WithType_ReturnsEvent()
    {
        var testEvent = _fixture.Create<UserCreatedTestEvent>();
        var json = _eventSerializer.Serialize(testEvent);
        var result = _eventSerializer.Deserialize(json, typeof(UserCreatedTestEvent));

        result.Should().BeOfType<UserCreatedTestEvent>();
    }
}
