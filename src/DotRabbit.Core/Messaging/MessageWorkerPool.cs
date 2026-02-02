using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Messaging.Abstract;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed class MessageWorkerPool
    : IAsyncDisposable
{
    private int _started = 0;
    public int WorkerCount { get; }
    public ushort BufferSize { get; }

    private readonly Channel<IMessage> _workersQueue;
    private readonly List<Task> _workersTaskList = [];
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;
    private readonly IMessageToEventTransformer _messageToEventTransformer;
    private readonly IEventProcessorInvoker _eventProcessorInvoker;
    private readonly IMessageRetryPolicy _messageRetryPolicy;

    public MessageWorkerPool(
        ILogger<MessageWorkerPool> logger,
        IMessageToEventTransformer messageToEventTransformer,
        IEventProcessorInvoker eventProcessorInvoker,
        IMessageRetryPolicy messageRetryPolicy,
        int? workerCount, 
        ushort? bufferSize)
    {
        _logger = logger;
        _messageToEventTransformer = messageToEventTransformer;
        _eventProcessorInvoker = eventProcessorInvoker;
        _messageRetryPolicy = messageRetryPolicy;
        WorkerCount = workerCount ?? Environment.ProcessorCount;
        BufferSize = bufferSize ?? (ushort)(WorkerCount * 4);

        _workersQueue = Channel.CreateBounded<IMessage>(BufferSize);
    }

    public void Start()
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
            return;

        try
        {
            StartWorkers(WorkerCount);
        }
        catch
        {
            Interlocked.Exchange(ref _started, 0);
            throw;
        }
    }

    public ValueTask EnqueueAsync(IMessage msg)
    {
        if(!_workersQueue.Writer.TryWrite(msg))
        {
            _logger.LogWarning("Worker queue is full. Blocking enqueue. Tag = {Tag}", msg.DeliveryTag);
            return _workersQueue.Writer.WriteAsync(msg);    
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask StopAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping worker pool...");

        _cts.Cancel();
        _workersQueue.Writer.TryComplete();

        try
        {
            await Task.WhenAll(_workersTaskList)
                .WaitAsync(ct)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Worker pool shutdown cancelled by host");
        }
        finally
        {
            _started = 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private void StartWorkers(int workerCount)
    {
        _logger.LogInformation("Starting WorkerPool with {Workers} workers", workerCount);

        for (int workerId = 0; workerId < workerCount; workerId++)
        {
            var task = Task.Run(() => WorkerSupervisor(workerId));
            _workersTaskList.Add(task);
        }
    }

    private async Task WorkerSupervisor(int workerId)
    {
        var delay = TimeSpan.FromSeconds(1);
        var maxDelay = TimeSpan.FromMinutes(1);

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await WorkerLoop(workerId).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(1);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Worker {WorkerId} crashed. Restarting...", workerId);
                await Task.Delay(delay, _cts.Token);

                delay = TimeSpan.FromSeconds(
                    Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds)
                );
            }
        }
    }

    private async Task WorkerLoop(int workerId)
    {
        _logger.LogInformation("Worker {WorkerId} started", workerId);

        try
        {
            await foreach (var msg in _workersQueue.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    _logger.LogDebug("Worker {WorkerId} is handling message {Tag}", workerId, msg.DeliveryTag);

                    var @event = _messageToEventTransformer.Transform(msg);

                    await _eventProcessorInvoker.InvokeAsync(@event).ConfigureAwait(false);
                    
                    await msg.AckAsync().ConfigureAwait(false);

                    _logger.LogDebug("Worker {WorkerId} ACK message {Tag}", workerId, msg.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker {WorkerId} failed message {Tag}", workerId, msg.DeliveryTag);
                    await _messageRetryPolicy.RetryAsync(msg).ConfigureAwait(false);
                }
            }
        }
        catch(OperationCanceledException)
        {
            _logger.LogInformation("Worker {WorkerId} cancelled", workerId);
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, "Worker {WorkerId} crashed", workerId);
            throw;
        }
        finally
        {
            _logger.LogInformation("Worker {WorkerId} stopped", workerId);
        }
    }
}
