using RabbitMQ.Client;

namespace DotRabbit.Core.Connection;

public interface IRmqConnectionFactory
{
    ValueTask<IConnection> GetConnectionAsync(CancellationToken ct = default);
}
