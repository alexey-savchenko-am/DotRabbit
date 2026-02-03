using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Configuration.Builders;

public sealed record EventProcessorRegistration(
    DomainDefinition Domain,
    Type EventType,
    Type HandlerType
);