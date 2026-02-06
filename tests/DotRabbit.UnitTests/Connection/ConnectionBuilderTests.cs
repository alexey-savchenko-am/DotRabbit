using DotRabbit.Core.Connection;
using FluentAssertions;
using Xunit;

namespace DotRabbit.UnitTests.Connection;

public class ConnectionBuilderTests
{
    [Fact]
    public void FromConnectionString_ShouldReturnCorrectConfiguration_WhenValidConnectionStringProvided()
    {
        var connectionString = "amqp://guest:guest@localhost:5671/localhost";
        var config = RmqConfigurationBuilder
            .Create()
            .FromConnectionString(connectionString)
            .Build();

        var host = config.Hosts.Single();
        config.Scheme.Should().Be(Scheme.Amqp);
        config.UserName.Should().Be("guest");
        config.Password.Should().Be("guest");
        config.VirtualHost.Should().Be("localhost");
        host.Host.Should().Be("localhost");
        host.Port.Should().Be(5671);
    }
}
