namespace DotRabbit.Core.Settings.Entities;

public record Domain
{
    public string Name { get; }

    public Domain(string name)
    {
        Name = name.FromPascalCase();
    }
}
