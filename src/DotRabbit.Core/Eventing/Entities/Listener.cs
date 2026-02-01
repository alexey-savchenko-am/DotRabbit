using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Entities;

public record Listener
{
    public Guid Id { get; }
    public DomainDefinition Domain { get; }

    public Listener(Guid id, DomainDefinition domain)
    {
        Id = id;
        Domain = domain;
    }
}
