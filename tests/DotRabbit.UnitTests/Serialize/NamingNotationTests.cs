using DotRabbit.Core.Settings;
using FluentAssertions;
using Xunit;

namespace DotRabbit.UnitTests.Serialize;

public class NamingNotationTests
{
    [Theory]
    [InlineData(nameof(UserCreatedTestEvent), "user-created-test-event")]
    [InlineData(nameof(NamingNotationTests), "naming-notation-tests")]
    public void ToKebabNotation_WhenEventNameProvided_ReturnsCorrectKebabCaseString(string given, string expected)
    {
        var kebabCaseString = NamingNotations.ToKebabNotation(given);
        kebabCaseString.Should().Be(expected);
    }

    [Theory]
    [InlineData(nameof(UserCreatedTestEvent), "user.created.test.event")]
    [InlineData(nameof(NamingNotationTests), "naming.notation.tests")]
    public void ToDottedNotation_WhenEventNameProvided_ReturnsCorrectDottedCaseString(string given, string expected)
    {
        var kebabCaseString = NamingNotations.ToDottedNotation(given);
        kebabCaseString.Should().Be(expected);
    }
}
