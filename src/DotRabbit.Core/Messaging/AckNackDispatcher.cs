using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Messaging.Entities;
using DotRabbit.Core.Settings.Entities;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

/// <summary>
/// Represents a queue-based consumer used to ACK/NACK incoming messages.
/// Instances are created by <see cref="Eventing.RmqEventConsumer"/> per <see cref="RmqMessageConsumer"/>
/// and should not be registered in DI.
/// </summary>
internal sealed class AckNackDispatcher: IAckNackDispatcher
{
    private readonly ILogger<AckNackDispatcher> _logger;
    private readonly IChannel _channel;
    private readonly DomainDefinition _domain;
    private readonly ChannelReader<DeliveryStatus> _reader;
    private readonly CancellationToken _ct;
    private Task? _runTask;
    private int _started = 0;

    public AckNackDispatcher(
        ILogger<AckNackDispatcher> logger,
        IChannel channel,
        DomainDefinition domain,
        Channel<DeliveryStatus> queue,
        CancellationToken ct)
    {
        _logger = logger;
        _channel = channel;
        _domain = domain;
        _reader = queue.Reader;
        _ct = ct;
    }

    public void Start()
    {
        if (Interlocked.CompareExchange(ref  _started, 1, 0) == 1)
            return;   

        try
        {
            _logger.LogInformation(
                "AckNackDispatcher for domain {Domain} is about to start",
                _domain.Name);

            _runTask = Task.Run(RunAsync, _ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception occured while starting AckNackDispatcher for {Domain}",
                _domain.Name);
            Volatile.Write(ref _started, 0);
        }
    }

    /// <summary>
    /// Allows awaiting the dispatcher task but does not actually stop the dispatcher.
    /// The dispatcher is stopped via the provided <see cref="CancellationToken"/>.
    /// </summary>
    public async Task StopAsync()
    {
        if (_runTask is null)
            return;

        try
        {
            await _runTask!.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            //graceful shutdown
        }
    }

    private async Task RunAsync()
    {
        _logger.LogInformation(
            "AckNackDispatcher for domain {Domain} started",
            _domain.Name);

        try
        {
            await foreach (var s in _reader.ReadAllAsync(_ct))
            {
                _ct.ThrowIfCancellationRequested();

                _logger.LogDebug(
                    "Delivery {Status} for tag={Tag} in domain {Domain}",
                    s.Status,
                    s.Tag,
                    _domain.Name);

                switch (s.Status)
                {
                    case DeliveryStatusCode.Ack:
                        await _channel.BasicAckAsync(
                            s.Tag,
                            multiple: false,
                            cancellationToken: _ct);
                        break;

                    case DeliveryStatusCode.Nack:
                        await _channel.BasicNackAsync(
                            s.Tag,
                            multiple: false,
                            requeue: false,
                            cancellationToken: _ct);
                        break;

                    case DeliveryStatusCode.Reject:
                        await _channel.BasicRejectAsync(
                            s.Tag,
                            requeue: false,
                            cancellationToken: _ct);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unknown delivery status: {s.Status}");
                }
            }
        }
        catch (OperationCanceledException) when (_ct.IsCancellationRequested)
        {
            _logger.LogInformation(
                "AckNackDispatcher for domain {Domain} stopped",
                _domain.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "AckNackDispatcher for domain {Domain} crashed",
                _domain.Name);

            throw; 
        }
    }
}
