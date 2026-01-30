using DotRabbit.Core.Events.Abstract;

namespace DotRabbit.UnitTests;

public class UserCreatedTestEvent : IEvent
{
    public Guid UserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
