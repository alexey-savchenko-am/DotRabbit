using DotRabbit.Core.Eventing.Abstract;
using DotRabbit.Core.Eventing.Entities;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DotRabbit.Core.Eventing;

internal static class EventContainerFactoryBuilder
{
    public static Func<EventContainerData, IEvent, IEventContainer<IEvent>> Build(Type eventType)
    {
        var containerType = typeof(EventContainer<>).MakeGenericType(eventType);
        var ctor = containerType.GetConstructor([typeof(EventContainerData), eventType])!;

        var dataParam = Expression.Parameter(typeof(EventContainerData));
        var eventParam = Expression.Parameter(typeof(IEvent));

        var castEvent = Expression.Convert(eventParam, eventType);

        var newExpr = Expression.New(ctor, dataParam, castEvent);
        var castContainer = Expression.Convert(newExpr, typeof(IEventContainer<IEvent>));

        var lambda = Expression.Lambda<Func<EventContainerData, IEvent, IEventContainer<IEvent>>>(
            castContainer,
            dataParam,
            eventParam);

        return lambda.Compile();
    }
}
