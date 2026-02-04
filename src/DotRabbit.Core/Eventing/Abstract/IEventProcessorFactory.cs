using DotRabbit.Core.Settings.Entities;

namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventProcessorFactory
{
    DomainDefinition Domain { get; }

    IEventProcessor Resolve(Type eventType, IServiceProvider serviceProvider);
}
