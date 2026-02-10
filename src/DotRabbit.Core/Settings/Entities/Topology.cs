
namespace DotRabbit.Core.Settings.Entities;

internal sealed class TopologyDefinition
{
    public IReadOnlyCollection<ExchangeDefinition> Exchanges { get; }
    public IReadOnlyCollection<QueueDefinition> Queues { get; }
    public IReadOnlyCollection<BindingDefinition> Bindings { get; }

    public TopologyDefinition(
        IEnumerable<ExchangeDefinition> exchanges,
        IEnumerable<QueueDefinition> queues,
        IEnumerable<BindingDefinition> bindings)
    {
        Exchanges = exchanges.ToArray();
        Queues = queues.ToArray();
        Bindings = bindings.ToArray();
    }
}