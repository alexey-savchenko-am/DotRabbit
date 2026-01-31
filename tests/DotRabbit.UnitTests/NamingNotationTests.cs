using DotRabbit.Core.Settings;
using FluentAssertions;
using Xunit;

namespace DotRabbit.UnitTests;

public class NamingNotationTests
{
    [Fact]
    public void ToKebabNotation_WhenEventNameProvided_ReturnsCorrectKebabCaseString()
    {
        var kebabCaseString = NamingNotations.ToKebabNotation(nameof(UserCreatedTestEvent));
        kebabCaseString.Should().Be("user-created-test-event");  
    }

    [Fact]
    public void ToDottedNotation_WhenEventNameProvided_ReturnsCorrectDottedCaseString()
    {
        var kebabCaseString = NamingNotations.ToDottedNotation(nameof(UserCreatedTestEvent));
        kebabCaseString.Should().Be("user.created.test.event");
    }
}
