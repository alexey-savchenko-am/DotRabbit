using DotRabbit.Core.Messaging.Abstract;
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
    private IAckNackDispatcher AckNackDispatcher { get; }
    public bool IsFaulted { get; private set; }

    public bool IsHealthy => Channel.IsOpen && !IsFaulted;

    public ConsumerSubscription(
        QueueDefinition queue,
        IChannel channel,
        IAckNackDispatcher ackNackDispatcher,
        string tag)
    {
        Queue = queue;
        Channel = channel;
        AckNackDispatcher = ackNackDispatcher;
        Tag = tag;
    }

    public void MakeFaulted() => IsFaulted = true;

    public async ValueTask UnsubscribeAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _unsubscribed, 1) == 1)
            return;

        ct.ThrowIfCancellationRequested();

        try
        {
            if (Channel.IsOpen)
                await Channel.BasicCancelAsync(Tag, noWait: false, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // shutdown race
        }

        ct.ThrowIfCancellationRequested();

        try
        {
            await Channel.CloseAsync(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
        }

        await Channel.DisposeAsync();
        // wait until dispatcher is stopped
        //await AckNackDispatcher.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeAsync();
    }
}
