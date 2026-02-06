using DotRabbit.Core.Eventing;
using DotRabbit.Core.Eventing.Abstract;

namespace DotRabbit.IntegrationTests;

[EventName("user-created")]
public class UserCreatedTestEvent : IEvent
{
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
