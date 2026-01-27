using DotRabbit.Abstractions;
using RabbitMQ.Client;

namespace DotRabbit.Core.Connection;

internal class RmqConnectionFactory
    : IRmqConnectionFactory
    , IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RmqConnectionFactory(RmqConnectionOptions options)
    {
        _factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.User,
            Password = options.Password,
        };
    }

    public async ValueTask<IConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true})
            return _connection;

        await _lock.WaitAsync(ct);

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _connection = await _factory.CreateConnectionAsync();
            return _connection;
        }
        finally
        {
            _lock.Release();
        }

    }

    public async ValueTask DisposeAsync()
    {
        if(_connection is not null)
            await _connection.DisposeAsync();
    }
}
