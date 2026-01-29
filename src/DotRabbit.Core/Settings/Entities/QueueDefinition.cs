namespace DotRabbit.Core.Settings.Entities;

public record QueueDefinition
{
    public static QueueDefinition Live(string name) => Create(name, "Live");  
    public static QueueDefinition Dead(string name) => Create(name, "Dead");
    public bool IsDead => Name.Equals("dead", StringComparison.OrdinalIgnoreCase);
    public string Name { get; }
    public string Type { get; }

    private QueueDefinition(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public static QueueDefinition Create(string name, string type)
    {
        return new QueueDefinition(name.FromPascalCase(), type.ToLowerInvariant());
    }
}


