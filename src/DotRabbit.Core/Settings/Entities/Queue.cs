namespace DotRabbit.Core.Settings.Entities;

public record Queue
{
    public static Queue Live(string name) => Create(name, "Live");  
    public static Queue Dead(string name) => Create(name, "Dead");
    public bool IsDead => Name.Equals("dead", StringComparison.OrdinalIgnoreCase);
    public string Name { get; }
    public string Type { get; }

    private Queue(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public static Queue Create(string name, string type)
    {
        return new Queue(name.FromPascalCase(), type.ToLowerInvariant());
    }
}


