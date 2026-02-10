using DotRabbit.Core.Settings.Abstract;
using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;
namespace DotRabbit.Core.Settings.Topology;

internal class RmqDomainQueueWithEventRoutingTopologyStrategy
    : ITopologyStrategy
{
    private readonly IRmqTopologyManager _topologyManager;
    private readonly ITopologyResolver _topologyResolver;

    public RmqDomainQueueWithEventRoutingTopologyStrategy(
        IRmqTopologyManager topologyManager,
        ITopologyResolver topologyResolver)
    {
        _topologyManager = topologyManager;
        _topologyResolver = topologyResolver;
    }

    public async Task<TopologyDefinition> ProvisionTopologyAsync(
        DomainDefinition domain,
        IReadOnlyCollection<EventDefinition> events)
    {
        var queues = new List<QueueDefinition>(3);
        var exchanges = new List<ExchangeDefinition>(3);
        var bindings = new List<BindingDefinition>(events.Count * 3);

        // ex. user.users.topic
        var exchange = _topologyResolver.ResolveExchange(domain);
        // ex. user.users.retry
        var retryExchange = _topologyResolver.ResolveRetryExchange(domain);
        // ex. user.users.dlx
        var dlx = _topologyResolver.ResolveDlxExchange(domain);

        await _topologyManager.DeclareExchangeAsync(exchange, ExchangeType.Topic);
        await _topologyManager.DeclareExchangeAsync(retryExchange, ExchangeType.Direct);
        await _topologyManager.DeclareExchangeAsync(dlx, ExchangeType.Fanout);

        exchanges.Add(ExchangeDefinition.DefineLive(exchange, ExchangeType.Topic));
        exchanges.Add(ExchangeDefinition.DefineRetry(retryExchange, ExchangeType.Direct));
        exchanges.Add(ExchangeDefinition.DefineDead(dlx, ExchangeType.Fanout));
     
        // ex. user.users.q
        var queue = _topologyResolver.ResolveQueue(domain);
        // ex. user.users.retry
        var retryQueue = _topologyResolver.ResolveRetryQueue(domain);
        // ex. user.users.dlq
        var dlq = _topologyResolver.ResolveDlqQueue(domain);

        // MAIN QUEUE
        await _topologyManager.DeclareQueueAsync(queue, new QueueOptions
        {
            DeadLetterExchange = retryExchange,
        });

        // RETRY
        await _topologyManager.DeclareQueueAsync(retryQueue, new QueueOptions
        {
            MessageTtl = TimeSpan.FromSeconds(10),
            DeadLetterExchange = exchange,
        });

        // DLQ
        await _topologyManager.DeclareQueueAsync(dlq);

        queues.Add(QueueDefinition.DefineLive(queue));
        queues.Add(QueueDefinition.DefineRetry(retryQueue));
        queues.Add(QueueDefinition.DefineDead(dlq));

        foreach (var e in events)
        {
            // bind exchange with queue with event-name as a routing key
            await _topologyManager.BindQueueAsync(queue, exchange, routingKey: e.Name);
            await _topologyManager.BindQueueAsync(retryQueue, retryExchange, routingKey: e.Name);
            await _topologyManager.BindQueueAsync(queue: dlq, exchange: dlx, routingKey: ""); // routingKey is ""
            bindings.Add(new BindingDefinition(exchange, queue, e.Name));
            bindings.Add(new BindingDefinition(retryExchange, retryQueue, e.Name));
            bindings.Add(new BindingDefinition(exchange, queue, e.Name));
        }

        return new TopologyDefinition(exchanges, queues, bindings);
    }
}
