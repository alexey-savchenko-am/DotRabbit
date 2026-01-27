namespace DotRabbit.Core.Settings.Entities;

public record Event
{
    public string Name { get; }

    public string Domain => Name.Split('.').First();

    public Event(string name)
    {
        Name = name.FromPascalCase();
    }
}
