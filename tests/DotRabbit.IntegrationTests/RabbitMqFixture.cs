
using Testcontainers.RabbitMq;

namespace DotRabbit.IntegrationTests;

public sealed class RabbitMqFixture : IAsyncDisposable
{
    /// <summary>
    /// amqp://guest:guest@localhost:{port}
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    private readonly RabbitMqContainer _container;
    public RabbitMqFixture()
    {
        _container = new RabbitMqBuilder("rabbitmq:4.2-management").Build();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
