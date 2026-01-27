
using RabbitMQ.Client;

namespace DotRabbit.Core.Connection;

internal sealed class RmqChannelLease
    : IAsyncDisposable
{
    private readonly RmqChannelPool _pool;
    public IChannel Channel { get; }

    private RmqChannelLease(RmqChannelPool pool, IChannel channel)
    {
        _pool = pool;
        Channel = channel;
    }

    public static async ValueTask<RmqChannelLease> CreateAsync(RmqChannelPool pool)
    {
        var channel = await pool.RentAsync();
        return new RmqChannelLease(pool, channel);
    }

    public ValueTask DisposeAsync()
    {
        _pool.Return(Channel);
        return ValueTask.CompletedTask;
    }
}
