using FluentAssertions;
using RabbitMQ.Client;
using Xunit;

namespace DotRabbit.IntegrationTests;

public class RmqAvailabilityIntegrationTest : IAsyncLifetime
{
    private RabbitMqFixture _rabbit;

    public async Task InitializeAsync()
    {
        _rabbit = new RabbitMqFixture();
        await _rabbit.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _rabbit.DisposeAsync();
    }

    [Fact]
    public async Task Can_connect_to_real_rmq()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_rabbit.ConnectionString)
        };

        using var connection = await factory.CreateConnectionAsync();

        connection.IsOpen.Should().BeTrue();
    }

}
