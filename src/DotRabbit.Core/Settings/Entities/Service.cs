namespace DotRabbit.Core.Settings.Entities;

public record Service
{
    public string Name { get; }

    public Service(string name)
    {
        Name = name.FromPascalCase();
    }
}


