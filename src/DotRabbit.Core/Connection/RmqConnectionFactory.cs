using RabbitMQ.Client;

namespace DotRabbit.Core.Connection;

internal class RmqConnectionFactory
    : IRmqConnectionFactory
    , IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly AmqpTcpEndpoint[] _endpoints = [];

    public RmqConnectionFactory(RmqConnectionConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            UserName = config.UserName!,
            Password = config.Password!,
            VirtualHost = config.VirtualHost,
            AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled,
            ConsumerDispatchConcurrency = 2
        };

        if (config.Scheme == Scheme.Amqps)
        {
            _factory.Ssl.Enabled = true;
            _factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
        }

        _endpoints = config.Hosts
            .Select(h => new AmqpTcpEndpoint(h.Host, h.Port))
            .ToArray();

        if (_endpoints.Length == 0)
            throw new ArgumentException("At least one RMQ host must be specified");
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

            _connection = await _factory.CreateConnectionAsync(_endpoints, ct);
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
