using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Entities;

public record Listener
{
    public Guid Id { get; }
    public Domain Domain{ get; }

    public Listener(Guid id, Domain domain)
    {
        Id = id;
        Domain = domain;
    }
}
