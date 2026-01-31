using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Settings.Abstract;

internal interface ITopologyStrategy
{
    Task<IReadOnlyList<QueueDefinition>> ProvisionTopologyAsync(
        DomainDefinition domain,
        IReadOnlyCollection<EventDefinition> events);
}
