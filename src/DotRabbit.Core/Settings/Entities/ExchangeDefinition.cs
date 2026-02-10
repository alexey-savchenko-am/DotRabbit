
namespace DotRabbit.Core.Settings.Entities;

public record ExchangeDefinition
{
    public static ExchangeDefinition DefineLive(string name, string topologyType) => 
        Create(name, "live", topologyType);
    public static ExchangeDefinition DefineRetry(string name, string topologyType) => 
        Create(name, "retry", topologyType);
    public static ExchangeDefinition DefineDead(string name, string topologyType) => 
        Create(name, "dead", topologyType);

    public bool IsLiveDefinition => Type.Equals("live", StringComparison.OrdinalIgnoreCase);

    public string Name { get; }
    public string Type { get; }
    public string TopologyType { get; }

    private ExchangeDefinition(string name, string type, string topologyType)
    {
        Name = name;
        Type = type;
        TopologyType = topologyType;
    }

    public static ExchangeDefinition Create(string name, string type, string topologyType)
    {
        return new ExchangeDefinition(name, type.ToLowerInvariant(), topologyType);
    }
}
