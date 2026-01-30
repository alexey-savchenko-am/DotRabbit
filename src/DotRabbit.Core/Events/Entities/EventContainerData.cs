using DotRabbit.Core.Messaging.Abstract;
using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Events.Entities;

public sealed record EventContainerData(
    string Id,
    Domain Domain,
    string? Error,
    IMessage? Message
);
