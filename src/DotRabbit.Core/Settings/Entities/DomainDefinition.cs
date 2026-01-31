namespace DotRabbit.Core.Settings.Entities;

public record DomainDefinition
{
    public string Name { get; }

    public DomainDefinition(string name)
    {
        Name = name.ToDottedNotation();
    }
}
