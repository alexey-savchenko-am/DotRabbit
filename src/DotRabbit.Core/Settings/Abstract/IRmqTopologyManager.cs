using DotRabbit.Core.Settings.Entities;
using RabbitMQ.Client;

namespace DotRabbit.Core.Settings.Abstract;

public interface IRmqTopologyManager
{
    Task DeclareExchangeAsync(string name, string type = ExchangeType.Topic, CancellationToken ct = default);
    Task DeclareQueueAsync(string name, QueueOptions? options = null, CancellationToken ct = default);
    Task BindQueueAsync(string queue, string exchange, string routingKey, CancellationToken ct = default);
}