using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DotRabbit.Core.Messaging;

internal sealed class MessageWorkerPool
    : IAsyncDisposable
{
    public int WorkerCount { get; }
    public ushort BufferSize { get; }

    private readonly Channel<Message> _workersQueue;
    private readonly List<Task> _workersTaskList = [];
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;

    public MessageWorkerPool(
        ILogger<MessageWorkerPool> logger, 
        int? workerCount, 
        ushort? bufferSize)
    {
        _logger = logger;
        WorkerCount = workerCount ?? Environment.ProcessorCount;
        BufferSize = bufferSize ?? (ushort)(WorkerCount * 4);

        _workersQueue = Channel.CreateBounded<Message>(BufferSize);

        StartWorkers(WorkerCount);
    }

    public ValueTask EnqueueAsync(Message msg)
    {
        if(!_workersQueue.Writer.TryWrite(msg))
        {
            _logger.LogWarning("Worker queue is full. Blocking enqueue. Tag = {Tag}", msg.DeliveryTag);
            return _workersQueue.Writer.WriteAsync(msg);    
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask StopAsync()
    {
        _logger.LogInformation("Stopping worker pool...");

        _cts.Cancel();

        _workersQueue.Writer.TryComplete();
        await Task.WhenAll(_workersTaskList); 
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
            var task = Task.Run(() => WorkerLoop(workerId));
            task.ContinueWith(async t =>
            {
                _logger.LogCritical(t.Exception, "Worker {WorkerId} crashed.", workerId);

                if (!_cts.IsCancellationRequested)
                {
                    // trying to restart the Worker if it's failed to run 
                    await Task.Delay(1_000);
                    await WorkerLoop(workerId);
                }
                    
            }, TaskContinuationOptions.OnlyOnFaulted);

            _workersTaskList.Add(task);
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
                    _logger.LogDebug("Worker {WorkerId} handling message {Tag}", workerId, msg.DeliveryTag);

                    _logger.LogDebug("Worker {WorkerId} ACK message {Tag}", workerId, msg.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker {WorkerId} failed message {Tag}", workerId, msg.DeliveryTag);
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
