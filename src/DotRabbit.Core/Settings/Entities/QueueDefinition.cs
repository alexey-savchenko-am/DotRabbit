namespace DotRabbit.Core.Settings.Entities;

public record QueueDefinition
{
    public static QueueDefinition DefineLive(string name) => Create(name, "live");  
    public static QueueDefinition DefineRetry(string name) => Create(name, "retry");  
    public static QueueDefinition DefineDead(string name) => Create(name, "dead");
    public bool IsLiveDefinition => Type.Equals("live", StringComparison.OrdinalIgnoreCase);

    public string Name { get; }
    public string Type { get; }

    private QueueDefinition(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public static QueueDefinition Create(string name, string type)
    {
        return new QueueDefinition(name, type.ToLowerInvariant());
    }
}


