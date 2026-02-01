using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Entities;

public sealed record EventContainerData(
    string Id,
    DomainDefinition Domain,
    IMessage? Message
);
