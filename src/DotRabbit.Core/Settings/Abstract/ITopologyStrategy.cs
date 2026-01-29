using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings.Abstract;

internal interface ITopologyStrategy
{
    Task<IReadOnlyList<QueueDefinition>> ProvisionTopologyAsync(
        Service service,
        Domain domain,
        IReadOnlyCollection<Event> events);
}
