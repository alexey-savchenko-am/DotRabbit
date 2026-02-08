using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.IntegrationTests.EventsAndHandlers;

[EventName("user-updated")]
internal class UserUpdatedFailedEvent : IEvent
{
    public Guid Id { get; set; }
}
