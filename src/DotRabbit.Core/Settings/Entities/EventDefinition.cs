namespace DotRabbit.Core.Settings.Entities;

public record EventDefinition
{
    public string Name { get; }
    public Type Type { get; }
    public DomainDefinition Domain { get; }

    public EventDefinition(string name, Type type, DomainDefinition domain)
    {
        Name = name.ToKebabNotation();
        Type = type;
        Domain = domain;
    }
}
