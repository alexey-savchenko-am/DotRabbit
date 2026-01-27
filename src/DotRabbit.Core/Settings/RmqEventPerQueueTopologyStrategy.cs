using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;
namespace DotRabbit.Core.Settings;

internal class RmqEventPerQueueTopologyStrategy
    : ITopologyStrategy
{
    private readonly RmqTopologyManager _topologyManager;

    public RmqEventPerQueueTopologyStrategy(RmqTopologyManager topologyManager)
    {
        _topologyManager = topologyManager;
    }

    public async Task<IReadOnlyList<Queue>> ProvisionTopologyAsync(
        Service service, 
        Domain domain, 
        IReadOnlyCollection<Event> events)
    {
        var result = new List<Queue>();

        var exchange = $"{service.Name}.{domain.Name}.topic";
        var dlx = $"{service.Name}.{domain.Name}.dlx";

        await _topologyManager.DeclareExchangeAsync(exchange, ExchangeType.Topic);
        await _topologyManager.DeclareExchangeAsync(dlx, ExchangeType.Fanout);

        foreach (var e in events)
        {
            var queue = $"{service.Name}.{domain.Name}.{e.Name}.q";
            var dlq = $"{service.Name}.{domain.Name}.{e.Name}.dlq";

            await _topologyManager.DeclareQueueAsync(queue, new QueueOptions
            {
                DeadLetterExchange = dlx
            });

            await _topologyManager.DeclareQueueAsync(dlq);

            await _topologyManager.BindQueueAsync(queue, exchange, e.Name);

            // dlx is Fanout, so bindingKey is ""
            await _topologyManager.BindQueueAsync(dlq, dlx, "");

            result.Add(Queue.Live(queue));
            result.Add(Queue.Dead(dlq));
        }

        return result;
    }
}
