namespace DotRabbit.Core.Settings.Entities;

public record EventDefinition
{
    public string Name { get; }
    public Type Type { get; }

    public EventDefinition(string name, Type type)
    {
        Name = name.ToKebabNotation();
        Type = type;
    }
}
