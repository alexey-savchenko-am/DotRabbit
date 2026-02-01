using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventTypeRegistry
{
    bool Register(EventDefinition @event);

    public Type GetByName(string eventName);
}
