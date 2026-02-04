using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.UnitTests;

[EventName("user-created")]
public class UserCreatedTestEvent : IEvent
{
    public Guid UserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
