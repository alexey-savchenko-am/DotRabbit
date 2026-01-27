using RabbitMQ.Client;

namespace DotRabbit.Abstractions;

public interface IRmqConnectionFactory
{
    ValueTask<IConnection> GetConnectionAsync(CancellationToken ct = default);
}
