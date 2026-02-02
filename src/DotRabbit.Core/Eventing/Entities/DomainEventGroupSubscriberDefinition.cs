using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Entities;

public record DomainEventGroupSubscriberDefinition
{
    public Guid Id { get; }
    public DomainDefinition Domain { get; }

    public DomainEventGroupSubscriberDefinition(Guid id, DomainDefinition domain)
    {
        Id = id;
        Domain = domain;
    }
}
