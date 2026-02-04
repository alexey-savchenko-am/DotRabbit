using DotRabbit.Core.Settings;

namespace DotRabbit.Core.Eventing;

[AttributeUsage(AttributeTargets.Class)]
public class EventNameAttribute
    : Attribute
{
    public string EventName { get; }
    public EventNameAttribute(string eventName)
    {
        EventName = eventName.ToKebabNotation();
    }
}
