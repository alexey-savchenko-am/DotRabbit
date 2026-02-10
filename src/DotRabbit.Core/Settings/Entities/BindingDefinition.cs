namespace DotRabbit.Core.Settings.Entities;

internal sealed record BindingDefinition(string Exchange, string Queue, string RoutingKey);
