namespace DotRabbit.Core.Settings.Entities;

public record EventDefinition
{
    public string Name { get; }

    public EventDefinition(string name)
    {
        Name = name.ToKebabNotation();
    }
}
