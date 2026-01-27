using DotRabbit.Abstractions;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace DotRabbit.Core.Connection;

internal class RmqChannelPool
    : IAsyncDisposable
{
    private readonly IRmqConnectionFactory _connectionFactory;
    private readonly ConcurrentBag<IChannel> _channels = new();
    private readonly SemaphoreSlim _semaphore;

    public RmqChannelPool(IRmqConnectionFactory connectionFactory, int size = 32)
    {
        _connectionFactory = connectionFactory;
        _semaphore = new SemaphoreSlim(size, size);
    }

    public async ValueTask<IChannel> RentAsync()
    {
        await _semaphore.WaitAsync();

        if (_channels.TryTake(out var ch) && ch.IsOpen)
            return ch;

        var conn = await _connectionFactory.GetConnectionAsync();
        return await conn.CreateChannelAsync();
    }

    public void Return(IChannel channel)
    {
        if (channel.IsOpen)
            _channels.Add(channel);
        else
            channel.Dispose();

        _semaphore.Release();
    }

    public async ValueTask DisposeAsync()
    {
        while (_channels.TryTake(out var ch))
            await ch.DisposeAsync();
    }
}
