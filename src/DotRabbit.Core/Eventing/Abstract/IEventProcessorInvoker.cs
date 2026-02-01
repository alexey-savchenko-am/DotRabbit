namespace DotRabbit.Core.Eventing.Abstract;

public interface IEventProcessorInvoker
{
    Task InvokeAsync(IEventContainer<IEvent> @event);
}
