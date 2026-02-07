using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;
namespace DotRabbit.Core.Settings.Topology;

internal class RmqEventPerQueueTopologyStrategy
    : ITopologyStrategy
{
    private readonly RmqTopologyManager _topologyManager;
    private readonly ITopologyResolver _topologyResolver;

    public RmqEventPerQueueTopologyStrategy(
        RmqTopologyManager topologyManager,
        ITopologyResolver topologyResolver)
    {
        _topologyManager = topologyManager;
        _topologyResolver = topologyResolver;
    }

    /*
        Publisher
           ↓ publish (routingKey = user-created)
        user.users.topic (exchange)
           ↓
        user.users.user-created.q  <-- Consumer

        ❌ Exception → NACK(requeue:false)
           ↓ DLX
        user.users.retry (exchange)
           ↓
        user.users.user-created.retry (TTL = 10s)
           ↓ DLX after TTL
        user.users.topic
           ↓
        user.users.user-created.q (retry attempt)

        I have to store retry count in message headers by myself to avoid the dead circle.
     */

    public async Task<IReadOnlyList<QueueDefinition>> ProvisionTopologyAsync(
        DomainDefinition domain,
        IReadOnlyCollection<EventDefinition> events)
    {
        var result = new List<QueueDefinition>();

        // ex. user.users.topic
        var exchange = _topologyResolver.ResolveExchange(domain);
        // ex. user.users.retry
        var retryExchange = _topologyResolver.ResolveRetryExchange(domain);
        // ex. user.users.dlx
        var dlx = _topologyResolver.ResolveDlxExchange(domain);

        await _topologyManager.DeclareExchangeAsync(exchange, ExchangeType.Topic);
        await _topologyManager.DeclareExchangeAsync(retryExchange, ExchangeType.Direct);
        await _topologyManager.DeclareExchangeAsync(dlx, ExchangeType.Fanout);

        foreach (var e in events)
        {
            // ex. user.users.user-created.q
            var queue = _topologyResolver.ResolveQueue(domain, e);
            // ex. user.users.user-created.retry
            var retryQueue = _topologyResolver.ResolveRetryQueue(domain, e);
            // ex. user.users.user-created.dlq
            var dlq = _topologyResolver.ResolveDlqQueue(domain, e);

            // MAIN QUEUE
            await _topologyManager.DeclareQueueAsync(queue, new QueueOptions
            {
                DeadLetterExchange = retryExchange,
                DeadLetterRoutingKey = e.Name
            });

            // RETRY
            await _topologyManager.DeclareQueueAsync(retryQueue, new QueueOptions
            {
                MessageTtl = TimeSpan.FromSeconds(10),
                DeadLetterExchange = exchange,
                DeadLetterRoutingKey = e.Name
            });

            // DLQ
            await _topologyManager.DeclareQueueAsync(dlq);

            // BINDING
            await _topologyManager.BindQueueAsync(queue, exchange, routingKey: e.Name);
            await _topologyManager.BindQueueAsync(retryQueue, retryExchange, routingKey: e.Name);
            await _topologyManager.BindQueueAsync(queue: dlq, exchange: dlx, routingKey: ""); // routingKey is ""

            result.Add(QueueDefinition.DefineLive(queue));
            result.Add(QueueDefinition.DefineRetry(retryQueue));
            result.Add(QueueDefinition.DefineDead(dlq));
        }

        return result;
    }
}
