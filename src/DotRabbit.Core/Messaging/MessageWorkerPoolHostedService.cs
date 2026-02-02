using Microsoft.Extensions.Hosting;

namespace DotRabbit.Core.Messaging;

internal class MessageWorkerPoolHostedService : IHostedService
{
    private readonly MessageWorkerPool _messageWorkerPool;

    public MessageWorkerPoolHostedService(MessageWorkerPool messageWorkerPool)
    {
        _messageWorkerPool = messageWorkerPool;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messageWorkerPool.Start();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _messageWorkerPool.StopAsync(cancellationToken);
    }
}
