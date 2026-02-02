using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;

namespace DotRabbit.Core.Messaging.Entities;

public sealed class ConsumerSubscription
    : IAsyncDisposable
{
    private int _unsubscribed;
    public QueueDefinition Queue { get; }
    public IChannel Channel { get; }
    public string Tag { get; }
    public bool IsFaulted { get; private set; }

    public bool IsHealthy => Channel.IsOpen && !IsFaulted;

    public ConsumerSubscription(
        QueueDefinition queue,
        IChannel channel,
        string tag)
    {
        Queue = queue;
        Channel = channel;
        Tag = tag;
    }

    public void MakeFaulted() => IsFaulted = true;  

    public async ValueTask UnsubscribeAsync()
    {
        if (Interlocked.Exchange(ref _unsubscribed, 1) == 1)
            return;

        try
        {
            if (Channel.IsOpen)
                await Channel.BasicCancelAsync(Tag);
        }
        catch { /* shutdown race */ }

        try
        {
            await Channel.CloseAsync();
        }
        catch { }

        await Channel.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeAsync();
    }
}
